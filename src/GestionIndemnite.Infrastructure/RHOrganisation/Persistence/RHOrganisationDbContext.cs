using System.Data;
using GestionIndemnite.Application.RHOrganisation.Common;
using GestionIndemnite.Domain.RHOrganisation.Common;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using GestionIndemnite.Domain.RHOrganisation.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GestionIndemnite.Infrastructure.RHOrganisation.Persistence;

public class RHOrganisationDbContext : DbContext, IRHOrganisationDbContext
{
    private readonly ICurrentUser _currentUser;

    public RHOrganisationDbContext(
        DbContextOptions<RHOrganisationDbContext> options,
        ICurrentUser currentUser)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Cadre> Cadres => Set<Cadre>();
    public DbSet<Grade> Grades => Set<Grade>();
    public DbSet<Responsabilite> Responsabilites => Set<Responsabilite>();
    public DbSet<StatutAdministratif> StatutAdministratifs => Set<StatutAdministratif>();
    public DbSet<Pays> PaysSet => Set<Pays>();
    public DbSet<Ville> Villes => Set<Ville>();
    public DbSet<Fonctionnaire> Fonctionnaires => Set<Fonctionnaire>();
    public DbSet<Rib> Ribs => Set<Rib>();
    public DbSet<PieceJointe> PieceJointes => Set<PieceJointe>();
    public DbSet<Tribunal> Tribunaux => Set<Tribunal>();
    public DbSet<UniteOrganisationnelle> UnitesOrganisationnelles => Set<UniteOrganisationnelle>();
    public DbSet<HistoriqueOrganigrammeTribunal> HistoriqueOrganigrammeTribunalSet => Set<HistoriqueOrganigrammeTribunal>();
    public DbSet<HistoriqueOrganigrammeUnite> HistoriqueOrganigrammeUniteSet => Set<HistoriqueOrganigrammeUnite>();
    public DbSet<HistoriqueCadreGrade> HistoriqueCadreGrades => Set<HistoriqueCadreGrade>();
    public DbSet<SituationAdministrative> SituationAdministratives => Set<SituationAdministrative>();
    public DbSet<HistoriqueAffectation> HistoriqueAffectations => Set<HistoriqueAffectation>();
    public DbSet<HistoriqueResponsabilite> HistoriqueResponsabilites => Set<HistoriqueResponsabilite>();
    public DbSet<RapportImport> RapportImports => Set<RapportImport>();

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var userName = _currentUser.UserName;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = userName;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userName;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userName;
                    break;
            }
        }

        return await base.SaveChangesAsync(ct);
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel level,
        CancellationToken ct = default)
    {
        return Database.BeginTransactionAsync(level, ct);
    }

    public bool HasActiveTransaction => Database.CurrentTransaction is not null;

    public void ClearTrackedChanges()
    {
        ChangeTracker.Clear();
    }

    public async Task AcquireKeyLockAsync(string resource, CancellationToken ct = default)
    {
        if (Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(
                "AcquireKeyLockAsync doit être appelé dans une transaction explicite.");
        }

        var connection = Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(ct);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = "sp_getapplock";
        command.CommandType = CommandType.StoredProcedure;
        command.Transaction = Database.CurrentTransaction.GetDbTransaction();

        var pResource = command.CreateParameter();
        pResource.ParameterName = "@Resource";
        pResource.Value = resource;
        command.Parameters.Add(pResource);

        var pMode = command.CreateParameter();
        pMode.ParameterName = "@LockMode";
        pMode.Value = "Exclusive";
        command.Parameters.Add(pMode);

        var pOwner = command.CreateParameter();
        pOwner.ParameterName = "@LockOwner";
        pOwner.Value = "Transaction";
        command.Parameters.Add(pOwner);

        var pTimeout = command.CreateParameter();
        pTimeout.ParameterName = "@LockTimeout";
        pTimeout.Value = 5000;
        command.Parameters.Add(pTimeout);

        var resultParam = command.CreateParameter();
        resultParam.ParameterName = "@Result";
        resultParam.DbType = DbType.Int32;
        resultParam.Direction = ParameterDirection.ReturnValue;
        command.Parameters.Add(resultParam);

        await command.ExecuteNonQueryAsync(ct);

        var result = (int)resultParam.Value!;

        switch (result)
        {
            case 0:
            case 1:
                return;
            case -1:
                throw new RegleMetierException(
                    "السجل قيد التعديل من مستخدم آخر، أعد المحاولة بعد لحظات");
            case -2:
            case -3:
                throw new RegleMetierException(
                    "تعارض في المعالجة، أعد المحاولة");
            default:
                throw new RegleMetierException(
                    "تعذّر الاتصال بقاعدة البيانات. تحقق من القائمة قبل إعادة المحاولة");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("rh");

        modelBuilder.Entity<Cadre>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Grade>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Responsabilite>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<StatutAdministratif>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Pays>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Ville>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Fonctionnaire>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Rib>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PieceJointe>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Tribunal>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UniteOrganisationnelle>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<HistoriqueOrganigrammeTribunal>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<HistoriqueOrganigrammeUnite>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<HistoriqueCadreGrade>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<SituationAdministrative>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<HistoriqueAffectation>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<HistoriqueResponsabilite>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<RapportImport>().HasQueryFilter(e => !e.IsDeleted);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RHOrganisationDbContext).Assembly);
    }
}
