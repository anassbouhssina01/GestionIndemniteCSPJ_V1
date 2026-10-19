using GestionIndemnite.Domain.RHOrganisation.Common;
using GestionIndemnite.Domain.RHOrganisation.Enums;

namespace GestionIndemnite.Domain.RHOrganisation.Entities;

public class Fonctionnaire : AuditableEntity
{
    public string Matricule { get; set; } = string.Empty;
    public string Cin { get; set; } = string.Empty;
    public string NomAr { get; set; } = string.Empty;
    public string PrenomAr { get; set; } = string.Empty;
    public string NomFr { get; set; } = string.Empty;
    public string PrenomFr { get; set; } = string.Empty;
    public DateOnly DateNaissance { get; set; }
    public DateOnly DateRecrutement { get; set; }
    public Genre Genre { get; set; }
}
