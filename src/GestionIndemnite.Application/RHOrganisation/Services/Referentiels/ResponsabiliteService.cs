using GestionIndemnite.Application.RHOrganisation.Common;
using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Domain.RHOrganisation.Entities;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GestionIndemnite.Application.RHOrganisation.Services.Referentiels;

public class ResponsabiliteService
{
    private readonly IRHOrganisationDbContext _db;

    public ResponsabiliteService(IRHOrganisationDbContext db)
    {
        _db = db;
    }

    public async Task<ResponsabiliteDto> CreateAsync(
        CreateResponsabiliteCommand cmd,
        CancellationToken ct = default)
    {
        var code = (cmd.CodeResponsabilite ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(code))
            throw new RegleMetierException("Le code de la responsabilité est obligatoire.");

        if (await _db.Responsabilites.AnyAsync(r => r.CodeResponsabilite == code, ct))
            throw new RegleMetierException($"Le code «{code}» est déjà utilisé.");

        var entity = new Responsabilite
        {
            Id = Guid.NewGuid(),
            CodeResponsabilite = code,
            LibelleResponsabiliteAr = (cmd.LibelleResponsabiliteAr ?? string.Empty).Trim(),
            LibelleResponsabiliteFr = (cmd.LibelleResponsabiliteFr ?? string.Empty).Trim(),
            Description = cmd.Description?.Trim()
        };

        _db.Responsabilites.Add(entity);
        await _db.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task<ResponsabiliteDto> UpdateAsync(
        Guid id,
        UpdateResponsabiliteCommand cmd,
        CancellationToken ct = default)
    {
        var entity = await _db.Responsabilites.FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new RegleMetierException("La responsabilité est introuvable.");

        if (entity.EstActif && !cmd.EstActif
            && await _db.HistoriqueResponsabilites.AnyAsync(h => h.IdResponsabilite == id, ct))
            throw new RegleMetierException("Impossible de désactiver une responsabilité utilisée dans les dossiers.");

        entity.LibelleResponsabiliteAr = (cmd.LibelleResponsabiliteAr ?? string.Empty).Trim();
        entity.LibelleResponsabiliteFr = (cmd.LibelleResponsabiliteFr ?? string.Empty).Trim();
        entity.Description = cmd.Description?.Trim();
        entity.EstActif = cmd.EstActif;

        await _db.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<ResponsabiliteDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Responsabilites.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new RegleMetierException("La responsabilité est introuvable.");
        return ToDto(entity);
    }

    public async Task<IReadOnlyList<ResponsabiliteDto>> ListAsync(
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        var query = _db.Responsabilites.AsNoTracking();
        if (!includeInactive)
            query = query.Where(r => r.EstActif);

        var list = await query.OrderBy(r => r.CodeResponsabilite).ToListAsync(ct);
        return list.Select(ToDto).ToList();
    }

    public async Task DisableAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Responsabilites.FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new RegleMetierException("La responsabilité est introuvable.");

        if (await _db.HistoriqueResponsabilites.AnyAsync(h => h.IdResponsabilite == id, ct))
            throw new RegleMetierException("Impossible de désactiver une responsabilité utilisée dans les dossiers.");

        entity.EstActif = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task EnableAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Responsabilites.FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new RegleMetierException("La responsabilité est introuvable.");
        entity.EstActif = true;
        await _db.SaveChangesAsync(ct);
    }

    private static ResponsabiliteDto ToDto(Responsabilite r) =>
        new(r.Id, r.CodeResponsabilite, r.LibelleResponsabiliteAr, r.LibelleResponsabiliteFr,
            r.Description, r.EstActif);
}
