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
        var code = cmd.CodePays.Trim();
        if (string.IsNullOrEmpty(code))
            throw new RegleMetierException("رمز البلد مطلوب");

        if (await _db.PaysSet.AnyAsync(p => p.CodePays == code, ct))
            throw new RegleMetierException($"الرمز «{code}» مستعمل من قبل");

        if (cmd.EstLocal && await _db.PaysSet.AnyAsync(p => p.EstLocal, ct))
            throw new RegleMetierException("يوجد بلد محلي واحد فقط");

        var entity = new Pays
        {
            Id = Guid.NewGuid(),
            CodePays = code,
            NomPaysAr = cmd.NomPaysAr.Trim(),
            NomPaysFr = cmd.NomPaysFr.Trim(),
            EstLocal = cmd.EstLocal
        };

        _db.PaysSet.Add(entity);
        await _db.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task<PaysDto> UpdateAsync(Guid id, UpdatePaysCommand cmd, CancellationToken ct = default)
    {
        var entity = await _db.PaysSet.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new RegleMetierException("البلد غير موجود");

        if (cmd.EstLocal
            && !entity.EstLocal
            && await _db.PaysSet.AnyAsync(p => p.EstLocal && p.Id != id, ct))
            throw new RegleMetierException("يوجد بلد محلي واحد فقط");

        entity.NomPaysAr = cmd.NomPaysAr.Trim();
        entity.NomPaysFr = cmd.NomPaysFr.Trim();
        entity.EstLocal = cmd.EstLocal;
        entity.EstActif = cmd.EstActif;

        await _db.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<PaysDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.PaysSet.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new RegleMetierException("البلد غير موجود");
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
            ?? throw new RegleMetierException("البلد غير موجود");
        entity.EstActif = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task EnableAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.PaysSet.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new RegleMetierException("البلد غير موجود");
        entity.EstActif = true;
        await _db.SaveChangesAsync(ct);
    }

    private static PaysDto ToDto(Pays p) =>
        new(p.Id, p.CodePays, p.NomPaysAr, p.NomPaysFr, p.EstLocal, p.EstActif);
}
