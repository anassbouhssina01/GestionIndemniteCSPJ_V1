using System.Diagnostics;
using GestionIndemnite.Application.RHOrganisation.Services;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using GestionIndemnite.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace GestionIndemnite.Tests;

[Collection("SqlServer")]
public class PeriodeServiceTests
{
    private readonly TestDatabaseFixture _fixture;
    private readonly ITestOutputHelper _output;

    public PeriodeServiceTests(TestDatabaseFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    private static PeriodeTest NewPeriode(Guid cle, DateOnly debut, DateOnly? fin) =>
        new() { Id = Guid.NewGuid(), IdCle = cle, DateDebut = debut, DateFin = fin };

    [Fact]
    public async Task Premier_enregistrement_reussi()
    {
        using var context = TestDatabaseFixture.CreateTestContext();
        var svc = new PeriodeService(context);
        var cle = Guid.NewGuid();

        var id = await svc.AddAsync(
            context.PeriodesTest,
            NewPeriode(cle, new DateOnly(2020, 1, 1), null),
            e => e.IdCle == cle);

        Assert.NotEqual(default, id);
    }

    [Fact]
    public async Task Chevauchement_partiel_rejete()
    {
        using var context = TestDatabaseFixture.CreateTestContext();
        var svc = new PeriodeService(context);
        var cle = Guid.NewGuid();

        await svc.AddAsync(context.PeriodesTest,
            NewPeriode(cle, new DateOnly(2020, 1, 1), new DateOnly(2020, 12, 31)),
            e => e.IdCle == cle);

        await Assert.ThrowsAsync<RegleMetierException>(() => svc.AddAsync(
            context.PeriodesTest,
            NewPeriode(cle, new DateOnly(2020, 6, 1), new DateOnly(2021, 6, 30)),
            e => e.IdCle == cle));
    }

    [Fact]
    public async Task Chevauchement_total_rejete()
    {
        using var context = TestDatabaseFixture.CreateTestContext();
        var svc = new PeriodeService(context);
        var cle = Guid.NewGuid();

        await svc.AddAsync(context.PeriodesTest,
            NewPeriode(cle, new DateOnly(2020, 1, 1), new DateOnly(2020, 12, 31)),
            e => e.IdCle == cle);

        await Assert.ThrowsAsync<RegleMetierException>(() => svc.AddAsync(
            context.PeriodesTest,
            NewPeriode(cle, new DateOnly(2020, 1, 1), new DateOnly(2020, 12, 31)),
            e => e.IdCle == cle));
    }

    [Fact]
    public async Task Chevauchement_un_jour_commum_rejete()
    {
        using var context = TestDatabaseFixture.CreateTestContext();
        var svc = new PeriodeService(context);
        var cle = Guid.NewGuid();

        await svc.AddAsync(context.PeriodesTest,
            NewPeriode(cle, new DateOnly(2020, 1, 1), new DateOnly(2020, 6, 30)),
            e => e.IdCle == cle);

        await Assert.ThrowsAsync<RegleMetierException>(() => svc.AddAsync(
            context.PeriodesTest,
            NewPeriode(cle, new DateOnly(2020, 6, 30), new DateOnly(2020, 12, 31)),
            e => e.IdCle == cle));
    }

    [Fact]
    public async Task Date_identique_rejetee()
    {
        using var context = TestDatabaseFixture.CreateTestContext();
        var svc = new PeriodeService(context);
        var cle = Guid.NewGuid();

        await svc.AddAsync(context.PeriodesTest,
            NewPeriode(cle, new DateOnly(2020, 5, 1), new DateOnly(2020, 5, 1)),
            e => e.IdCle == cle);

        await Assert.ThrowsAsync<RegleMetierException>(() => svc.AddAsync(
            context.PeriodesTest,
            NewPeriode(cle, new DateOnly(2020, 5, 1), new DateOnly(2020, 5, 1)),
            e => e.IdCle == cle));
    }

    [Fact]
    public async Task DateDebut_apres_DateFin_rejete()
    {
        using var context = TestDatabaseFixture.CreateTestContext();
        var svc = new PeriodeService(context);
        var cle = Guid.NewGuid();

        await Assert.ThrowsAsync<RegleMetierException>(() => svc.AddAsync(
            context.PeriodesTest,
            NewPeriode(cle, new DateOnly(2021, 1, 1), new DateOnly(2020, 1, 1)),
            e => e.IdCle == cle));
    }

    [Fact]
    public async Task Deux_enregistrements_ouverts_rejetes()
    {
        using var context = TestDatabaseFixture.CreateTestContext();
        var svc = new PeriodeService(context);
        var cle = Guid.NewGuid();

        await svc.AddAsync(context.PeriodesTest,
            NewPeriode(cle, new DateOnly(2020, 1, 1), null),
            e => e.IdCle == cle);

        await Assert.ThrowsAsync<RegleMetierException>(() => svc.AddAsync(
            context.PeriodesTest,
            NewPeriode(cle, new DateOnly(2021, 1, 1), null),
            e => e.IdCle == cle));
    }

    [Fact]
    public async Task Trou_accepte_et_visible_dans_GetGaps()
    {
        using var context = TestDatabaseFixture.CreateTestContext();
        var svc = new PeriodeService(context);
        var cle = Guid.NewGuid();

        await svc.AddAsync(context.PeriodesTest,
            NewPeriode(cle, new DateOnly(2020, 1, 1), new DateOnly(2020, 1, 31)),
            e => e.IdCle == cle);
        await svc.AddAsync(context.PeriodesTest,
            NewPeriode(cle, new DateOnly(2020, 3, 1), new DateOnly(2020, 3, 31)),
            e => e.IdCle == cle);

        var gaps = await svc.GetGapsAsync(context.PeriodesTest, e => e.IdCle == cle);

        Assert.Single(gaps);
        Assert.Equal(new DateOnly(2020, 2, 1), gaps[0].DateDebut);
        Assert.Equal(new DateOnly(2020, 2, 29), gaps[0].DateFin);
    }

    [Fact]
    public async Task Insertion_au_milieu_acceptee()
    {
        using var context = TestDatabaseFixture.CreateTestContext();
        var svc = new PeriodeService(context);
        var cle = Guid.NewGuid();

        await svc.AddAsync(context.PeriodesTest,
            NewPeriode(cle, new DateOnly(2020, 1, 1), new DateOnly(2020, 4, 30)),
            e => e.IdCle == cle);
        await svc.AddAsync(context.PeriodesTest,
            NewPeriode(cle, new DateOnly(2020, 8, 1), new DateOnly(2020, 12, 31)),
            e => e.IdCle == cle);

        var mid = await svc.AddAsync(context.PeriodesTest,
            NewPeriode(cle, new DateOnly(2020, 5, 1), new DateOnly(2020, 7, 31)),
            e => e.IdCle == cle);

        Assert.NotEqual(default, mid);
    }

    [Fact]
    public async Task Modification_ne_chevauche_pas_elle_mememe()
    {
        using var context = TestDatabaseFixture.CreateTestContext();
        var svc = new PeriodeService(context);
        var cle = Guid.NewGuid();

        var id = await svc.AddAsync(context.PeriodesTest,
            NewPeriode(cle, new DateOnly(2020, 1, 1), new DateOnly(2020, 12, 31)),
            e => e.IdCle == cle);

        var tracked = await context.PeriodesTest.FirstAsync(e => e.Id == id);
        tracked.DateFin = new DateOnly(2021, 6, 30);

        await svc.UpdateAsync(context.PeriodesTest, tracked, e => e.IdCle == cle);

        var reloaded = await context.PeriodesTest.AsNoTracking().FirstAsync(e => e.Id == id);
        Assert.Equal(new DateOnly(2021, 6, 30), reloaded.DateFin);
    }

    [Fact]
    public async Task Meme_periode_deux_cles_differentes_acceptee()
    {
        using var context = TestDatabaseFixture.CreateTestContext();
        var svc = new PeriodeService(context);
        var cleA = Guid.NewGuid();
        var cleB = Guid.NewGuid();

        await svc.AddAsync(context.PeriodesTest,
            NewPeriode(cleA, new DateOnly(2020, 1, 1), new DateOnly(2020, 12, 31)),
            e => e.IdCle == cleA);
        await svc.AddAsync(context.PeriodesTest,
            NewPeriode(cleB, new DateOnly(2020, 1, 1), new DateOnly(2020, 12, 31)),
            e => e.IdCle == cleB);
    }

    [Fact]
    public async Task Test_temoin_20_paralleles_sans_verrou()
    {
        var cle = Guid.NewGuid();
        var successes = 0;
        var failures = 0;

        var tasks = Enumerable.Range(0, 20).Select(_ => Task.Run(async () =>
        {
            using var context = TestDatabaseFixture.CreateTestContext();
            var svc = new PeriodeService(context);

            try
            {
                await svc.AddAsync(context.PeriodesTest,
                    NewPeriode(cle, new DateOnly(2020, 1, 1), new DateOnly(2020, 12, 31)),
                    e => e.IdCle == cle);
                Interlocked.Increment(ref successes);
            }
            catch (RegleMetierException)
            {
                Interlocked.Increment(ref failures);
            }
        }));

        await Task.WhenAll(tasks);

        _output.WriteLine($"Témoin sans verrou : {successes} succès, {failures} rejets sur 20");
        Assert.Equal(20, successes + failures);
    }

    [Fact]
    public async Task Test_charge_20_paralleles_avec_verrou_exactement_un_succes()
    {
        var cle = Guid.NewGuid();
        var lockKey = $"rh:periode:PeriodeTest:{cle}";
        var successes = 0;
        var rejets = 0;
        var autres = 0;

        var tasks = Enumerable.Range(0, 20).Select(_ => Task.Run(async () =>
        {
            using var context = TestDatabaseFixture.CreateTestContext();
            var svc = new PeriodeService(context);

            try
            {
                await svc.AddAsync(context.PeriodesTest,
                    NewPeriode(cle, new DateOnly(2020, 1, 1), new DateOnly(2020, 12, 31)),
                    e => e.IdCle == cle,
                    lockKey);
                Interlocked.Increment(ref successes);
            }
            catch (RegleMetierException)
            {
                Interlocked.Increment(ref rejets);
            }
            catch
            {
                Interlocked.Increment(ref autres);
            }
        }));

        await Task.WhenAll(tasks);

        _output.WriteLine($"Charge avec verrou : {successes} succès, {rejets} rejets métier, {autres} autres sur 20");
        Assert.Equal(1, successes);
        Assert.Equal(20, successes + rejets + autres);
    }

    [Fact]
    public async Task Verrou_bloque_le_second_acces()
    {
        var cle = Guid.NewGuid();
        var lockKey = $"rh:periode:PeriodeTest:block:{cle}";
        var signal = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        using var contextA = TestDatabaseFixture.CreateTestContext();
        await using var txnA = await contextA.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        await contextA.AcquireKeyLockAsync(lockKey);

        var taskB = Task.Run(async () =>
        {
            using var contextB = TestDatabaseFixture.CreateTestContext();
            var svc = new PeriodeService(contextB);

            try
            {
                await svc.AddAsync(contextB.PeriodesTest,
                    NewPeriode(cle, new DateOnly(2020, 1, 1), new DateOnly(2020, 12, 31)),
                    e => e.IdCle == cle,
                    lockKey);
                return false;
            }
            catch (RegleMetierException)
            {
                return true;
            }
        });

        var sw = Stopwatch.StartNew();
        await Task.Delay(600);
        Assert.False(taskB.IsCompleted, "B ne doit pas se terminer tant que A détient le verrou");

        contextA.PeriodesTest.Add(NewPeriode(cle, new DateOnly(2020, 1, 1), new DateOnly(2020, 12, 31)));
        await contextA.SaveChangesAsync();
        signal.TrySetResult();
        await txnA.CommitAsync();

        var rejete = await taskB;
        sw.Stop();

        Assert.True(rejete, "B doit détecter le chevauchement après la libération du verrou");
        Assert.True(sw.ElapsedMilliseconds >= 500, "B a dû attendre la libération");
    }

    [Fact]
    public async Task Verrou_delai_depasse_message_clair()
    {
        var lockKey = $"rh:periode:PeriodeTest:timeout:{Guid.NewGuid()}";

        using var contextA = TestDatabaseFixture.CreateTestContext();
        await using var txnA = await contextA.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        await contextA.AcquireKeyLockAsync(lockKey);

        using var contextB = TestDatabaseFixture.CreateTestContext();
        await using var txnB = await contextB.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);

        var ex = await Assert.ThrowsAsync<RegleMetierException>(
            () => contextB.AcquireKeyLockAsync(lockKey));

        Assert.Equal(
            "L'enregistrement est en cours de modification par un autre utilisateur. Veuillez réessayer dans un instant.",
            ex.Message);

        await txnA.CommitAsync();
    }
}
