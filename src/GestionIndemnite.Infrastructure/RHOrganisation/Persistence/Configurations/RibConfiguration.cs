using GestionIndemnite.Domain.RHOrganisation.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionIndemnite.Infrastructure.RHOrganisation.Persistence.Configurations;

public class RibConfiguration : IEntityTypeConfiguration<Rib>
{
    public void Configure(EntityTypeBuilder<Rib> builder)
    {
        builder.ToTable("Rib");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.CodeBanque)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(e => e.CodeVille)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(e => e.NumeroCompte)
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(e => e.Cle)
            .HasMaxLength(2)
            .IsRequired();

        builder.HasOne<Fonctionnaire>()
            .WithMany()
            .HasForeignKey(e => e.IdFonctionnaire)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Rib_Fonctionnaire");

        builder.HasIndex(e => e.IdFonctionnaire)
            .IsUnique()
            .HasDatabaseName("UX_Rib_Actif")
            .HasFilter("[EstActif] = 1 AND [IsDeleted] = 0");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_Rib_CodeBanque",
                "[CodeBanque] NOT LIKE '%[^0-9]%' AND LEN([CodeBanque]) = 3");

            t.HasCheckConstraint(
                "CK_Rib_CodeVille",
                "[CodeVille] NOT LIKE '%[^0-9]%' AND LEN([CodeVille]) = 3");

            t.HasCheckConstraint(
                "CK_Rib_NumeroCompte",
                "[NumeroCompte] NOT LIKE '%[^0-9]%' AND LEN([NumeroCompte]) = 16");

            t.HasCheckConstraint(
                "CK_Rib_Cle",
                "[Cle] NOT LIKE '%[^0-9]%' AND LEN([Cle]) = 2");
        });
    }
}
