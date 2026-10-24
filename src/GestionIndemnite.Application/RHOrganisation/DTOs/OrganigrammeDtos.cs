using GestionIndemnite.Domain.RHOrganisation.Enums;

namespace GestionIndemnite.Application.RHOrganisation.DTOs;

public record TribunalDto(
    Guid Id,
    string NomTribunal,
    TypeTribunal TypeTribunal,
    NiveauJuridiction NiveauJuridiction,
    Guid IdVille,
    bool IsActive,
    string NomTribunalAr)
{
    public string NumeroTribunal { get; init; } = string.Empty;
    public DateOnly? DateCreation { get; init; }
}

public record CreateTribunalCommand(
    string NomTribunal,
    TypeTribunal TypeTribunal,
    NiveauJuridiction NiveauJuridiction,
    Guid IdVille,
    Guid? IdTribunalParent)
{
    public string? NomTribunalAr { get; init; }
    public string? NumeroTribunal { get; init; }
    public DateOnly? DateCreation { get; init; }
}

public record UpdateTribunalCommand(
    string NomTribunal,
    TypeTribunal TypeTribunal,
    NiveauJuridiction NiveauJuridiction,
    Guid IdVille)
{
    public string? NomTribunalAr { get; init; }
    public string? NumeroTribunal { get; init; }
    public DateOnly? DateCreation { get; init; }
}

public record UniteOrganisationnelleDto(
    Guid Id,
    string NomUnite,
    TypeUnite TypeUnite,
    Guid IdVille,
    bool IsActive,
    string NomUniteAr)
{
    public DateOnly? DateCreation { get; init; }
}

public record CreateUniteOrganisationnelleCommand(
    string NomUnite,
    TypeUnite TypeUnite,
    Guid IdVille,
    Guid? IdUniteParent)
{
    public string? NomUniteAr { get; init; }
    public DateOnly? DateCreation { get; init; }
}

public record UpdateUniteOrganisationnelleCommand(
    string NomUnite,
    TypeUnite TypeUnite,
    Guid IdVille)
{
    public string? NomUniteAr { get; init; }
    public DateOnly? DateCreation { get; init; }
}

public record ParentOrganigrammeDto(
    Guid Id,
    string Nom,
    DateOnly DateDebut,
    DateOnly? DateFin);

public record HistoriqueRattachementDto(
    Guid IdTribunal,
    string NomTribunal,
    Guid? IdTribunalParent,
    string? NomTribunalParent,
    DateOnly DateDebut,
    DateOnly? DateFin);
