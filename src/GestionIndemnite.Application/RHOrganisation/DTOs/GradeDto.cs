namespace GestionIndemnite.Application.RHOrganisation.DTOs;

public record GradeDto(
    Guid Id,
    Guid IdCadre,
    int NumeroReferencie,
    string LibelleGradeAr,
    string LibelleGradeFr,
    string? Description,
    bool EstActif);

public record CreateGradeCommand(
    Guid IdCadre,
    int NumeroReferencie,
    string LibelleGradeAr,
    string LibelleGradeFr,
    string? Description);

public record UpdateGradeCommand(
    int NumeroReferencie,
    string LibelleGradeAr,
    string LibelleGradeFr,
    string? Description,
    bool EstActif);
