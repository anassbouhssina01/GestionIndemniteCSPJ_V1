using GestionIndemnite.Application.RHOrganisation.Common;
using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Domain.RHOrganisation.Entities;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GestionIndemnite.Application.RHOrganisation.Services;

public class FonctionnaireService
{
    private readonly IRHOrganisationDbContext _db;

    public FonctionnaireService(IRHOrganisationDbContext db)
    {
        _db = db;
    }

    public async Task<FonctionnaireDto> CreateAsync(
        CreateFonctionnaireCommand cmd,
        CancellationToken ct = default)
    {
        var matricule = (cmd.Matricule ?? string.Empty).Trim();
        var cin = (cmd.Cin ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(matricule))
            throw new RegleMetierException("Le numéro matricule est obligatoire.");
        if (string.IsNullOrEmpty(cin))
            throw new RegleMetierException("Le numéro de CIN est obligatoire.");

        if (await _db.Fonctionnaires.AnyAsync(f => f.Matricule == matricule, ct))
            throw new RegleMetierException($"Le matricule «{matricule}» est déjà utilisé.");

        if (await _db.Fonctionnaires.AnyAsync(f => f.Cin == cin, ct))
            throw new RegleMetierException($"Le CIN «{cin}» est déjà utilisé.");

        var entity = new Fonctionnaire
        {
            Id = Guid.NewGuid(),
            Matricule = matricule,
            Cin = cin,
            NomAr = (cmd.NomAr ?? string.Empty).Trim(),
            PrenomAr = (cmd.PrenomAr ?? string.Empty).Trim(),
            NomFr = (cmd.NomFr ?? string.Empty).Trim(),
            PrenomFr = (cmd.PrenomFr ?? string.Empty).Trim(),
            DateNaissance = cmd.DateNaissance,
            DateRecrutement = cmd.DateRecrutement,
            Genre = cmd.Genre
        };

        _db.Fonctionnaires.Add(entity);
        await _db.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task<FonctionnaireDto> UpdateAsync(
        Guid id,
        UpdateFonctionnaireCommand cmd,
        CancellationToken ct = default)
    {
        var entity = await _db.Fonctionnaires.FirstOrDefaultAsync(f => f.Id == id, ct)
            ?? throw new RegleMetierException("Le fonctionnaire est introuvable.");

        entity.NomAr = (cmd.NomAr ?? string.Empty).Trim();
        entity.PrenomAr = (cmd.PrenomAr ?? string.Empty).Trim();
        entity.NomFr = (cmd.NomFr ?? string.Empty).Trim();
        entity.PrenomFr = (cmd.PrenomFr ?? string.Empty).Trim();
        entity.DateNaissance = cmd.DateNaissance;
        entity.DateRecrutement = cmd.DateRecrutement;
        entity.Genre = cmd.Genre;

        await _db.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<FonctionnaireDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Fonctionnaires.AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, ct)
            ?? throw new RegleMetierException("Le fonctionnaire est introuvable.");
        return ToDto(entity);
    }

    public async Task<IReadOnlyList<FonctionnaireDto>> SearchAsync(
        string? matricule = null,
        string? cin = null,
        string? nom = null,
        CancellationToken ct = default)
    {
        var query = _db.Fonctionnaires.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(matricule))
            query = query.Where(f => f.Matricule.Contains(matricule));

        if (!string.IsNullOrWhiteSpace(cin))
            query = query.Where(f => f.Cin.Contains(cin));

        if (!string.IsNullOrWhiteSpace(nom))
        {
            var n = nom.Trim();
            query = query.Where(f => f.NomFr.Contains(n) || f.NomAr.Contains(n));
        }

        var list = await query.OrderBy(f => f.Matricule).Take(200).ToListAsync(ct);
        return list.Select(ToDto).ToList();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Fonctionnaires.FirstOrDefaultAsync(f => f.Id == id, ct)
            ?? throw new RegleMetierException("Le fonctionnaire est introuvable.");

        var hasHistorique = await _db.Ribs.AnyAsync(r => r.IdFonctionnaire == id, ct)
            || await _db.HistoriqueCadreGrades.AnyAsync(h => h.IdFonctionnaire == id, ct)
            || await _db.SituationAdministratives.AnyAsync(h => h.IdFonctionnaire == id, ct)
            || await _db.HistoriqueAffectations.AnyAsync(h => h.IdFonctionnaire == id, ct)
            || await _db.HistoriqueResponsabilites.AnyAsync(h => h.IdFonctionnaire == id, ct);
        if (hasHistorique)
            throw new RegleMetierException("Impossible de supprimer un fonctionnaire disposant d'un historique.");

        _db.Fonctionnaires.Remove(entity);
        await _db.SaveChangesAsync(ct);
    }

    private static FonctionnaireDto ToDto(Fonctionnaire f) =>
        new(f.Id, f.Matricule, f.Cin, f.NomAr, f.PrenomAr, f.NomFr, f.PrenomFr,
            f.DateNaissance, f.DateRecrutement, f.Genre);
}
