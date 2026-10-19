using GestionIndemnite.Domain.RHOrganisation.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionIndemnite.Infrastructure.RHOrganisation.Persistence.Configurations;

public class CadreConfiguration : IEntityTypeConfiguration<Cadre>
{
    public void Configure(EntityTypeBuilder<Cadre> builder)
    {
        builder.ToTable("Cadre");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.CodeCadre)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.LibelleCadreAr)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.LibelleCadreFr)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.HasIndex(e => e.CodeCadre)
            .IsUnique()
            .HasDatabaseName("UX_Cadre_CodeCadre")
            .HasFilter("[IsDeleted] = 0");
    }
}
