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
        var code = cmd.CodeStatut.Trim();
        if (string.IsNullOrEmpty(code))
            throw new RegleMetierException("رمز الوضعية الإدارية مطلوب");

        if (await _db.StatutAdministratifs.AnyAsync(s => s.CodeStatut == code, ct))
            throw new RegleMetierException($"الرمز «{code}» مستعمل من قبل");

        var entity = new StatutAdministratif
        {
            Id = Guid.NewGuid(),
            CodeStatut = code,
            LibelleStatutAr = cmd.LibelleStatutAr.Trim(),
            LibelleStatutFr = cmd.LibelleStatutFr.Trim(),
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
            ?? throw new RegleMetierException("الوضعية الإدارية غير موجودة");

        if (entity.EstActif && !cmd.EstActif
            && await _db.SituationAdministratives.AnyAsync(h => h.IdStatut == id, ct))
            throw new RegleMetierException("لا يمكن تعطيل وضعية إدارية مستعملة في السجلات");

        entity.LibelleStatutAr = cmd.LibelleStatutAr.Trim();
        entity.LibelleStatutFr = cmd.LibelleStatutFr.Trim();
        entity.Description = cmd.Description?.Trim();
        entity.EstActif = cmd.EstActif;

        await _db.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<StatutAdministratifDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.StatutAdministratifs.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new RegleMetierException("الوضعية الإدارية غير موجودة");
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
            ?? throw new RegleMetierException("الوضعية الإدارية غير موجودة");

        if (await _db.SituationAdministratives.AnyAsync(h => h.IdStatut == id, ct))
            throw new RegleMetierException("لا يمكن تعطيل وضعية إدارية مستعملة في السجلات");

        entity.EstActif = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task EnableAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.StatutAdministratifs.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new RegleMetierException("الوضعية الإدارية غير موجودة");
        entity.EstActif = true;
        await _db.SaveChangesAsync(ct);
    }

    private static StatutAdministratifDto ToDto(StatutAdministratif s) =>
        new(s.Id, s.CodeStatut, s.LibelleStatutAr, s.LibelleStatutFr, s.Description, s.EstActif);
}
