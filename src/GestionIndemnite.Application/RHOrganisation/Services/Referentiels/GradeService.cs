using GestionIndemnite.Application.RHOrganisation.Common;
using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Domain.RHOrganisation.Entities;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GestionIndemnite.Application.RHOrganisation.Services.Referentiels;

public class GradeService
{
    private readonly IRHOrganisationDbContext _db;

    public GradeService(IRHOrganisationDbContext db)
    {
        _db = db;
    }

    public async Task<GradeDto> CreateAsync(CreateGradeCommand cmd, CancellationToken ct = default)
    {
        var cadre = await _db.Cadres.FirstOrDefaultAsync(c => c.Id == cmd.IdCadre, ct)
            ?? throw new RegleMetierException("الإطار غير موجود");

        if (await _db.Grades.AnyAsync(
                g => g.IdCadre == cadre.Id && g.NumeroReferencie == cmd.NumeroReferencie, ct))
            throw new RegleMetierException(
                $"الرقم المرجعي {cmd.NumeroReferencie} مستعمل داخل هذا الإطار");

        var grade = new Grade
        {
            Id = Guid.NewGuid(),
            IdCadre = cadre.Id,
            NumeroReferencie = cmd.NumeroReferencie,
            LibelleGradeAr = cmd.LibelleGradeAr.Trim(),
            LibelleGradeFr = cmd.LibelleGradeFr.Trim(),
            Description = cmd.Description?.Trim()
        };

        _db.Grades.Add(grade);
        await _db.SaveChangesAsync(ct);

        return ToDto(grade);
    }

    public async Task<GradeDto> UpdateAsync(Guid id, UpdateGradeCommand cmd, CancellationToken ct = default)
    {
        var grade = await _db.Grades.FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw new RegleMetierException("الرتبة غير موجودة");

        if (await _db.Grades.AnyAsync(
                g => g.IdCadre == grade.IdCadre
                  && g.NumeroReferencie == cmd.NumeroReferencie
                  && g.Id != id, ct))
            throw new RegleMetierException(
                $"الرقم المرجعي {cmd.NumeroReferencie} مستعمل داخل هذا الإطار");

        if (grade.NumeroReferencie != cmd.NumeroReferencie
            && await _db.HistoriqueCadreGrades.AnyAsync(h => h.IdGrade == id, ct))
            throw new RegleMetierException("لا يمكن تغيير الرقم المرجعي لرتبة مستعملة في السجلات");

        if (grade.EstActif && !cmd.EstActif
            && await _db.HistoriqueCadreGrades.AnyAsync(h => h.IdGrade == id, ct))
            throw new RegleMetierException("لا يمكن تعطيل رتبة مستعملة في السجلات");

        grade.NumeroReferencie = cmd.NumeroReferencie;
        grade.LibelleGradeAr = cmd.LibelleGradeAr.Trim();
        grade.LibelleGradeFr = cmd.LibelleGradeFr.Trim();
        grade.Description = cmd.Description?.Trim();
        grade.EstActif = cmd.EstActif;

        await _db.SaveChangesAsync(ct);
        return ToDto(grade);
    }

    public async Task<GradeDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var grade = await _db.Grades.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw new RegleMetierException("الرتبة غير موجودة");
        return ToDto(grade);
    }

    public async Task<IReadOnlyList<GradeDto>> ListAsync(
        Guid? idCadre = null,
        bool includeInactive = false,
        CancellationToken ct = default)
    {
        var query = _db.Grades.AsNoTracking();
        if (idCadre.HasValue)
            query = query.Where(g => g.IdCadre == idCadre);
        if (!includeInactive)
            query = query.Where(g => g.EstActif);

        var list = await query.OrderBy(g => g.NumeroReferencie).ToListAsync(ct);
        return list.Select(ToDto).ToList();
    }

    public async Task DisableAsync(Guid id, CancellationToken ct = default)
    {
        var grade = await _db.Grades.FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw new RegleMetierException("الرتبة غير موجودة");

        if (await _db.HistoriqueCadreGrades.AnyAsync(h => h.IdGrade == id, ct))
            throw new RegleMetierException("لا يمكن تعطيل رتبة مستعملة في السجلات");

        grade.EstActif = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task EnableAsync(Guid id, CancellationToken ct = default)
    {
        var grade = await _db.Grades.FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw new RegleMetierException("الرتبة غير موجودة");
        grade.EstActif = true;
        await _db.SaveChangesAsync(ct);
    }

    private static GradeDto ToDto(Grade g) =>
        new(g.Id, g.IdCadre, g.NumeroReferencie, g.LibelleGradeAr, g.LibelleGradeFr, g.Description, g.EstActif);
}
