using GestionIndemnite.Domain.RHOrganisation.Common;

namespace GestionIndemnite.Domain.RHOrganisation.Entities;

public class Ville : AuditableEntity
{
    public string NomVille { get; set; } = string.Empty;
    public string NomVilleAr { get; set; } = string.Empty;
    public Guid IdPays { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool IsActive { get; set; } = true;
}
