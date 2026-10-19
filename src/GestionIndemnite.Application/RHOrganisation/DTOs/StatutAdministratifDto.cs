namespace GestionIndemnite.Application.RHOrganisation.DTOs;

public record StatutAdministratifDto(
    Guid Id,
    string CodeStatut,
    string LibelleStatutAr,
    string LibelleStatutFr,
    string? Description,
    bool EstActif);

public record CreateStatutAdministratifCommand(
    string CodeStatut,
    string LibelleStatutAr,
    string LibelleStatutFr,
    string? Description);

public record UpdateStatutAdministratifCommand(
    string LibelleStatutAr,
    string LibelleStatutFr,
    string? Description,
    bool EstActif);
