using System.Data;
using System.Linq.Expressions;
using GestionIndemnite.Application.RHOrganisation.Common;
using GestionIndemnite.Domain.RHOrganisation.Common;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GestionIndemnite.Application.RHOrganisation.Services;

public record PeriodeGap(DateOnly DateDebut, DateOnly DateFin);

public class PeriodeService
{
    private readonly IRHOrganisationDbContext _db;

    public PeriodeService(IRHOrganisationDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> AddAsync<T>(
        DbSet<T> set,
        T candidate,
        Expression<Func<T, bool>> sameKey,
        string? lockKey = null,
        CancellationToken ct = default)
        where T : AuditableEntity, IPeriode
    {
        ValidatePeriode(candidate);

        var ownsTransaction = !_db.HasActiveTransaction;
        var transaction = ownsTransaction
            ? await _db.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct)
            : null;

        try
        {
            if (lockKey is not null)
                await _db.AcquireKeyLockAsync(lockKey, ct);

            var others = await set.AsNoTracking()
                .Where(sameKey)
                .Where(e => e.Id != candidate.Id)
                .Select(e => new { e.DateDebut, e.DateFin })
                .ToListAsync(ct);

            CheckChevauchement(candidate, others.Select(o => (o.DateDebut, o.DateFin)).ToList());

            set.Add(candidate);

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                throw new RegleMetierException("L'enregistrement a été modifié pendant la saisie. Veuillez réessayer.");
            }

            if (ownsTransaction)
                await transaction!.CommitAsync(ct);

            return candidate.Id;
        }
        finally
        {
            if (transaction is not null)
                await transaction.DisposeAsync();
        }
    }

    public async Task<Guid> CloseAndAddAsync<T>(
        DbSet<T> set,
        T candidate,
        Expression<Func<T, bool>> sameKey,
        string? lockKey = null,
        CancellationToken ct = default)
        where T : AuditableEntity, IPeriode
    {
        ValidatePeriode(candidate);

        var ownsTransaction = !_db.HasActiveTransaction;
        var transaction = ownsTransaction
            ? await _db.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct)
            : null;

        try
        {
            if (lockKey is not null)
                await _db.AcquireKeyLockAsync(lockKey, ct);

            var existing = await set.AsNoTracking()
                .Where(sameKey)
                .Where(e => e.Id != candidate.Id)
                .Select(e => new { e.Id, e.DateDebut, e.DateFin })
                .ToListAsync(ct);

            var openOnes = existing.Where(e => e.DateFin is null).ToList();
            if (openOnes.Count > 1)
                throw new RegleMetierException("Il existe plusieurs enregistrements ouverts pour la même clé.");

            var closed = existing.Where(e => e.DateFin is not null).ToList();

            if (openOnes.Count == 1)
            {
                var open = openOnes[0];
                if (candidate.DateDebut <= open.DateDebut)
                    throw new RegleMetierException("La date de début de la nouvelle période doit être postérieure à la date de début de la période en cours.");

                var tracked = await set.FirstAsync(e => e.Id == open.Id, ct);
                tracked.DateFin = candidate.DateDebut.AddDays(-1);
            }

            CheckChevauchement(candidate, closed.Select(o => (o.DateDebut, o.DateFin)).ToList());

            set.Add(candidate);

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                throw new RegleMetierException("L'enregistrement a été modifié pendant la saisie. Veuillez réessayer.");
            }

            if (ownsTransaction)
                await transaction!.CommitAsync(ct);

            return candidate.Id;
        }
        finally
        {
            if (transaction is not null)
                await transaction.DisposeAsync();
        }
    }

    public async Task UpdateAsync<T>(
        DbSet<T> set,
        T entity,
        Expression<Func<T, bool>> sameKey,
        string? lockKey = null,
        CancellationToken ct = default)
        where T : AuditableEntity, IPeriode
    {
        ValidatePeriode(entity);

        var ownsTransaction = !_db.HasActiveTransaction;
        var transaction = ownsTransaction
            ? await _db.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct)
            : null;

        try
        {
            if (lockKey is not null)
                await _db.AcquireKeyLockAsync(lockKey, ct);

            var others = await set.AsNoTracking()
                .Where(sameKey)
                .Where(e => e.Id != entity.Id)
                .Select(e => new { e.DateDebut, e.DateFin })
                .ToListAsync(ct);

            CheckChevauchement(entity, others.Select(o => (o.DateDebut, o.DateFin)).ToList());

            try
            {
                await _db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                throw new RegleMetierException("L'enregistrement a été modifié pendant la saisie. Veuillez réessayer.");
            }

            if (ownsTransaction)
                await transaction!.CommitAsync(ct);
        }
        finally
        {
            if (transaction is not null)
                await transaction.DisposeAsync();
        }
    }

    public async Task<IReadOnlyList<PeriodeGap>> GetGapsAsync<T>(
        DbSet<T> set,
        Expression<Func<T, bool>> sameKey,
        CancellationToken ct = default)
        where T : AuditableEntity, IPeriode
    {
        var records = await set.AsNoTracking()
            .Where(sameKey)
            .OrderBy(e => e.DateDebut)
            .Select(e => new { e.DateDebut, e.DateFin })
            .ToListAsync(ct);

        var gaps = new List<PeriodeGap>();

        for (var i = 1; i < records.Count; i++)
        {
            var prev = records[i - 1];
            var next = records[i];

            if (prev.DateFin is null)
                break;

            if (next.DateDebut > prev.DateFin.Value.AddDays(1))
                gaps.Add(new PeriodeGap(prev.DateFin.Value.AddDays(1), next.DateDebut.AddDays(-1)));
        }

        return gaps;
    }

    private static void ValidatePeriode(IPeriode candidate)
    {
        if (candidate.DateFin is not null && candidate.DateDebut > candidate.DateFin)
            throw new RegleMetierException("La date de début doit précéder la date de fin.");
    }

    private static void CheckChevauchement<T>(
        T candidate,
        IReadOnlyList<(DateOnly DateDebut, DateOnly? DateFin)> others)
        where T : IPeriode
    {
        var candidateFin = candidate.DateFin ?? DateOnly.MaxValue;

        foreach (var other in others)
        {
            var otherFin = other.DateFin ?? DateOnly.MaxValue;

            if (candidate.DateDebut <= otherFin && other.DateDebut <= candidateFin)
                throw new RegleMetierException("Une période chevauche un enregistrement existant.");
        }

        if (candidate.DateFin is null && others.Any(o => o.DateFin is null))
            throw new RegleMetierException("Un autre enregistrement ouvert existe déjà pour la même clé.");
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        var msg = ex.InnerException?.Message ?? string.Empty;
        return msg.Contains("duplicate key", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("UNIQUE KEY constraint", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("2601")
            || msg.Contains("2627");
    }
}
