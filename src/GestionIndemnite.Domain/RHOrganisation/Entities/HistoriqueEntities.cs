using GestionIndemnite.Domain.RHOrganisation.Common;
using GestionIndemnite.Domain.RHOrganisation.Enums;

namespace GestionIndemnite.Domain.RHOrganisation.Entities;

public class HistoriqueCadreGrade : AuditableEntity, IPeriode
{
    public Guid IdFonctionnaire { get; set; }
    public Guid IdGrade { get; set; }
    public DateOnly DateDebut { get; set; }
    public DateOnly? DateFin { get; set; }
    public Guid? IdDocument { get; set; }
}

public class SituationAdministrative : AuditableEntity, IPeriode
{
    public Guid IdFonctionnaire { get; set; }
    public Guid IdStatut { get; set; }
    public string? MatriculeCspj { get; set; }
    public DateOnly DateDebut { get; set; }
    public DateOnly? DateFin { get; set; }
    public Guid? IdDocument { get; set; }
}

public class HistoriqueAffectation : AuditableEntity, IPeriode
{
    public Guid IdFonctionnaire { get; set; }
    public Guid? IdTribunalAffectation { get; set; }
    public Guid? IdUniteAffectation { get; set; }
    public Guid? IdUniteTravail { get; set; }
    public SectionTribunal? SectionTribunal { get; set; }
    public DateOnly DateDebut { get; set; }
    public DateOnly? DateFin { get; set; }
    public Guid? IdDocument { get; set; }
}

public class HistoriqueResponsabilite : AuditableEntity, IPeriode
{
    public Guid IdFonctionnaire { get; set; }
    public Guid IdResponsabilite { get; set; }
    public Guid? IdTribunal { get; set; }
    public Guid? IdUniteOrganisationnelle { get; set; }
    public DateOnly DateDebut { get; set; }
    public DateOnly? DateFin { get; set; }
    public Guid? IdDocument { get; set; }
}
