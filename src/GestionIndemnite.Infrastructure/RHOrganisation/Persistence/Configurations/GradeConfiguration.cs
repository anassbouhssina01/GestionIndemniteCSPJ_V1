using GestionIndemnite.Domain.RHOrganisation.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionIndemnite.Infrastructure.RHOrganisation.Persistence.Configurations;

public class GradeConfiguration : IEntityTypeConfiguration<Grade>
{
    public void Configure(EntityTypeBuilder<Grade> builder)
    {
        builder.ToTable("Grade");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.NumeroReferencie).IsRequired();

        builder.Property(e => e.LibelleGradeAr)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.LibelleGradeFr)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(1000);

        builder.HasOne<Cadre>()
            .WithMany()
            .HasForeignKey(e => e.IdCadre)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Grade_Cadre");

        builder.HasIndex(e => new { e.IdCadre, e.NumeroReferencie })
            .IsUnique()
            .HasDatabaseName("UX_Grade_IdCadre_NumeroReferencie")
            .HasFilter("[IsDeleted] = 0");
    }
}
