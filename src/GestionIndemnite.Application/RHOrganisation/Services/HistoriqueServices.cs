using GestionIndemnite.Application.RHOrganisation.Common;
using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Domain.RHOrganisation.Entities;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GestionIndemnite.Application.RHOrganisation.Services;

public class HistoriqueCadreGradeService
{
    private readonly IRHOrganisationDbContext _db;
    private readonly PeriodeService _periodes;

    public HistoriqueCadreGradeService(IRHOrganisationDbContext db, PeriodeService periodes)
    {
        _db = db;
        _periodes = periodes;
    }

    public async Task<Guid> AddAsync(
        CreateHistoriqueCadreGradeCommand cmd,
        CancellationToken ct = default)
    {
        if (!await _db.Fonctionnaires.AnyAsync(f => f.Id == cmd.IdFonctionnaire, ct))
            throw new RegleMetierException("الموظف غير موجود");
        if (!await _db.Grades.AnyAsync(g => g.Id == cmd.IdGrade, ct))
            throw new RegleMetierException("الدرجة غير موجودة");
        if (cmd.IdDocument.HasValue &&
            !await _db.PieceJointes.AnyAsync(p => p.Id == cmd.IdDocument.Value, ct))
            throw new RegleMetierException("المستند غير موجود");

        var entity = new HistoriqueCadreGrade
        {
            Id = Guid.NewGuid(),
            IdFonctionnaire = cmd.IdFonctionnaire,
            IdGrade = cmd.IdGrade,
            DateDebut = cmd.DateDebut,
            DateFin = cmd.DateFin,
            IdDocument = cmd.IdDocument
        };

        return await _periodes.AddAsync(
            _db.HistoriqueCadreGrades,
            entity,
            e => e.IdFonctionnaire == cmd.IdFonctionnaire,
            $"rh:periode:HistoriqueCadreGrade:{cmd.IdFonctionnaire}",
            ct);
    }

    public async Task<IReadOnlyList<HistoriqueCadreGradeDto>> ListByFonctionnaireAsync(
        Guid idFonctionnaire,
        CancellationToken ct = default)
    {
        var list = await _db.HistoriqueCadreGrades.AsNoTracking()
            .Where(h => h.IdFonctionnaire == idFonctionnaire)
            .OrderBy(h => h.DateDebut)
            .ToListAsync(ct);

        return list.Select(ToDto).ToList();
    }

    /// <summary>
    /// Clôture la période de grade ouverte (si elle existe) et ouvre la nouvelle.
    /// Permet de changer la grade d'un fonctionnaire en une seule opération.
    /// </summary>
    public async Task<Guid> ChangeAsync(
        Guid idFonctionnaire,
        Guid idGrade,
        DateOnly dateDebut,
        Guid? idDocument,
        CancellationToken ct = default)
    {
        if (!await _db.Fonctionnaires.AnyAsync(f => f.Id == idFonctionnaire, ct))
            throw new RegleMetierException("الموظف غير موجود");
        if (!await _db.Grades.AnyAsync(g => g.Id == idGrade, ct))
            throw new RegleMetierException("الدرجة غير موجودة");
        if (idDocument.HasValue &&
            !await _db.PieceJointes.AnyAsync(p => p.Id == idDocument.Value, ct))
            throw new RegleMetierException("المستند غير موجود");

        var entity = new HistoriqueCadreGrade
        {
            Id = Guid.NewGuid(),
            IdFonctionnaire = idFonctionnaire,
            IdGrade = idGrade,
            DateDebut = dateDebut,
            DateFin = null,
            IdDocument = idDocument
        };

        return await _periodes.CloseAndAddAsync(
            _db.HistoriqueCadreGrades,
            entity,
            e => e.IdFonctionnaire == idFonctionnaire,
            $"rh:periode:HistoriqueCadreGrade:{idFonctionnaire}",
            ct);
    }

    private static HistoriqueCadreGradeDto ToDto(HistoriqueCadreGrade h) =>
        new(h.Id, h.IdFonctionnaire, h.IdGrade, h.DateDebut, h.DateFin, h.IdDocument);
}

public class SituationAdministrativeService
{
    private readonly IRHOrganisationDbContext _db;
    private readonly PeriodeService _periodes;

    public SituationAdministrativeService(IRHOrganisationDbContext db, PeriodeService periodes)
    {
        _db = db;
        _periodes = periodes;
    }

    public async Task<Guid> AddAsync(
        CreateSituationAdministrativeCommand cmd,
        CancellationToken ct = default)
    {
        var fonctionnaire = await _db.Fonctionnaires
            .FirstOrDefaultAsync(f => f.Id == cmd.IdFonctionnaire, ct)
            ?? throw new RegleMetierException("الموظف غير موجود");

        if (!await _db.StatutAdministratifs.AnyAsync(s => s.Id == cmd.IdStatut, ct))
            throw new RegleMetierException("الوضع الإداري غير موجود");
        if (cmd.IdDocument.HasValue &&
            !await _db.PieceJointes.AnyAsync(p => p.Id == cmd.IdDocument.Value, ct))
            throw new RegleMetierException("المستند غير موجود");

        if (cmd.DateDebut < fonctionnaire.DateRecrutement)
            throw new RegleMetierException("التاريخ لا يمكن أن يسبق تاريخ التوظيف");

        var entity = new SituationAdministrative
        {
            Id = Guid.NewGuid(),
            IdFonctionnaire = cmd.IdFonctionnaire,
            IdStatut = cmd.IdStatut,
            DateDebut = cmd.DateDebut,
            DateFin = cmd.DateFin,
            IdDocument = cmd.IdDocument
        };

        return await _periodes.AddAsync(
            _db.SituationAdministratives,
            entity,
            e => e.IdFonctionnaire == cmd.IdFonctionnaire,
            $"rh:periode:SituationAdministrative:{cmd.IdFonctionnaire}",
            ct);
    }

    public async Task<IReadOnlyList<SituationAdministrativeDto>> ListByFonctionnaireAsync(
        Guid idFonctionnaire,
        CancellationToken ct = default)
    {
        var list = await _db.SituationAdministratives.AsNoTracking()
            .Where(s => s.IdFonctionnaire == idFonctionnaire)
            .OrderBy(s => s.DateDebut)
            .ToListAsync(ct);

        return list.Select(ToDto).ToList();
    }

    /// <summary>
    /// Clôture la situation administrative ouverte (si elle existe) et ouvre la nouvelle.
    /// </summary>
    public async Task<Guid> ChangeAsync(
        Guid idFonctionnaire,
        Guid idStatut,
        DateOnly dateDebut,
        Guid? idDocument,
        CancellationToken ct = default)
    {
        var fonctionnaire = await _db.Fonctionnaires
            .FirstOrDefaultAsync(f => f.Id == idFonctionnaire, ct)
            ?? throw new RegleMetierException("الموظف غير موجود");

        if (!await _db.StatutAdministratifs.AnyAsync(s => s.Id == idStatut, ct))
            throw new RegleMetierException("الوضع الإداري غير موجود");
        if (idDocument.HasValue &&
            !await _db.PieceJointes.AnyAsync(p => p.Id == idDocument.Value, ct))
            throw new RegleMetierException("المستند غير موجود");

        if (dateDebut < fonctionnaire.DateRecrutement)
            throw new RegleMetierException("التاريخ لا يمكن أن يسبق تاريخ التوظيف");

        var entity = new SituationAdministrative
        {
            Id = Guid.NewGuid(),
            IdFonctionnaire = idFonctionnaire,
            IdStatut = idStatut,
            DateDebut = dateDebut,
            DateFin = null,
            IdDocument = idDocument
        };

        return await _periodes.CloseAndAddAsync(
            _db.SituationAdministratives,
            entity,
            e => e.IdFonctionnaire == idFonctionnaire,
            $"rh:periode:SituationAdministrative:{idFonctionnaire}",
            ct);
    }

    private static SituationAdministrativeDto ToDto(SituationAdministrative s) =>
        new(s.Id, s.IdFonctionnaire, s.IdStatut, s.DateDebut, s.DateFin, s.IdDocument);
}

public class HistoriqueAffectationService
{
    private readonly IRHOrganisationDbContext _db;
    private readonly PeriodeService _periodes;

    public HistoriqueAffectationService(IRHOrganisationDbContext db, PeriodeService periodes)
    {
        _db = db;
        _periodes = periodes;
    }

    public async Task<Guid> AddAsync(
        CreateHistoriqueAffectationCommand cmd,
        CancellationToken ct = default)
    {
        Domain.RHOrganisation.Rules.AffectationRules.Validate(
            cmd.IdTribunalAffectation,
            cmd.IdUniteAffectation,
            cmd.IdUniteTravail,
            cmd.SectionTribunal);

        if (!await _db.Fonctionnaires.AnyAsync(f => f.Id == cmd.IdFonctionnaire, ct))
            throw new RegleMetierException("الموظف غير موجود");
        if (cmd.IdTribunalAffectation.HasValue &&
            !await _db.Tribunaux.AnyAsync(t => t.Id == cmd.IdTribunalAffectation.Value, ct))
            throw new RegleMetierException("المحكمة غير موجودة");
        if (cmd.IdUniteAffectation.HasValue &&
            !await _db.UnitesOrganisationnelles.AnyAsync(u => u.Id == cmd.IdUniteAffectation.Value, ct))
            throw new RegleMetierException("الهيكل غير موجود");
        if (cmd.IdUniteTravail.HasValue &&
            !await _db.UnitesOrganisationnelles.AnyAsync(u => u.Id == cmd.IdUniteTravail.Value, ct))
            throw new RegleMetierException("هيكل العمل غير موجود");
        if (cmd.IdDocument.HasValue &&
            !await _db.PieceJointes.AnyAsync(p => p.Id == cmd.IdDocument.Value, ct))
            throw new RegleMetierException("المستند غير موجود");

        var entity = new HistoriqueAffectation
        {
            Id = Guid.NewGuid(),
            IdFonctionnaire = cmd.IdFonctionnaire,
            IdTribunalAffectation = cmd.IdTribunalAffectation,
            IdUniteAffectation = cmd.IdUniteAffectation,
            IdUniteTravail = cmd.IdUniteTravail,
            SectionTribunal = cmd.SectionTribunal,
            DateDebut = cmd.DateDebut,
            DateFin = cmd.DateFin,
            IdDocument = cmd.IdDocument
        };

        return await _periodes.AddAsync(
            _db.HistoriqueAffectations,
            entity,
            e => e.IdFonctionnaire == cmd.IdFonctionnaire,
            $"rh:periode:HistoriqueAffectation:{cmd.IdFonctionnaire}",
            ct);
    }

    public async Task<IReadOnlyList<HistoriqueAffectationDto>> ListByFonctionnaireAsync(
        Guid idFonctionnaire,
        CancellationToken ct = default)
    {
        var list = await _db.HistoriqueAffectations.AsNoTracking()
            .Where(h => h.IdFonctionnaire == idFonctionnaire)
            .OrderBy(h => h.DateDebut)
            .ToListAsync(ct);

        return list.Select(ToDto).ToList();
    }

    /// <summary>
    /// Clôture l'affectation ouverte (si elle existe) et ouvre la nouvelle.
    /// </summary>
    public async Task<Guid> ChangeAsync(
        Guid idFonctionnaire,
        Guid? idTribunalAffectation,
        Guid? idUniteAffectation,
        Guid? idUniteTravail,
        Domain.RHOrganisation.Enums.SectionTribunal? sectionTribunal,
        DateOnly dateDebut,
        Guid? idDocument,
        CancellationToken ct = default)
    {
        Domain.RHOrganisation.Rules.AffectationRules.Validate(
            idTribunalAffectation,
            idUniteAffectation,
            idUniteTravail,
            sectionTribunal);

        if (!await _db.Fonctionnaires.AnyAsync(f => f.Id == idFonctionnaire, ct))
            throw new RegleMetierException("الموظف غير موجود");
        if (idTribunalAffectation.HasValue &&
            !await _db.Tribunaux.AnyAsync(t => t.Id == idTribunalAffectation.Value, ct))
            throw new RegleMetierException("المحكمة غير موجودة");
        if (idUniteAffectation.HasValue &&
            !await _db.UnitesOrganisationnelles.AnyAsync(u => u.Id == idUniteAffectation.Value, ct))
            throw new RegleMetierException("الهيكل غير موجود");
        if (idUniteTravail.HasValue &&
            !await _db.UnitesOrganisationnelles.AnyAsync(u => u.Id == idUniteTravail.Value, ct))
            throw new RegleMetierException("هيكل العمل غير موجود");
        if (idDocument.HasValue &&
            !await _db.PieceJointes.AnyAsync(p => p.Id == idDocument.Value, ct))
            throw new RegleMetierException("المستند غير موجود");

        var entity = new HistoriqueAffectation
        {
            Id = Guid.NewGuid(),
            IdFonctionnaire = idFonctionnaire,
            IdTribunalAffectation = idTribunalAffectation,
            IdUniteAffectation = idUniteAffectation,
            IdUniteTravail = idUniteTravail,
            SectionTribunal = sectionTribunal,
            DateDebut = dateDebut,
            DateFin = null,
            IdDocument = idDocument
        };

        return await _periodes.CloseAndAddAsync(
            _db.HistoriqueAffectations,
            entity,
            e => e.IdFonctionnaire == idFonctionnaire,
            $"rh:periode:HistoriqueAffectation:{idFonctionnaire}",
            ct);
    }

    private static HistoriqueAffectationDto ToDto(HistoriqueAffectation h) =>
        new(h.Id, h.IdFonctionnaire, h.IdTribunalAffectation, h.IdUniteAffectation,
            h.IdUniteTravail, h.SectionTribunal, h.DateDebut, h.DateFin, h.IdDocument);
}

public class HistoriqueResponsabiliteService
{
    private readonly IRHOrganisationDbContext _db;
    private readonly PeriodeService _periodes;

    public HistoriqueResponsabiliteService(IRHOrganisationDbContext db, PeriodeService periodes)
    {
        _db = db;
        _periodes = periodes;
    }

    public async Task<Guid> AddAsync(
        CreateHistoriqueResponsabiliteCommand cmd,
        CancellationToken ct = default)
    {
        var exactementUn = cmd.IdTribunal.HasValue ^ cmd.IdUniteOrganisationnelle.HasValue;
        if (!exactementUn)
            throw new RegleMetierException("يجب تحديد المحكمة أو الهيكل بدقة واحدة");

        if (!await _db.Fonctionnaires.AnyAsync(f => f.Id == cmd.IdFonctionnaire, ct))
            throw new RegleMetierException("الموظف غير موجود");
        if (!await _db.Responsabilites.AnyAsync(r => r.Id == cmd.IdResponsabilite, ct))
            throw new RegleMetierException("المسؤولية غير موجودة");
        if (cmd.IdTribunal.HasValue &&
            !await _db.Tribunaux.AnyAsync(t => t.Id == cmd.IdTribunal.Value, ct))
            throw new RegleMetierException("المحكمة غير موجودة");
        if (cmd.IdUniteOrganisationnelle.HasValue &&
            !await _db.UnitesOrganisationnelles.AnyAsync(u => u.Id == cmd.IdUniteOrganisationnelle.Value, ct))
            throw new RegleMetierException("الهيكل غير موجود");
        if (cmd.IdDocument.HasValue &&
            !await _db.PieceJointes.AnyAsync(p => p.Id == cmd.IdDocument.Value, ct))
            throw new RegleMetierException("المستند غير موجود");

        var entity = new HistoriqueResponsabilite
        {
            Id = Guid.NewGuid(),
            IdFonctionnaire = cmd.IdFonctionnaire,
            IdResponsabilite = cmd.IdResponsabilite,
            IdTribunal = cmd.IdTribunal,
            IdUniteOrganisationnelle = cmd.IdUniteOrganisationnelle,
            DateDebut = cmd.DateDebut,
            DateFin = cmd.DateFin,
            IdDocument = cmd.IdDocument
        };

        var lockKey =
            $"rh:periode:HistoriqueResponsabilite:{cmd.IdFonctionnaire}:{cmd.IdResponsabilite}:" +
            $"{cmd.IdTribunal?.ToString() ?? "null"}:{cmd.IdUniteOrganisationnelle?.ToString() ?? "null"}";

        return await _periodes.AddAsync(
            _db.HistoriqueResponsabilites,
            entity,
            e => e.IdFonctionnaire == cmd.IdFonctionnaire
                && e.IdResponsabilite == cmd.IdResponsabilite
                && e.IdTribunal == cmd.IdTribunal
                && e.IdUniteOrganisationnelle == cmd.IdUniteOrganisationnelle,
            lockKey,
            ct);
    }

    public async Task<IReadOnlyList<HistoriqueResponsabiliteDto>> ListByFonctionnaireAsync(
        Guid idFonctionnaire,
        CancellationToken ct = default)
    {
        var list = await _db.HistoriqueResponsabilites.AsNoTracking()
            .Where(h => h.IdFonctionnaire == idFonctionnaire)
            .OrderBy(h => h.DateDebut)
            .ToListAsync(ct);

        return list.Select(ToDto).ToList();
    }

    /// <summary>Clôture une responsabilité ouverte.</summary>
    public async Task CloseAsync(Guid id, DateOnly dateFin, CancellationToken ct = default)
    {
        var entity = await _db.HistoriqueResponsabilites
            .FirstOrDefaultAsync(h => h.Id == id, ct)
            ?? throw new RegleMetierException("السجل غير موجود");

        if (entity.DateFin is not null)
            throw new RegleMetierException("هذا السجل مغلق مسبقاً");
        if (dateFin < entity.DateDebut)
            throw new RegleMetierException("تاريخ النهاية يجب أن يلي تاريخ البداية");

        entity.DateFin = dateFin;
        await _db.SaveChangesAsync(ct);
    }

    private static HistoriqueResponsabiliteDto ToDto(HistoriqueResponsabilite h) =>
        new(h.Id, h.IdFonctionnaire, h.IdResponsabilite, h.IdTribunal,
            h.IdUniteOrganisationnelle, h.DateDebut, h.DateFin, h.IdDocument);
}
