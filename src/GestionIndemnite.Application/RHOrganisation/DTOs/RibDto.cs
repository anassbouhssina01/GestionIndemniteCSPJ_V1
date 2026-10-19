namespace GestionIndemnite.Application.RHOrganisation.DTOs;

public record RibDto(
    Guid Id,
    Guid IdFonctionnaire,
    string CodeBanque,
    string CodeVille,
    string NumeroCompte,
    string Cle,
    bool EstActif);

public record CreateRibCommand(
    string CodeBanque,
    string CodeVille,
    string NumeroCompte,
    string Cle);
