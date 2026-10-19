using GestionIndemnite.Application.RHOrganisation.Common;
using GestionIndemnite.Infrastructure.RHOrganisation.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionIndemnite.Tests.Infrastructure;

public class TestDbContext : RHOrganisationDbContext
{
    public TestDbContext(
        DbContextOptions<RHOrganisationDbContext> options,
        ICurrentUser currentUser)
        : base(options, currentUser)
    {
    }

    public DbSet<PeriodeTest> PeriodesTest => Set<PeriodeTest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PeriodeTest>(e =>
        {
            e.ToTable("PeriodeTest");
            e.HasKey(x => x.Id);
            e.Property(x => x.IdCle).IsRequired();

            e.HasIndex(x => new { x.IdCle, x.DateFin })
                .IsUnique()
                .HasDatabaseName("UX_PeriodeTest_Ouvert")
                .HasFilter("[DateFin] IS NULL AND [IsDeleted] = 0");

            e.HasIndex(x => new { x.IdCle, x.DateDebut, x.DateFin })
                .HasDatabaseName("IX_PeriodeTest_Cle_Periode");

            e.ToTable(t => t.HasCheckConstraint(
                "CK_PeriodeTest_Periode",
                "[DateFin] IS NULL OR [DateDebut] <= [DateFin]"));

            e.HasQueryFilter(x => !x.IsDeleted);
        });
    }
}
