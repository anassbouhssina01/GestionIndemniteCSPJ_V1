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
            ?? throw new RegleMetierException("Le cadre est introuvable.");

        if (await _db.Grades.AnyAsync(
                g => g.IdCadre == cadre.Id && g.NumeroReferencie == cmd.NumeroReferencie, ct))
            throw new RegleMetierException(
                $"La référence {cmd.NumeroReferencie} est déjà utilisée dans ce cadre.");

        var grade = new Grade
        {
            Id = Guid.NewGuid(),
            IdCadre = cadre.Id,
            NumeroReferencie = cmd.NumeroReferencie,
            LibelleGradeAr = (cmd.LibelleGradeAr ?? string.Empty).Trim(),
            LibelleGradeFr = (cmd.LibelleGradeFr ?? string.Empty).Trim(),
            Description = cmd.Description?.Trim()
        };

        _db.Grades.Add(grade);
        await _db.SaveChangesAsync(ct);

        return ToDto(grade);
    }

    public async Task<GradeDto> UpdateAsync(Guid id, UpdateGradeCommand cmd, CancellationToken ct = default)
    {
        var grade = await _db.Grades.FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw new RegleMetierException("L'échelon est introuvable.");

        if (await _db.Grades.AnyAsync(
                g => g.IdCadre == grade.IdCadre
                  && g.NumeroReferencie == cmd.NumeroReferencie
                  && g.Id != id, ct))
            throw new RegleMetierException(
                $"La référence {cmd.NumeroReferencie} est déjà utilisée dans ce cadre.");

        if (grade.NumeroReferencie != cmd.NumeroReferencie
            && await _db.HistoriqueCadreGrades.AnyAsync(h => h.IdGrade == id, ct))
            throw new RegleMetierException("Impossible de modifier la référence d'un échelon utilisé dans les dossiers.");

        if (grade.EstActif && !cmd.EstActif
            && await _db.HistoriqueCadreGrades.AnyAsync(h => h.IdGrade == id, ct))
            throw new RegleMetierException("Impossible de désactiver un échelon utilisé dans les dossiers.");

        grade.NumeroReferencie = cmd.NumeroReferencie;
        grade.LibelleGradeAr = (cmd.LibelleGradeAr ?? string.Empty).Trim();
        grade.LibelleGradeFr = (cmd.LibelleGradeFr ?? string.Empty).Trim();
        grade.Description = cmd.Description?.Trim();
        grade.EstActif = cmd.EstActif;

        await _db.SaveChangesAsync(ct);
        return ToDto(grade);
    }

    public async Task<GradeDto> GetAsync(Guid id, CancellationToken ct = default)
    {
        var grade = await _db.Grades.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw new RegleMetierException("L'échelon est introuvable.");
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
            ?? throw new RegleMetierException("L'échelon est introuvable.");

        if (await _db.HistoriqueCadreGrades.AnyAsync(h => h.IdGrade == id, ct))
            throw new RegleMetierException("Impossible de désactiver un échelon utilisé dans les dossiers.");

        grade.EstActif = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task EnableAsync(Guid id, CancellationToken ct = default)
    {
        var grade = await _db.Grades.FirstOrDefaultAsync(g => g.Id == id, ct)
            ?? throw new RegleMetierException("L'échelon est introuvable.");
        grade.EstActif = true;
        await _db.SaveChangesAsync(ct);
    }

    private static GradeDto ToDto(Grade g) =>
        new(g.Id, g.IdCadre, g.NumeroReferencie, g.LibelleGradeAr, g.LibelleGradeFr, g.Description, g.EstActif);
}
