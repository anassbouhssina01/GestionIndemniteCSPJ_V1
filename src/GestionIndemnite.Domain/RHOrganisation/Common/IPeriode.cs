namespace GestionIndemnite.Domain.RHOrganisation.Common;

public interface IPeriode
{
    DateOnly DateDebut { get; set; }
    DateOnly? DateFin { get; set; }
    Guid? IdDocument { get; set; }
}
