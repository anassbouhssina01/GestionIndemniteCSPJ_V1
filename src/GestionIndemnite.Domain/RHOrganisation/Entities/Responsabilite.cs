using GestionIndemnite.Domain.RHOrganisation.Common;

namespace GestionIndemnite.Domain.RHOrganisation.Entities;

public class Responsabilite : AuditableEntity
{
    public string CodeResponsabilite { get; set; } = string.Empty;
    public string LibelleResponsabiliteAr { get; set; } = string.Empty;
    public string LibelleResponsabiliteFr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool EstActif { get; set; } = true;
}
