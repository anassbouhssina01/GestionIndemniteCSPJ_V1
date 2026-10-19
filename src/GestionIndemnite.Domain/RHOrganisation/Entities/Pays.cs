using GestionIndemnite.Domain.RHOrganisation.Common;

namespace GestionIndemnite.Domain.RHOrganisation.Entities;

public class Pays : AuditableEntity
{
    public string CodePays { get; set; } = string.Empty;
    public string NomPaysAr { get; set; } = string.Empty;
    public string NomPaysFr { get; set; } = string.Empty;
    public bool EstLocal { get; set; }
    public bool EstActif { get; set; } = true;
}
