using GestionIndemnite.Domain.RHOrganisation.Common;

namespace GestionIndemnite.Tests.Infrastructure;

public class PeriodeTest : AuditableEntity, IPeriode
{
    public Guid IdCle { get; set; }
    public DateOnly DateDebut { get; set; }
    public DateOnly? DateFin { get; set; }
    public Guid? IdDocument { get; set; }
}
