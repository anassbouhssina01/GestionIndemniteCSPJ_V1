using GestionIndemnite.Domain.RHOrganisation.Common;
using GestionIndemnite.Domain.RHOrganisation.Enums;

namespace GestionIndemnite.Domain.RHOrganisation.Entities;

public class Tribunal : AuditableEntity
{
    public string NomTribunal { get; set; } = string.Empty;
    public string NomTribunalAr { get; set; } = string.Empty;
    public TypeTribunal TypeTribunal { get; set; }
    public NiveauJuridiction NiveauJuridiction { get; set; }
    public Guid IdVille { get; set; }
    public bool IsActive { get; set; }
}

public class UniteOrganisationnelle : AuditableEntity
{
    public string NomUnite { get; set; } = string.Empty;
    public string NomUniteAr { get; set; } = string.Empty;
    public TypeUnite TypeUnite { get; set; }
    public Guid IdVille { get; set; }
    public bool IsActive { get; set; }
}

public class HistoriqueOrganigrammeTribunal : AuditableEntity, IPeriode
{
    public Guid IdTribunal { get; set; }
    public Guid? IdTribunalParent { get; set; }
    public DateOnly DateDebut { get; set; }
    public DateOnly? DateFin { get; set; }
    public Guid? IdDocument { get; set; }
}

public class HistoriqueOrganigrammeUnite : AuditableEntity, IPeriode
{
    public Guid IdUnite { get; set; }
    public Guid? IdUniteParent { get; set; }
    public DateOnly DateDebut { get; set; }
    public DateOnly? DateFin { get; set; }
    public Guid? IdDocument { get; set; }
}
