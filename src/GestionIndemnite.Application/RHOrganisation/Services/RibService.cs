using System.Data;
using GestionIndemnite.Application.RHOrganisation.Common;
using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Domain.RHOrganisation.Entities;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using GestionIndemnite.Domain.RHOrganisation.Rules;
using Microsoft.EntityFrameworkCore;

namespace GestionIndemnite.Application.RHOrganisation.Services;

public class RibService
{
    private readonly IRHOrganisationDbContext _db;

    public RibService(IRHOrganisationDbContext db)
    {
        _db = db;
    }

    public async Task<RibDto> ChangeActiveAsync(
        Guid idFonctionnaire,
        CreateRibCommand cmd,
        CancellationToken ct = default)
    {
        RibRules.Validate(cmd.CodeBanque, cmd.CodeVille, cmd.NumeroCompte, cmd.Cle);

        var fonctionnaire = await _db.Fonctionnaires
            .FirstOrDefaultAsync(f => f.Id == idFonctionnaire, ct)
            ?? throw new RegleMetierException("الموظف غير موجود");

        var ownsTransaction = !_db.HasActiveTransaction;
        var transaction = ownsTransaction
            ? await _db.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct)
            : null;

        try
        {
            var currentActive = await _db.Ribs
                .FirstOrDefaultAsync(r => r.IdFonctionnaire == fonctionnaire.Id && r.EstActif, ct);

            if (currentActive is not null)
            {
                currentActive.EstActif = false;
            }

            var newRib = new Rib
            {
                Id = Guid.NewGuid(),
                IdFonctionnaire = fonctionnaire.Id,
                CodeBanque = cmd.CodeBanque,
                CodeVille = cmd.CodeVille,
                NumeroCompte = cmd.NumeroCompte,
                Cle = cmd.Cle,
                EstActif = true
            };

            _db.Ribs.Add(newRib);

            await _db.SaveChangesAsync(ct);

            if (ownsTransaction)
                await transaction!.CommitAsync(ct);

            return ToDto(newRib);
        }
        finally
        {
            if (transaction is not null)
                await transaction.DisposeAsync();
        }
    }

    public async Task<RibDto?> GetActiveAsync(Guid idFonctionnaire, CancellationToken ct = default)
    {
        var rib = await _db.Ribs.AsNoTracking()
            .FirstOrDefaultAsync(r => r.IdFonctionnaire == idFonctionnaire && r.EstActif, ct);

        return rib is null ? null : ToDto(rib);
    }

    public async Task<IReadOnlyList<RibDto>> ListHistoryAsync(
        Guid idFonctionnaire,
        CancellationToken ct = default)
    {
        var list = await _db.Ribs.AsNoTracking()
            .Where(r => r.IdFonctionnaire == idFonctionnaire)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(ct);

        return list.Select(ToDto).ToList();
    }

    private static RibDto ToDto(Rib r) =>
        new(r.Id, r.IdFonctionnaire, r.CodeBanque, r.CodeVille, r.NumeroCompte, r.Cle, r.EstActif);
}
