using GestionIndemnite.Application.RHOrganisation.Common;
using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Domain.RHOrganisation.Entities;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GestionIndemnite.Application.RHOrganisation.Services.Referentiels;

public class CadreService
{
    private readonly IRHOrganisationDbContext _db;

    public CadreService(IRHOrganisationDbContext db)
    {
        _db = db;
    }

    public async Task<CadreDto> CreateAsync(CreateCadreCommand cmd, CancellationToken ct = default)
    {
        var code = cmd.CodeCadre.Trim();
        if (string.IsNullOrEmpty(code))
            throw new RegleMetierException("رمز الإطار مطلوب");

        if (await _db.Cadres.AnyAsync(c => c.CodeCadre == code, ct))
            throw new RegleMetierException($"الرمز «{code}» مستعمل من قبل");

        var cadre = new Cadre
        {
            Id = Guid.NewGuid(),
            CodeCadre = code,
            LibelleCadreAr = cmd.LibelleCadreAr.Trim(),
            LibelleCadreFr = cmd.LibelleCadreFr.Trim(),
            Description = cmd.Description?.Trim()
        };

        _db.Cadres.Add(cadre);
        await _db.SaveChangesAsync(ct);

        return ToDto(cadre);
    }

    public async Task<CadreDto> UpdateAsync(Guid id, UpdateCadreCommand cmd, CancellationToken ct = default)
    {
        var cadre = await _db.Cadres.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new RegleMetierException("الإطار غير موجود");

        if (cadre.EstActif && !cmd.EstActif && await EstUtiliseAsync(id, ct))
            throw new RegleMetierException("لا يمكن تعطيل إطار مستعمل في السجلات");

        cadre.LibelleCadreAr = cmd.LibelleCadreAr.Trim();
        cadre.LibelleCadreFr = cmd.LibelleCadreFr.Trim();
        cadre.Description = cmd.Description?.Trim();
        cadre.EstActif = cmd.EstActif;

        await _db.SaveChangesAsync(ct);
        return ToDto(cadre);
    }

    public async Task<CadreDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var cadre = await _db.Cadres.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new RegleMetierException("الإطار غير موجود");
        return ToDto(cadre);
    }

    public async Task<IReadOnlyList<CadreDto>> ListAsync(bool includeInactive = false, CancellationToken ct = default)
    {
        var query = _db.Cadres.AsNoTracking();
        if (!includeInactive)
            query = query.Where(c => c.EstActif);

        var list = await query.OrderBy(c => c.CodeCadre).ToListAsync(ct);
        return list.Select(ToDto).ToList();
    }

    public async Task DisableAsync(Guid id, CancellationToken ct = default)
    {
        var cadre = await _db.Cadres.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new RegleMetierException("الإطار غير موجود");

        if (await EstUtiliseAsync(id, ct))
            throw new RegleMetierException("لا يمكن تعطيل إطار مستعمل في السجلات");

        cadre.EstActif = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task EnableAsync(Guid id, CancellationToken ct = default)
    {
        var cadre = await _db.Cadres.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new RegleMetierException("الإطار غير موجود");
        cadre.EstActif = true;
        await _db.SaveChangesAsync(ct);
    }

    private async Task<bool> EstUtiliseAsync(Guid idCadre, CancellationToken ct) =>
        await _db.Grades.AnyAsync(
            g => g.IdCadre == idCadre
                && _db.HistoriqueCadreGrades.Any(h => h.IdGrade == g.Id), ct);

    private static CadreDto ToDto(Cadre c) =>
        new(c.Id, c.CodeCadre, c.LibelleCadreAr, c.LibelleCadreFr, c.Description, c.EstActif);
}
