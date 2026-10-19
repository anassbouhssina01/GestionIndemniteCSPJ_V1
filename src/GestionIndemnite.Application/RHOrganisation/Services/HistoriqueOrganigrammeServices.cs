using GestionIndemnite.Application.RHOrganisation.Common;
using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Domain.RHOrganisation.Entities;
using GestionIndemnite.Domain.RHOrganisation.Enums;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using GestionIndemnite.Domain.RHOrganisation.Rules;
using Microsoft.EntityFrameworkCore;

namespace GestionIndemnite.Application.RHOrganisation.Services;

public class HistoriqueOrganigrammeTribunalService
{
    private readonly IRHOrganisationDbContext _db;
    private readonly PeriodeService _periodes;

    public HistoriqueOrganigrammeTribunalService(
        IRHOrganisationDbContext db,
        PeriodeService periodes)
    {
        _db = db;
        _periodes = periodes;
    }

    public async Task<Guid> ChangeParentAsync(
        Guid idTribunal,
        ChangeParentTribunalCommand cmd,
        CancellationToken ct = default)
    {
        var tribunal = await _db.Tribunaux
            .FirstOrDefaultAsync(t => t.Id == idTribunal, ct)
            ?? throw new RegleMetierException("المحكمة غير موجودة");

        NiveauJuridiction? parentLevel = null;
        if (cmd.IdTribunalParent.HasValue)
        {
            if (cmd.IdTribunalParent == idTribunal)
                throw new RegleMetierException("المحكمة لا يمكن أن تكون تابعة لنفسها");

            var parent = await _db.Tribunaux
                .FirstOrDefaultAsync(t => t.Id == cmd.IdTribunalParent.Value, ct)
                ?? throw new RegleMetierException("المحكمة الأم غير موجودة");
            parentLevel = parent.NiveauJuridiction;
        }

        OrganigrammeRules.ValidateTribunalParent(tribunal.NiveauJuridiction, parentLevel);

        if (await IsCycleAsync(idTribunal, cmd.IdTribunalParent, cmd.DateDebut, ct))
            throw new RegleMetierException(
                "تمييز في الهيكلة: المحكمة لا يمكن أن تكون تابعة لمن هو تابع لها");

        if (cmd.IdDocument.HasValue &&
            !await _db.PieceJointes.AnyAsync(p => p.Id == cmd.IdDocument.Value, ct))
            throw new RegleMetierException("المستند غير موجود");

        var entity = new HistoriqueOrganigrammeTribunal
        {
            Id = Guid.NewGuid(),
            IdTribunal = idTribunal,
            IdTribunalParent = cmd.IdTribunalParent,
            DateDebut = cmd.DateDebut,
            DateFin = null,
            IdDocument = cmd.IdDocument
        };

        return await _periodes.CloseAndAddAsync(
            _db.HistoriqueOrganigrammeTribunalSet,
            entity,
            e => e.IdTribunal == idTribunal,
            $"rh:periode:HistOrgTribunal:{idTribunal}",
            ct);
    }

    public async Task<IReadOnlyList<ParentOrganigrammeDto>> ListHistoryAsync(
        Guid idTribunal,
        CancellationToken ct = default)
    {
        var records = await _db.HistoriqueOrganigrammeTribunalSet.AsNoTracking()
            .Where(h => h.IdTribunal == idTribunal)
            .OrderByDescending(h => h.DateDebut)
            .ToListAsync(ct);

        var result = new List<ParentOrganigrammeDto>();
        foreach (var record in records)
        {
            if (record.IdTribunalParent is null)
            {
                result.Add(new ParentOrganigrammeDto(Guid.Empty, string.Empty, record.DateDebut, record.DateFin));
                continue;
            }

            var parent = await _db.Tribunaux.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == record.IdTribunalParent, ct);
            result.Add(new ParentOrganigrammeDto(
                parent?.Id ?? Guid.Empty,
                parent?.NomTribunal ?? string.Empty,
                record.DateDebut,
                record.DateFin));
        }

        return result;
    }

    private async Task<bool> IsCycleAsync(
        Guid idTribunal,
        Guid? parentId,
        DateOnly date,
        CancellationToken ct)
    {
        if (parentId is null)
            return false;

        var visited = new HashSet<Guid> { idTribunal };
        var current = parentId;
        var guard = 0;

        while (current is not null && guard++ < 100)
        {
            if (!visited.Add(current.Value))
                return true;

            var record = await _db.HistoriqueOrganigrammeTribunalSet.AsNoTracking()
                .Where(h => h.IdTribunal == current.Value
                    && h.DateDebut <= date
                    && (h.DateFin == null || h.DateFin >= date))
                .OrderByDescending(h => h.DateDebut)
                .FirstOrDefaultAsync(ct);

            current = record?.IdTribunalParent;
        }

        return false;
    }
}

public class HistoriqueOrganigrammeUniteService
{
    private readonly IRHOrganisationDbContext _db;
    private readonly PeriodeService _periodes;

    public HistoriqueOrganigrammeUniteService(
        IRHOrganisationDbContext db,
        PeriodeService periodes)
    {
        _db = db;
        _periodes = periodes;
    }

    public async Task<Guid> ChangeParentAsync(
        Guid idUnite,
        ChangeParentUniteCommand cmd,
        CancellationToken ct = default)
    {
        var unite = await _db.UnitesOrganisationnelles
            .FirstOrDefaultAsync(u => u.Id == idUnite, ct)
            ?? throw new RegleMetierException("الهيكل غير موجود");

        if (cmd.IdUniteParent.HasValue)
        {
            if (cmd.IdUniteParent == idUnite)
                throw new RegleMetierException("الهيكل لا يمكن أن يكون تابعا لنفسه");

            if (!await _db.UnitesOrganisationnelles.AnyAsync(
                    u => u.Id == cmd.IdUniteParent.Value, ct))
                throw new RegleMetierException("الهيكل الأب غير موجود");
        }

        if (await IsCycleAsync(idUnite, cmd.IdUniteParent, cmd.DateDebut, ct))
            throw new RegleMetierException(
                "تمييز في الهيكلة: الهيكل لا يمكن أن يكون تابعا لمن هو تابع له");

        if (cmd.IdDocument.HasValue &&
            !await _db.PieceJointes.AnyAsync(p => p.Id == cmd.IdDocument.Value, ct))
            throw new RegleMetierException("المستند غير موجود");

        var entity = new HistoriqueOrganigrammeUnite
        {
            Id = Guid.NewGuid(),
            IdUnite = idUnite,
            IdUniteParent = cmd.IdUniteParent,
            DateDebut = cmd.DateDebut,
            DateFin = null,
            IdDocument = cmd.IdDocument
        };

        return await _periodes.CloseAndAddAsync(
            _db.HistoriqueOrganigrammeUniteSet,
            entity,
            e => e.IdUnite == idUnite,
            $"rh:periode:HistOrgUnite:{idUnite}",
            ct);
    }

    public async Task<IReadOnlyList<ParentOrganigrammeDto>> ListHistoryAsync(
        Guid idUnite,
        CancellationToken ct = default)
    {
        var records = await _db.HistoriqueOrganigrammeUniteSet.AsNoTracking()
            .Where(h => h.IdUnite == idUnite)
            .OrderByDescending(h => h.DateDebut)
            .ToListAsync(ct);

        var result = new List<ParentOrganigrammeDto>();
        foreach (var record in records)
        {
            if (record.IdUniteParent is null)
            {
                result.Add(new ParentOrganigrammeDto(Guid.Empty, string.Empty, record.DateDebut, record.DateFin));
                continue;
            }

            var parent = await _db.UnitesOrganisationnelles.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == record.IdUniteParent, ct);
            result.Add(new ParentOrganigrammeDto(
                parent?.Id ?? Guid.Empty,
                parent?.NomUnite ?? string.Empty,
                record.DateDebut,
                record.DateFin));
        }

        return result;
    }

    private async Task<bool> IsCycleAsync(
        Guid idUnite,
        Guid? parentId,
        DateOnly date,
        CancellationToken ct)
    {
        if (parentId is null)
            return false;

        var visited = new HashSet<Guid> { idUnite };
        var current = parentId;
        var guard = 0;

        while (current is not null && guard++ < 100)
        {
            if (!visited.Add(current.Value))
                return true;

            var record = await _db.HistoriqueOrganigrammeUniteSet.AsNoTracking()
                .Where(h => h.IdUnite == current.Value
                    && h.DateDebut <= date
                    && (h.DateFin == null || h.DateFin >= date))
                .OrderByDescending(h => h.DateDebut)
                .FirstOrDefaultAsync(ct);

            current = record?.IdUniteParent;
        }

        return false;
    }
}
