using GestionIndemnite.Domain.RHOrganisation.Enums;

namespace GestionIndemnite.Application.RHOrganisation.DTOs;

public record FonctionnaireDto(
    Guid Id,
    string Matricule,
    string Cin,
    string NomAr,
    string PrenomAr,
    string NomFr,
    string PrenomFr,
    DateOnly DateNaissance,
    DateOnly DateRecrutement,
    Genre Genre);

public record CreateFonctionnaireCommand(
    string Matricule,
    string Cin,
    string NomAr,
    string PrenomAr,
    string NomFr,
    string PrenomFr,
    DateOnly DateNaissance,
    DateOnly DateRecrutement,
    Genre Genre);

public record UpdateFonctionnaireCommand(
    string NomAr,
    string PrenomAr,
    string NomFr,
    string PrenomFr,
    DateOnly DateNaissance,
    DateOnly DateRecrutement,
    Genre Genre);
