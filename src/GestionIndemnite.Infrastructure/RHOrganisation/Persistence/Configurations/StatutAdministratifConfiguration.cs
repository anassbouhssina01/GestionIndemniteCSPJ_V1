using GestionIndemnite.Domain.RHOrganisation.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionIndemnite.Infrastructure.RHOrganisation.Persistence.Configurations;

public class StatutAdministratifConfiguration : IEntityTypeConfiguration<StatutAdministratif>
{
    public void Configure(EntityTypeBuilder<StatutAdministratif> builder)
    {
        builder.ToTable("StatutAdministratif");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.CodeStatut)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.LibelleStatutAr)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.LibelleStatutFr)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.HasIndex(e => e.CodeStatut)
            .IsUnique()
            .HasDatabaseName("UX_StatutAdministratif_Code")
            .HasFilter("[IsDeleted] = 0");
    }
}
