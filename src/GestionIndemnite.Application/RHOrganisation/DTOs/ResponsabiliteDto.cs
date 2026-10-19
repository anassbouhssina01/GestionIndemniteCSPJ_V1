namespace GestionIndemnite.Application.RHOrganisation.DTOs;

public record ResponsabiliteDto(
    Guid Id,
    string CodeResponsabilite,
    string LibelleResponsabiliteAr,
    string LibelleResponsabiliteFr,
    string? Description,
    bool EstActif);

public record CreateResponsabiliteCommand(
    string CodeResponsabilite,
    string LibelleResponsabiliteAr,
    string LibelleResponsabiliteFr,
    string? Description);

public record UpdateResponsabiliteCommand(
    string LibelleResponsabiliteAr,
    string LibelleResponsabiliteFr,
    string? Description,
    bool EstActif);
