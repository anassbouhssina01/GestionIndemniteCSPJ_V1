using GestionIndemnite.Application.RHOrganisation.Common;
using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Domain.RHOrganisation.Entities;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GestionIndemnite.Application.RHOrganisation.Services.Referentiels;

public class VilleService
{
    private readonly IRHOrganisationDbContext _db;

    public VilleService(IRHOrganisationDbContext db)
    {
        _db = db;
    }

    public async Task<VilleDto> CreateAsync(CreateVilleCommand cmd, CancellationToken ct = default)
    {
        var nom = cmd.NomVille.Trim();
        if (string.IsNullOrEmpty(nom))
            throw new RegleMetierException("اسم المدينة مطلوب");

        if (cmd.Latitude.HasValue != cmd.Longitude.HasValue)
            throw new RegleMetierException(
                "إذا أدخلت إحداثيات فيجب إدخال خط الطول وخط العرض معاً");

        if (!await _db.PaysSet.AnyAsync(p => p.Id == cmd.IdPays, ct))
            throw new RegleMetierException("البلد غير موجود");

        var entity = new Ville
        {
            Id = Guid.NewGuid(),
            NomVille = nom,
            NomVilleAr = (cmd.NomVilleAr ?? string.Empty).Trim(),
            IdPays = cmd.IdPays,
            Latitude = cmd.Latitude,
            Longitude = cmd.Longitude
        };

        _db.Villes.Add(entity);
        await _db.SaveChangesAsync(ct);

        return ToDto(entity);
    }

    public async Task<VilleDto> UpdateAsync(Guid id, UpdateVilleCommand cmd, CancellationToken ct = default)
    {
        var entity = await _db.Villes.FirstOrDefaultAsync(v => v.Id == id, ct)
            ?? throw new RegleMetierException("المدينة غير موجودة");

        if (cmd.Latitude.HasValue != cmd.Longitude.HasValue)
            throw new RegleMetierException(
                "إذا أدخلت إحداثيات فيجب إدخال خط الطول وخط العرض معاً");

        entity.NomVille = cmd.NomVille.Trim();
        entity.NomVilleAr = (cmd.NomVilleAr ?? string.Empty).Trim();
        entity.Latitude = cmd.Latitude;
        entity.Longitude = cmd.Longitude;
        entity.IsActive = cmd.IsActive;

        await _db.SaveChangesAsync(ct);
        return ToDto(entity);
    }

    public async Task<VilleDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Villes.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, ct)
            ?? throw new RegleMetierException("المدينة غير موجودة");
        return ToDto(entity);
    }

    public async Task<IReadOnlyList<VilleDto>> ListAsync(
        Guid? idPays = null,
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        var query = _db.Villes.AsNoTracking();
        if (idPays.HasValue)
            query = query.Where(v => v.IdPays == idPays);
        if (!includeInactive)
            query = query.Where(v => v.IsActive);

        var list = await query.OrderBy(v => v.NomVille).ToListAsync(ct);
        return list.Select(ToDto).ToList();
    }

    public async Task DisableAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Villes.FirstOrDefaultAsync(v => v.Id == id, ct)
            ?? throw new RegleMetierException("المدينة غير موجودة");
        entity.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task EnableAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Villes.FirstOrDefaultAsync(v => v.Id == id, ct)
            ?? throw new RegleMetierException("المدينة غير موجودة");
        entity.IsActive = true;
        await _db.SaveChangesAsync(ct);
    }

    private static VilleDto ToDto(Ville v) =>
        new(v.Id, v.NomVille, v.IdPays, v.Latitude, v.Longitude, v.IsActive, v.NomVilleAr);
}
