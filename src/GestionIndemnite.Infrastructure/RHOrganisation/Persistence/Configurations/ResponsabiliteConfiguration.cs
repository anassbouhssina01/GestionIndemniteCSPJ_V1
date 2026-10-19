using GestionIndemnite.Domain.RHOrganisation.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionIndemnite.Infrastructure.RHOrganisation.Persistence.Configurations;

public class ResponsabiliteConfiguration : IEntityTypeConfiguration<Responsabilite>
{
    public void Configure(EntityTypeBuilder<Responsabilite> builder)
    {
        builder.ToTable("Responsabilite");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.CodeResponsabilite)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.LibelleResponsabiliteAr)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.LibelleResponsabiliteFr)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.HasIndex(e => e.CodeResponsabilite)
            .IsUnique()
            .HasDatabaseName("UX_Responsabilite_Code")
            .HasFilter("[IsDeleted] = 0");
    }
}
