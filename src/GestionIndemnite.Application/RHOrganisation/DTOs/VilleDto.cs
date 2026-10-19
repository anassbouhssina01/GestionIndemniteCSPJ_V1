namespace GestionIndemnite.Application.RHOrganisation.DTOs;

public record VilleDto(
    Guid Id,
    string NomVille,
    Guid IdPays,
    decimal? Latitude,
    decimal? Longitude,
    bool IsActive,
    string NomVilleAr);

public record CreateVilleCommand(
    string NomVille,
    Guid IdPays,
    decimal? Latitude,
    decimal? Longitude)
{
    public string? NomVilleAr { get; init; }
}

public record UpdateVilleCommand(
    string NomVille,
    decimal? Latitude,
    decimal? Longitude,
    bool IsActive)
{
    public string? NomVilleAr { get; init; }
}
