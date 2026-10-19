using GestionIndemnite.Domain.RHOrganisation.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionIndemnite.Infrastructure.RHOrganisation.Persistence.Configurations;

public class RapportImportConfiguration : IEntityTypeConfiguration<RapportImport>
{
    public void Configure(EntityTypeBuilder<RapportImport> builder)
    {
        builder.ToTable("RapportImport");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.NomFichier)
            .HasMaxLength(260)
            .IsRequired();

        builder.Property(e => e.ModeDryRun)
            .IsRequired();

        builder.Property(e => e.NombreLignes)
            .IsRequired();

        builder.Property(e => e.NombreErreurs)
            .IsRequired();

        builder.Property(e => e.ContenuRapport)
            .IsRequired()
            .HasColumnType("NVARCHAR(MAX)");

        builder.HasIndex(e => e.CreatedAt)
            .HasDatabaseName("IX_RapportImport_CreatedAt");
    }
}
