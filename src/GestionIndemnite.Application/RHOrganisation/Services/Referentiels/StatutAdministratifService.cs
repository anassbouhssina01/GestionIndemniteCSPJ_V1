using GestionIndemnite.Application.RHOrganisation.Common;
using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Domain.RHOrganisation.Entities;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GestionIndemnite.Application.RHOrganisation.Services.Referentiels;

public class StatutAdministratifService
{
    private readonly IRHOrganisationDbContext _db;

    public StatutAdministratifService(IRHOrganisationDbContext db)
    {
        _db = db;
    }

    public async Task<StatutAdministratifDto> CreateAsync(
        CreateStatutAdministratifCommand cmd,
        CancellationToken ct = default)
    {
        var code = (cmd.CodeStatut ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(code))
            throw new RegleMetierException("Le code de la situation administrative est obligatoire.");

        if (await _db.StatutAdministratifs.AnyAsync(s => s.CodeStatut == code, ct))
            throw new RegleMetierException($"Le code «{code}» est déjà utilisé.");

        var entity = new StatutAdministratif
        {
            Id = Guid.NewGuid(),
            CodeStatut = code,
            LibelleStatutAr = (cmd.LibelleStatutAr ?? string.Empty).Trim(),
            LibelleStatutFr = (cmd.LibelleStatutFr ?? string.Empty).Trim(),
            Description = cmd.Description?.Trim()
        };

        _db.StatutAdministratifs.Add(entity);
        await _db.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task<StatutAdministratifDto> UpdateAsync(
        Guid id,
        UpdateStatutAdministratifCommand cmd,
        CancellationToken ct = default)
    {
        var entity = await _db.StatutAdministratifs.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new RegleMetierException("La situation administrative est introuvable.");

        if (entity.EstActif && !cmd.EstActif
            && await _db.SituationAdministratives.AnyAsync(h => h.IdStatut == id, ct))
            throw new RegleMetierException("Impossible de désactiver une situation administrative utilisée dans les dossiers.");

        entity.LibelleStatutAr = (cmd.LibelleStatutAr ?? string.Empty).Trim();
        entity.LibelleStatutFr = (cmd.LibelleStatutFr ?? string.Empty).Trim();
        entity.Description = cmd.Description?.Trim();
        entity.EstActif = cmd.EstActif;

        await _db.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<StatutAdministratifDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.StatutAdministratifs.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new RegleMetierException("La situation administrative est introuvable.");
        return ToDto(entity);
    }

    public async Task<IReadOnlyList<StatutAdministratifDto>> ListAsync(
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        var query = _db.StatutAdministratifs.AsNoTracking();
        if (!includeInactive)
            query = query.Where(s => s.EstActif);

        var list = await query.OrderBy(s => s.CodeStatut).ToListAsync(ct);
        return list.Select(ToDto).ToList();
    }

    public async Task DisableAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.StatutAdministratifs.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new RegleMetierException("La situation administrative est introuvable.");

        if (await _db.SituationAdministratives.AnyAsync(h => h.IdStatut == id, ct))
            throw new RegleMetierException("Impossible de désactiver une situation administrative utilisée dans les dossiers.");

        entity.EstActif = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task EnableAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.StatutAdministratifs.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new RegleMetierException("La situation administrative est introuvable.");
        entity.EstActif = true;
        await _db.SaveChangesAsync(ct);
    }

    private static StatutAdministratifDto ToDto(StatutAdministratif s) =>
        new(s.Id, s.CodeStatut, s.LibelleStatutAr, s.LibelleStatutFr, s.Description, s.EstActif);
}
