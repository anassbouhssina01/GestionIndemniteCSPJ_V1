namespace GestionIndemnite.Application.RHOrganisation.DTOs;

public record PaysDto(
    Guid Id,
    string CodePays,
    string NomPaysAr,
    string NomPaysFr,
    bool EstLocal,
    bool EstActif);

public record CreatePaysCommand(
    string CodePays,
    string NomPaysAr,
    string NomPaysFr,
    bool EstLocal);

public record UpdatePaysCommand(
    string NomPaysAr,
    string NomPaysFr,
    bool EstLocal,
    bool EstActif);
