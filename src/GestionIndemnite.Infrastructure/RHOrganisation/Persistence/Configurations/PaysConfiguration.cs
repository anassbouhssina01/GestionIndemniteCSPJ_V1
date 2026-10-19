using GestionIndemnite.Domain.RHOrganisation.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionIndemnite.Infrastructure.RHOrganisation.Persistence.Configurations;

public class PaysConfiguration : IEntityTypeConfiguration<Pays>
{
    public void Configure(EntityTypeBuilder<Pays> builder)
    {
        builder.ToTable("Pays");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.CodePays)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.NomPaysAr)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.NomPaysFr)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(e => e.CodePays)
            .IsUnique()
            .HasDatabaseName("UX_Pays_CodePays")
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(e => e.EstLocal)
            .IsUnique()
            .HasDatabaseName("UX_Pays_EstLocal")
            .HasFilter("[EstLocal] = 1 AND [IsDeleted] = 0");
    }
}
