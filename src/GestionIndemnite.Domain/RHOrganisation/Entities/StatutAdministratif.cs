using GestionIndemnite.Domain.RHOrganisation.Common;

namespace GestionIndemnite.Domain.RHOrganisation.Entities;

public class StatutAdministratif : AuditableEntity
{
    public string CodeStatut { get; set; } = string.Empty;
    public string LibelleStatutAr { get; set; } = string.Empty;
    public string LibelleStatutFr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool EstActif { get; set; } = true;
}
