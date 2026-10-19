using GestionIndemnite.Domain.RHOrganisation.Common;

namespace GestionIndemnite.Domain.RHOrganisation.Entities;

public class Grade : AuditableEntity
{
    public Guid IdCadre { get; set; }
    public int NumeroReferencie { get; set; }
    public string LibelleGradeAr { get; set; } = string.Empty;
    public string LibelleGradeFr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool EstActif { get; set; } = true;
}
