namespace GestionIndemnite.Application.RHOrganisation.DTOs;

public record CadreDto(
    Guid Id,
    string CodeCadre,
    string LibelleCadreAr,
    string LibelleCadreFr,
    string? Description,
    bool EstActif);

public record CreateCadreCommand(
    string CodeCadre,
    string LibelleCadreAr,
    string LibelleCadreFr,
    string? Description);

public record UpdateCadreCommand(
    string LibelleCadreAr,
    string LibelleCadreFr,
    string? Description,
    bool EstActif);
