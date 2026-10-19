using GestionIndemnite.Domain.RHOrganisation.Enums;

namespace GestionIndemnite.Application.RHOrganisation.DTOs;

public record TribunalDto(
    Guid Id,
    string NomTribunal,
    TypeTribunal TypeTribunal,
    NiveauJuridiction NiveauJuridiction,
    Guid IdVille,
    bool IsActive,
    string NomTribunalAr);

public record CreateTribunalCommand(
    string NomTribunal,
    TypeTribunal TypeTribunal,
    NiveauJuridiction NiveauJuridiction,
    Guid IdVille,
    Guid? IdTribunalParent)
{
    public string? NomTribunalAr { get; init; }
}

public record UniteOrganisationnelleDto(
    Guid Id,
    string NomUnite,
    TypeUnite TypeUnite,
    Guid IdVille,
    bool IsActive,
    string NomUniteAr);

public record CreateUniteOrganisationnelleCommand(
    string NomUnite,
    TypeUnite TypeUnite,
    Guid IdVille,
    Guid? IdUniteParent)
{
    public string? NomUniteAr { get; init; }
}

public record ParentOrganigrammeDto(
    Guid Id,
    string Nom,
    DateOnly DateDebut,
    DateOnly? DateFin);
