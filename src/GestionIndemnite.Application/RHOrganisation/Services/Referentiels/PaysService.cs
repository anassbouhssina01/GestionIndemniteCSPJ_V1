using GestionIndemnite.Application.RHOrganisation.Common;
using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Domain.RHOrganisation.Entities;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GestionIndemnite.Application.RHOrganisation.Services.Referentiels;

public class PaysService
{
    private readonly IRHOrganisationDbContext _db;

    public PaysService(IRHOrganisationDbContext db)
    {
        _db = db;
    }

    public async Task<PaysDto> CreateAsync(CreatePaysCommand cmd, CancellationToken ct = default)
    {
        var code = (cmd.CodePays ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(code))
            throw new RegleMetierException("Le code du pays est obligatoire.");

        if (await _db.PaysSet.AnyAsync(p => p.CodePays == code, ct))
            throw new RegleMetierException($"Le code «{code}» est déjà utilisé.");

        if (cmd.EstLocal && await _db.PaysSet.AnyAsync(p => p.EstLocal, ct))
            throw new RegleMetierException("Un seul pays local est autorisé.");

        var entity = new Pays
        {
            Id = Guid.NewGuid(),
            CodePays = code,
            NomPaysAr = (cmd.NomPaysAr ?? string.Empty).Trim(),
            NomPaysFr = (cmd.NomPaysFr ?? string.Empty).Trim(),
            EstLocal = cmd.EstLocal
        };

        _db.PaysSet.Add(entity);
        await _db.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task<PaysDto> UpdateAsync(Guid id, UpdatePaysCommand cmd, CancellationToken ct = default)
    {
        var entity = await _db.PaysSet.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new RegleMetierException("Le pays est introuvable.");

        if (cmd.EstLocal
            && !entity.EstLocal
            && await _db.PaysSet.AnyAsync(p => p.EstLocal && p.Id != id, ct))
            throw new RegleMetierException("Un seul pays local est autorisé.");

        entity.NomPaysAr = (cmd.NomPaysAr ?? string.Empty).Trim();
        entity.NomPaysFr = (cmd.NomPaysFr ?? string.Empty).Trim();
        entity.EstLocal = cmd.EstLocal;
        entity.EstActif = cmd.EstActif;

        await _db.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<PaysDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.PaysSet.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new RegleMetierException("Le pays est introuvable.");
        return ToDto(entity);
    }

    public async Task<IReadOnlyList<PaysDto>> ListAsync(
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        var query = _db.PaysSet.AsNoTracking();
        if (!includeInactive)
            query = query.Where(p => p.EstActif);

        var list = await query.OrderBy(p => p.CodePays).ToListAsync(ct);
        return list.Select(ToDto).ToList();
    }

    public async Task DisableAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.PaysSet.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new RegleMetierException("Le pays est introuvable.");
        entity.EstActif = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task EnableAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.PaysSet.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new RegleMetierException("Le pays est introuvable.");
        entity.EstActif = true;
        await _db.SaveChangesAsync(ct);
    }

    private static PaysDto ToDto(Pays p) =>
        new(p.Id, p.CodePays, p.NomPaysAr, p.NomPaysFr, p.EstLocal, p.EstActif);
}
