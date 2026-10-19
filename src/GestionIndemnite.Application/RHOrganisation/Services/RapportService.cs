using GestionIndemnite.Application.RHOrganisation.Common;
using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GestionIndemnite.Application.RHOrganisation.Services;

internal record PeriodeLigne(Guid Id, Guid IdCle, DateOnly DateDebut, DateOnly? DateFin);

public class RapportService
{
    private readonly IRHOrganisationDbContext _db;
    private readonly PeriodeService _periodes;

    public RapportService(IRHOrganisationDbContext db, PeriodeService periodes)
    {
        _db = db;
        _periodes = periodes;
    }

    public async Task<IReadOnlyList<TrouDto>> GetTrousAsync(
        TypeHistoriqueRapport type,
        Guid idCle,
        CancellationToken ct = default)
    {
        ValidateType(type);

        IReadOnlyList<PeriodeGap> gaps = type switch
        {
            TypeHistoriqueRapport.CadreGrade =>
                await _periodes.GetGapsAsync(_db.HistoriqueCadreGrades, h => h.IdFonctionnaire == idCle, ct),
            TypeHistoriqueRapport.SituationAdministrative =>
                await _periodes.GetGapsAsync(_db.SituationAdministratives, h => h.IdFonctionnaire == idCle, ct),
            TypeHistoriqueRapport.Affectation =>
                await _periodes.GetGapsAsync(_db.HistoriqueAffectations, h => h.IdFonctionnaire == idCle, ct),
            TypeHistoriqueRapport.Responsabilite =>
                await _periodes.GetGapsAsync(_db.HistoriqueResponsabilites, h => h.IdFonctionnaire == idCle, ct),
            TypeHistoriqueRapport.OrganigrammeTribunal =>
                await _periodes.GetGapsAsync(_db.HistoriqueOrganigrammeTribunalSet, h => h.IdTribunal == idCle, ct),
            TypeHistoriqueRapport.OrganigrammeUnite =>
                await _periodes.GetGapsAsync(_db.HistoriqueOrganigrammeUniteSet, h => h.IdUnite == idCle, ct),
            _ => throw new RegleMetierException("نوع التقرير غير معروف")
        };

        return gaps.Select(g => new TrouDto(type, idCle, g.DateDebut, g.DateFin)).ToList();
    }

    public async Task<IReadOnlyList<SansJustificatifDto>> GetSansJustificatifAsync(
        TypeHistoriqueRapport? type = null,
        CancellationToken ct = default)
    {
        if (type.HasValue)
            ValidateType(type.Value);

        var result = new List<SansJustificatifDto>();

        if (type is null or TypeHistoriqueRapport.CadreGrade)
        {
            var rows = await _db.HistoriqueCadreGrades.AsNoTracking()
                .Where(h => h.IdDocument == null)
                .Select(h => new PeriodeLigne(h.Id, h.IdFonctionnaire, h.DateDebut, h.DateFin))
                .ToListAsync(ct);
            result.AddRange(rows.Select(r => new SansJustificatifDto(
                TypeHistoriqueRapport.CadreGrade, r.Id, r.IdCle, r.DateDebut, r.DateFin)));
        }

        if (type is null or TypeHistoriqueRapport.SituationAdministrative)
        {
            var rows = await _db.SituationAdministratives.AsNoTracking()
                .Where(h => h.IdDocument == null)
                .Select(h => new PeriodeLigne(h.Id, h.IdFonctionnaire, h.DateDebut, h.DateFin))
                .ToListAsync(ct);
            result.AddRange(rows.Select(r => new SansJustificatifDto(
                TypeHistoriqueRapport.SituationAdministrative, r.Id, r.IdCle, r.DateDebut, r.DateFin)));
        }

        if (type is null or TypeHistoriqueRapport.Affectation)
        {
            var rows = await _db.HistoriqueAffectations.AsNoTracking()
                .Where(h => h.IdDocument == null)
                .Select(h => new PeriodeLigne(h.Id, h.IdFonctionnaire, h.DateDebut, h.DateFin))
                .ToListAsync(ct);
            result.AddRange(rows.Select(r => new SansJustificatifDto(
                TypeHistoriqueRapport.Affectation, r.Id, r.IdCle, r.DateDebut, r.DateFin)));
        }

        if (type is null or TypeHistoriqueRapport.Responsabilite)
        {
            var rows = await _db.HistoriqueResponsabilites.AsNoTracking()
                .Where(h => h.IdDocument == null)
                .Select(h => new PeriodeLigne(h.Id, h.IdFonctionnaire, h.DateDebut, h.DateFin))
                .ToListAsync(ct);
            result.AddRange(rows.Select(r => new SansJustificatifDto(
                TypeHistoriqueRapport.Responsabilite, r.Id, r.IdCle, r.DateDebut, r.DateFin)));
        }

        if (type is null or TypeHistoriqueRapport.OrganigrammeTribunal)
        {
            var rows = await _db.HistoriqueOrganigrammeTribunalSet.AsNoTracking()
                .Where(h => h.IdDocument == null)
                .Select(h => new PeriodeLigne(h.Id, h.IdTribunal, h.DateDebut, h.DateFin))
                .ToListAsync(ct);
            result.AddRange(rows.Select(r => new SansJustificatifDto(
                TypeHistoriqueRapport.OrganigrammeTribunal, r.Id, r.IdCle, r.DateDebut, r.DateFin)));
        }

        if (type is null or TypeHistoriqueRapport.OrganigrammeUnite)
        {
            var rows = await _db.HistoriqueOrganigrammeUniteSet.AsNoTracking()
                .Where(h => h.IdDocument == null)
                .Select(h => new PeriodeLigne(h.Id, h.IdUnite, h.DateDebut, h.DateFin))
                .ToListAsync(ct);
            result.AddRange(rows.Select(r => new SansJustificatifDto(
                TypeHistoriqueRapport.OrganigrammeUnite, r.Id, r.IdCle, r.DateDebut, r.DateFin)));
        }

        return result;
    }

    public async Task<IReadOnlyList<ChevauchementDto>> ControleChevauchementsAsync(
        TypeHistoriqueRapport? type = null,
        CancellationToken ct = default)
    {
        if (type.HasValue)
            ValidateType(type.Value);

        var result = new List<ChevauchementDto>();

        if (type is null or TypeHistoriqueRapport.CadreGrade)
        {
            var rows = await _db.HistoriqueCadreGrades.AsNoTracking()
                .Select(h => new PeriodeLigne(h.Id, h.IdFonctionnaire, h.DateDebut, h.DateFin))
                .ToListAsync(ct);
            result.AddRange(Detect(TypeHistoriqueRapport.CadreGrade, rows));
        }

        if (type is null or TypeHistoriqueRapport.SituationAdministrative)
        {
            var rows = await _db.SituationAdministratives.AsNoTracking()
                .Select(h => new PeriodeLigne(h.Id, h.IdFonctionnaire, h.DateDebut, h.DateFin))
                .ToListAsync(ct);
            result.AddRange(Detect(TypeHistoriqueRapport.SituationAdministrative, rows));
        }

        if (type is null or TypeHistoriqueRapport.Affectation)
        {
            var rows = await _db.HistoriqueAffectations.AsNoTracking()
                .Select(h => new PeriodeLigne(h.Id, h.IdFonctionnaire, h.DateDebut, h.DateFin))
                .ToListAsync(ct);
            result.AddRange(Detect(TypeHistoriqueRapport.Affectation, rows));
        }

        if (type is null or TypeHistoriqueRapport.Responsabilite)
        {
            var rows = await _db.HistoriqueResponsabilites.AsNoTracking()
                .Select(h => new PeriodeLigne(h.Id, h.IdFonctionnaire, h.DateDebut, h.DateFin))
                .ToListAsync(ct);
            result.AddRange(Detect(TypeHistoriqueRapport.Responsabilite, rows));
        }

        if (type is null or TypeHistoriqueRapport.OrganigrammeTribunal)
        {
            var rows = await _db.HistoriqueOrganigrammeTribunalSet.AsNoTracking()
                .Select(h => new PeriodeLigne(h.Id, h.IdTribunal, h.DateDebut, h.DateFin))
                .ToListAsync(ct);
            result.AddRange(Detect(TypeHistoriqueRapport.OrganigrammeTribunal, rows));
        }

        if (type is null or TypeHistoriqueRapport.OrganigrammeUnite)
        {
            var rows = await _db.HistoriqueOrganigrammeUniteSet.AsNoTracking()
                .Select(h => new PeriodeLigne(h.Id, h.IdUnite, h.DateDebut, h.DateFin))
                .ToListAsync(ct);
            result.AddRange(Detect(TypeHistoriqueRapport.OrganigrammeUnite, rows));
        }

        return result;
    }

    private static IEnumerable<ChevauchementDto> Detect(
        TypeHistoriqueRapport type,
        IReadOnlyList<PeriodeLigne> rows)
    {
        foreach (var group in rows.GroupBy(r => r.IdCle))
        {
            var sorted = group.OrderBy(r => r.DateDebut).ToList();

            for (var i = 1; i < sorted.Count; i++)
            {
                var prev = sorted[i - 1];
                var curr = sorted[i];

                if (prev.DateFin is null || prev.DateFin >= curr.DateDebut)
                    yield return new ChevauchementDto(
                        type, group.Key,
                        prev.DateDebut, prev.DateFin,
                        curr.DateDebut, curr.DateFin);
            }
        }
    }

    private static void ValidateType(TypeHistoriqueRapport type)
    {
        if (!Enum.IsDefined(type))
            throw new RegleMetierException("نوع التقرير غير معروف");
    }
}
