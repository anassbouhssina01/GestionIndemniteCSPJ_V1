using GestionIndemnite.Domain.RHOrganisation.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionIndemnite.Infrastructure.RHOrganisation.Persistence.Configurations;

public class TribunalConfiguration : IEntityTypeConfiguration<Tribunal>
{
    public void Configure(EntityTypeBuilder<Tribunal> builder)
    {
        builder.ToTable("Tribunal");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.NomTribunal)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.NomTribunalAr)
            .HasMaxLength(200)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.Property(e => e.NumeroTribunal)
            .HasMaxLength(50)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.HasIndex(e => e.NumeroTribunal)
            .IsUnique()
            .HasDatabaseName("UX_Tribunal_Numero")
            .HasFilter("[NumeroTribunal] <> '' AND [IsDeleted] = 0");

        builder.HasOne<Ville>()
            .WithMany()
            .HasForeignKey(e => e.IdVille)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Tribunal_Ville");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_Tribunal_CassationGenerale",
                "[NiveauJuridiction] <> 3 OR [TypeTribunal] = 0");
        });
    }
}

public class UniteOrganisationnelleConfiguration : IEntityTypeConfiguration<UniteOrganisationnelle>
{
    public void Configure(EntityTypeBuilder<UniteOrganisationnelle> builder)
    {
        builder.ToTable("UniteOrganisationnelle");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.NomUnite)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.NomUniteAr)
            .HasMaxLength(200)
            .IsRequired()
            .HasDefaultValue(string.Empty);

        builder.HasOne<Ville>()
            .WithMany()
            .HasForeignKey(e => e.IdVille)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_UniteOrganisationnelle_Ville");
    }
}

public class HistoriqueOrganigrammeTribunalConfiguration
    : IEntityTypeConfiguration<HistoriqueOrganigrammeTribunal>
{
    public void Configure(EntityTypeBuilder<HistoriqueOrganigrammeTribunal> builder)
    {
        builder.ToTable("HistoriqueOrganigrammeTribunal");

        builder.HasKey(e => e.Id);

        builder.HasOne<Tribunal>()
            .WithMany()
            .HasForeignKey(e => e.IdTribunal)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_HistOrgTribunal_Tribunal");

        builder.HasOne<Tribunal>()
            .WithMany()
            .HasForeignKey(e => e.IdTribunalParent)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_HistOrgTribunal_Parent");

        builder.HasOne<PieceJointe>()
            .WithMany()
            .HasForeignKey(e => e.IdDocument)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_HistOrgTribunal_Document");

        builder.HasIndex(e => new { e.IdTribunal, e.DateFin })
            .IsUnique()
            .HasDatabaseName("UX_HistOrgTribunal_Ouvert")
            .HasFilter("[DateFin] IS NULL AND [IsDeleted] = 0");

        builder.HasIndex(e => new { e.IdTribunal, e.DateDebut, e.DateFin })
            .HasDatabaseName("IX_HistOrgTribunal_Cle_Periode");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_HistOrgTribunal_Periode",
                "[DateFin] IS NULL OR [DateDebut] <= [DateFin]");
        });
    }
}

public class HistoriqueOrganigrammeUniteConfiguration
    : IEntityTypeConfiguration<HistoriqueOrganigrammeUnite>
{
    public void Configure(EntityTypeBuilder<HistoriqueOrganigrammeUnite> builder)
    {
        builder.ToTable("HistoriqueOrganigrammeUnite");

        builder.HasKey(e => e.Id);

        builder.HasOne<UniteOrganisationnelle>()
            .WithMany()
            .HasForeignKey(e => e.IdUnite)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_HistOrgUnite_Unite");

        builder.HasOne<UniteOrganisationnelle>()
            .WithMany()
            .HasForeignKey(e => e.IdUniteParent)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_HistOrgUnite_Parent");

        builder.HasOne<PieceJointe>()
            .WithMany()
            .HasForeignKey(e => e.IdDocument)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_HistOrgUnite_Document");

        builder.HasIndex(e => new { e.IdUnite, e.DateFin })
            .IsUnique()
            .HasDatabaseName("UX_HistOrgUnite_Ouvert")
            .HasFilter("[DateFin] IS NULL AND [IsDeleted] = 0");

        builder.HasIndex(e => new { e.IdUnite, e.DateDebut, e.DateFin })
            .HasDatabaseName("IX_HistOrgUnite_Cle_Periode");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "CK_HistOrgUnite_Periode",
                "[DateFin] IS NULL OR [DateDebut] <= [DateFin]");
        });
    }
}
