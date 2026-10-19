namespace GestionIndemnite.Application.RHOrganisation.DTOs;

public enum TypeHistoriqueRapport
{
    CadreGrade = 0,
    SituationAdministrative = 1,
    Affectation = 2,
    Responsabilite = 3,
    OrganigrammeTribunal = 4,
    OrganigrammeUnite = 5
}

public record TrouDto(
    TypeHistoriqueRapport Type,
    Guid IdCle,
    DateOnly DateDebut,
    DateOnly DateFin);

public record SansJustificatifDto(
    TypeHistoriqueRapport Type,
    Guid Id,
    Guid IdCle,
    DateOnly DateDebut,
    DateOnly? DateFin);

public record ChevauchementDto(
    TypeHistoriqueRapport Type,
    Guid IdCle,
    DateOnly DateDebut1,
    DateOnly? DateFin1,
    DateOnly DateDebut2,
    DateOnly? DateFin2);
