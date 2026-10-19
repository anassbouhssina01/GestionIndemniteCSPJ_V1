using GestionIndemnite.Domain.RHOrganisation.Common;

namespace GestionIndemnite.Domain.RHOrganisation.Entities;

public class PieceJointe : AuditableEntity
{
    public string NumeroDocument { get; set; } = string.Empty;
    public string NumeroDocumentNormalise { get; set; } = string.Empty;
    public DateOnly DateDocument { get; set; }
    public int AnneeDocument { get; set; }
    public string Emetteur { get; set; } = string.Empty;
    public string EmetteurNormalise { get; set; } = string.Empty;
    public string TypeDocument { get; set; } = string.Empty;
    public string Objet { get; set; } = string.Empty;
    public string? NomFichier { get; set; }
    public string? CheminFichier { get; set; }
    public string? HashFichier { get; set; }
    public DateOnly DateAjout { get; set; }
}
