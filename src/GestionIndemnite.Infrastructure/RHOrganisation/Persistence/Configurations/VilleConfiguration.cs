using GestionIndemnite.Domain.RHOrganisation.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionIndemnite.Infrastructure.RHOrganisation.Persistence.Configurations;

public class VilleConfiguration : IEntityTypeConfiguration<Ville>
{
    public void Configure(EntityTypeBuilder<Ville> builder)
    {
        builder.ToTable("Ville");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.NomVille)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.NomVilleAr)
            .HasMaxLength(100)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(e => e.Latitude)
            .HasPrecision(9, 6);

        builder.Property(e => e.Longitude)
            .HasPrecision(9, 6);

        builder.HasOne<Pays>()
            .WithMany()
            .HasForeignKey(e => e.IdPays)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Ville_Pays");

        builder.HasIndex(e => e.IdPays)
            .HasDatabaseName("IX_Ville_IdPays");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_Ville_Latitude",
                "[Latitude] IS NULL OR [Latitude] BETWEEN -90 AND 90");

            t.HasCheckConstraint(
                "CK_Ville_Longitude",
                "[Longitude] IS NULL OR [Longitude] BETWEEN -180 AND 180");
        });
    }
}
