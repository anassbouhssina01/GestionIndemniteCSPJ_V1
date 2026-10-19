using System.Data;
using GestionIndemnite.Domain.RHOrganisation.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GestionIndemnite.Application.RHOrganisation.Common;

public interface IRHOrganisationDbContext
{
    DbSet<Cadre> Cadres { get; }
    DbSet<Grade> Grades { get; }
    DbSet<Responsabilite> Responsabilites { get; }
    DbSet<StatutAdministratif> StatutAdministratifs { get; }
    DbSet<Pays> PaysSet { get; }
    DbSet<Ville> Villes { get; }
    DbSet<Fonctionnaire> Fonctionnaires { get; }
    DbSet<Rib> Ribs { get; }
    DbSet<PieceJointe> PieceJointes { get; }
    DbSet<Tribunal> Tribunaux { get; }
    DbSet<UniteOrganisationnelle> UnitesOrganisationnelles { get; }
    DbSet<HistoriqueOrganigrammeTribunal> HistoriqueOrganigrammeTribunalSet { get; }
    DbSet<HistoriqueOrganigrammeUnite> HistoriqueOrganigrammeUniteSet { get; }
    DbSet<HistoriqueCadreGrade> HistoriqueCadreGrades { get; }
    DbSet<SituationAdministrative> SituationAdministratives { get; }
    DbSet<HistoriqueAffectation> HistoriqueAffectations { get; }
    DbSet<HistoriqueResponsabilite> HistoriqueResponsabilites { get; }
    DbSet<RapportImport> RapportImports { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel level,
        CancellationToken ct = default);

    Task AcquireKeyLockAsync(string resource, CancellationToken ct = default);

    bool HasActiveTransaction { get; }

    void ClearTrackedChanges();
}
