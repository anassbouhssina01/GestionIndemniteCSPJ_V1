using GestionIndemnite.Domain.RHOrganisation.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionIndemnite.Infrastructure.RHOrganisation.Persistence.Configurations;

public class FonctionnaireConfiguration : IEntityTypeConfiguration<Fonctionnaire>
{
    public void Configure(EntityTypeBuilder<Fonctionnaire> builder)
    {
        builder.ToTable("Fonctionnaire");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Matricule)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.Cin)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.NomAr)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.PrenomAr)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.NomFr)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.PrenomFr)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Genre)
            .HasConversion<int>();

        builder.HasIndex(e => e.Matricule)
            .IsUnique()
            .HasDatabaseName("UX_Fonctionnaire_Matricule")
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(e => e.Cin)
            .IsUnique()
            .HasDatabaseName("UX_Fonctionnaire_Cin")
            .HasFilter("[IsDeleted] = 0");
    }
}
