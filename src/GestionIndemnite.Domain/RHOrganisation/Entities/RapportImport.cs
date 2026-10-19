using GestionIndemnite.Domain.RHOrganisation.Common;

namespace GestionIndemnite.Domain.RHOrganisation.Entities;

public class RapportImport : AuditableEntity
{
    public string NomFichier { get; set; } = string.Empty;
    public bool ModeDryRun { get; set; }
    public int NombreLignes { get; set; }
    public int NombreErreurs { get; set; }
    public string ContenuRapport { get; set; } = string.Empty;
}
