using GestionIndemnite.Domain.RHOrganisation.Enums;

namespace GestionIndemnite.Application.RHOrganisation.DTOs;

public record HistoriqueCadreGradeDto(
    Guid Id,
    Guid IdFonctionnaire,
    Guid IdGrade,
    DateOnly DateDebut,
    DateOnly? DateFin,
    Guid? IdDocument);

public record CreateHistoriqueCadreGradeCommand(
    Guid IdFonctionnaire,
    Guid IdGrade,
    DateOnly DateDebut,
    DateOnly? DateFin,
    Guid? IdDocument);

public record SituationAdministrativeDto(
    Guid Id,
    Guid IdFonctionnaire,
    Guid IdStatut,
    DateOnly DateDebut,
    DateOnly? DateFin,
    Guid? IdDocument);

public record CreateSituationAdministrativeCommand(
    Guid IdFonctionnaire,
    Guid IdStatut,
    DateOnly DateDebut,
    DateOnly? DateFin,
    Guid? IdDocument);

public record HistoriqueAffectationDto(
    Guid Id,
    Guid IdFonctionnaire,
    Guid? IdTribunalAffectation,
    Guid? IdUniteAffectation,
    Guid? IdUniteTravail,
    SectionTribunal? SectionTribunal,
    DateOnly DateDebut,
    DateOnly? DateFin,
    Guid? IdDocument);

public record CreateHistoriqueAffectationCommand(
    Guid IdFonctionnaire,
    Guid? IdTribunalAffectation,
    Guid? IdUniteAffectation,
    Guid? IdUniteTravail,
    SectionTribunal? SectionTribunal,
    DateOnly DateDebut,
    DateOnly? DateFin,
    Guid? IdDocument);

public record HistoriqueResponsabiliteDto(
    Guid Id,
    Guid IdFonctionnaire,
    Guid IdResponsabilite,
    Guid? IdTribunal,
    Guid? IdUniteOrganisationnelle,
    DateOnly DateDebut,
    DateOnly? DateFin,
    Guid? IdDocument);

public record CreateHistoriqueResponsabiliteCommand(
    Guid IdFonctionnaire,
    Guid IdResponsabilite,
    Guid? IdTribunal,
    Guid? IdUniteOrganisationnelle,
    DateOnly DateDebut,
    DateOnly? DateFin,
    Guid? IdDocument);

public record ChangeParentTribunalCommand(
    Guid? IdTribunalParent,
    DateOnly DateDebut,
    Guid? IdDocument);

public record ChangeParentUniteCommand(
    Guid? IdUniteParent,
    DateOnly DateDebut,
    Guid? IdDocument);
