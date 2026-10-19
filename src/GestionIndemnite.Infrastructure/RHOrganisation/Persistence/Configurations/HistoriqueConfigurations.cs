using GestionIndemnite.Domain.RHOrganisation.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionIndemnite.Infrastructure.RHOrganisation.Persistence.Configurations;

public class HistoriqueCadreGradeConfiguration : IEntityTypeConfiguration<HistoriqueCadreGrade>
{
    public void Configure(EntityTypeBuilder<HistoriqueCadreGrade> builder)
    {
        builder.ToTable("HistoriqueCadreGrade");

        builder.HasKey(e => e.Id);

        builder.HasOne<Fonctionnaire>()
            .WithMany()
            .HasForeignKey(e => e.IdFonctionnaire)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_HistCadreGrade_Fonctionnaire");

        builder.HasOne<Grade>()
            .WithMany()
            .HasForeignKey(e => e.IdGrade)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_HistCadreGrade_Grade");

        builder.HasOne<PieceJointe>()
            .WithMany()
            .HasForeignKey(e => e.IdDocument)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_HistCadreGrade_Document");

        builder.HasIndex(e => new { e.IdFonctionnaire, e.DateFin })
            .IsUnique()
            .HasDatabaseName("UX_HistCadreGrade_Ouvert")
            .HasFilter("[DateFin] IS NULL AND [IsDeleted] = 0");

        builder.HasIndex(e => new { e.IdFonctionnaire, e.DateDebut, e.DateFin })
            .HasDatabaseName("IX_HistCadreGrade_Cle_Periode");

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_HistCadreGrade_Periode",
            "[DateFin] IS NULL OR [DateDebut] <= [DateFin]"));
    }
}

public class SituationAdministrativeConfiguration : IEntityTypeConfiguration<SituationAdministrative>
{
    public void Configure(EntityTypeBuilder<SituationAdministrative> builder)
    {
        builder.ToTable("SituationAdministrative");

        builder.HasKey(e => e.Id);

        builder.HasOne<Fonctionnaire>()
            .WithMany()
            .HasForeignKey(e => e.IdFonctionnaire)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_SituationAdmin_Fonctionnaire");

        builder.HasOne<StatutAdministratif>()
            .WithMany()
            .HasForeignKey(e => e.IdStatut)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_SituationAdmin_Statut");

        builder.HasOne<PieceJointe>()
            .WithMany()
            .HasForeignKey(e => e.IdDocument)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_SituationAdmin_Document");

        builder.HasIndex(e => new { e.IdFonctionnaire, e.DateFin })
            .IsUnique()
            .HasDatabaseName("UX_SituationAdmin_Ouvert")
            .HasFilter("[DateFin] IS NULL AND [IsDeleted] = 0");

        builder.HasIndex(e => new { e.IdFonctionnaire, e.DateDebut, e.DateFin })
            .HasDatabaseName("IX_SituationAdmin_Cle_Periode");

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_SituationAdmin_Periode",
            "[DateFin] IS NULL OR [DateDebut] <= [DateFin]"));
    }
}

public class HistoriqueAffectationConfiguration : IEntityTypeConfiguration<HistoriqueAffectation>
{
    public void Configure(EntityTypeBuilder<HistoriqueAffectation> builder)
    {
        builder.ToTable("HistoriqueAffectation");

        builder.HasKey(e => e.Id);

        builder.HasOne<Fonctionnaire>()
            .WithMany()
            .HasForeignKey(e => e.IdFonctionnaire)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_HistAffectation_Fonctionnaire");

        builder.HasOne<Tribunal>()
            .WithMany()
            .HasForeignKey(e => e.IdTribunalAffectation)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_HistAffectation_Tribunal");

        builder.HasOne<UniteOrganisationnelle>()
            .WithMany()
            .HasForeignKey(e => e.IdUniteAffectation)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_HistAffectation_UniteAffectation");

        builder.HasOne<UniteOrganisationnelle>()
            .WithMany()
            .HasForeignKey(e => e.IdUniteTravail)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_HistAffectation_UniteTravail");

        builder.HasOne<PieceJointe>()
            .WithMany()
            .HasForeignKey(e => e.IdDocument)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_HistAffectation_Document");

        builder.HasIndex(e => new { e.IdFonctionnaire, e.DateFin })
            .IsUnique()
            .HasDatabaseName("UX_HistAffectation_Ouvert")
            .HasFilter("[DateFin] IS NULL AND [IsDeleted] = 0");

        builder.HasIndex(e => new { e.IdFonctionnaire, e.DateDebut, e.DateFin })
            .HasDatabaseName("IX_HistAffectation_Cle_Periode");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_HistAffectation_Periode",
                "[DateFin] IS NULL OR [DateDebut] <= [DateFin]");

            t.HasCheckConstraint(
                "CK_HistAffectation_ExactementUn",
                "([IdTribunalAffectation] IS NOT NULL AND [IdUniteAffectation] IS NULL) OR " +
                "([IdTribunalAffectation] IS NULL AND [IdUniteAffectation] IS NOT NULL)");

            t.HasCheckConstraint(
                "CK_HistAffectation_SectionTribunal",
                "[SectionTribunal] IS NULL OR [IdTribunalAffectation] IS NOT NULL");

            t.HasCheckConstraint(
                "CK_HistAffectation_UniteTravail",
                "[IdUniteTravail] IS NULL OR [IdTribunalAffectation] IS NOT NULL");
        });
    }
}

public class HistoriqueResponsabiliteConfiguration : IEntityTypeConfiguration<HistoriqueResponsabilite>
{
    public void Configure(EntityTypeBuilder<HistoriqueResponsabilite> builder)
    {
        builder.ToTable("HistoriqueResponsabilite");

        builder.HasKey(e => e.Id);

        builder.HasOne<Fonctionnaire>()
            .WithMany()
            .HasForeignKey(e => e.IdFonctionnaire)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_HistResp_Fonctionnaire");

        builder.HasOne<Responsabilite>()
            .WithMany()
            .HasForeignKey(e => e.IdResponsabilite)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_HistResp_Responsabilite");

        builder.HasOne<Tribunal>()
            .WithMany()
            .HasForeignKey(e => e.IdTribunal)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_HistResp_Tribunal");

        builder.HasOne<UniteOrganisationnelle>()
            .WithMany()
            .HasForeignKey(e => e.IdUniteOrganisationnelle)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_HistResp_Unite");

        builder.HasOne<PieceJointe>()
            .WithMany()
            .HasForeignKey(e => e.IdDocument)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_HistResp_Document");

        builder.HasIndex(e => new
            {
                e.IdFonctionnaire,
                e.IdResponsabilite,
                e.IdTribunal,
                e.IdUniteOrganisationnelle,
                e.DateFin
            })
            .IsUnique()
            .HasDatabaseName("UX_HistResp_Ouvert")
            .HasFilter("[DateFin] IS NULL AND [IsDeleted] = 0");

        builder.HasIndex(e => new { e.IdFonctionnaire, e.DateDebut, e.DateFin })
            .HasDatabaseName("IX_HistResp_Cle_Periode");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_HistResp_Periode",
                "[DateFin] IS NULL OR [DateDebut] <= [DateFin]");

            t.HasCheckConstraint(
                "CK_HistResp_ExactementUn",
                "([IdTribunal] IS NOT NULL AND [IdUniteOrganisationnelle] IS NULL) OR " +
                "([IdTribunal] IS NULL AND [IdUniteOrganisationnelle] IS NOT NULL)");
        });
    }
}
