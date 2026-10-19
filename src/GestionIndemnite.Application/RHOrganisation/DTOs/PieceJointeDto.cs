namespace GestionIndemnite.Application.RHOrganisation.DTOs;

public record PieceJointeDto(
    Guid Id,
    string NumeroDocument,
    DateOnly DateDocument,
    int AnneeDocument,
    string Emetteur,
    string TypeDocument,
    string Objet,
    string? NomFichier,
    string? HashFichier,
    DateOnly DateAjout);

public record UploadPieceJointeCommand(
    string NumeroDocument,
    DateOnly DateDocument,
    string Emetteur,
    string TypeDocument,
    string Objet);
