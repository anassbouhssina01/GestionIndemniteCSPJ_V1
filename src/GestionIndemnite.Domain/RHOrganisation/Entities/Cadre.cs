using GestionIndemnite.Domain.RHOrganisation.Common;

namespace GestionIndemnite.Domain.RHOrganisation.Entities;

public class Cadre : AuditableEntity
{
    public string CodeCadre { get; set; } = string.Empty;
    public string LibelleCadreAr { get; set; } = string.Empty;
    public string LibelleCadreFr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool EstActif { get; set; } = true;
}
