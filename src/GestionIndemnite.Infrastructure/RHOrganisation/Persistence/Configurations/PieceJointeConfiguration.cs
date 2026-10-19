using GestionIndemnite.Domain.RHOrganisation.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionIndemnite.Infrastructure.RHOrganisation.Persistence.Configurations;

public class PieceJointeConfiguration : IEntityTypeConfiguration<PieceJointe>
{
    public void Configure(EntityTypeBuilder<PieceJointe> builder)
    {
        builder.ToTable("PieceJointe");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.NumeroDocument)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.NumeroDocumentNormalise)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Emetteur)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(e => e.EmetteurNormalise)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(e => e.TypeDocument)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Objet)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.NomFichier)
            .HasMaxLength(255);

        builder.Property(e => e.CheminFichier)
            .HasMaxLength(500);

        builder.Property(e => e.HashFichier)
            .HasMaxLength(64);

        builder.HasIndex(e => new { e.NumeroDocumentNormalise, e.AnneeDocument, e.EmetteurNormalise })
            .IsUnique()
            .HasDatabaseName("UX_PieceJointe_Cle")
            .HasFilter("[IsDeleted] = 0");
    }
}
