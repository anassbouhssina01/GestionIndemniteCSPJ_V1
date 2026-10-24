using System.Data;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using GestionIndemnite.Tests.Infrastructure;

namespace GestionIndemnite.Tests;

[Collection("SqlServer")]
public class AcquireKeyLockTests
{
    private readonly TestDatabaseFixture _fixture;

    public AcquireKeyLockTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Verrou_acquis_dans_une_transaction()
    {
        using var context = TestDatabaseFixture.CreateContext();
        await using var transaction = await context.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        await context.AcquireKeyLockAsync("rh:test:lock1");

        await transaction.RollbackAsync();
    }

    [Fact]
    public async Task Verrou_libere_apres_commit_et_reacquissable()
    {
        using (var first = TestDatabaseFixture.CreateContext())
        await using (var tx = await first.BeginTransactionAsync(IsolationLevel.ReadCommitted))
        {
            await first.AcquireKeyLockAsync("rh:test:lock2");
            await tx.CommitAsync();
        }

        using var second = TestDatabaseFixture.CreateContext();
        await using var tx2 = await second.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        await second.AcquireKeyLockAsync("rh:test:lock2");
        await tx2.RollbackAsync();
    }

    [Fact]
    public async Task Verrou_libere_apres_rollback()
    {
        using (var first = TestDatabaseFixture.CreateContext())
        await using (var tx = await first.BeginTransactionAsync(IsolationLevel.ReadCommitted))
        {
            await first.AcquireKeyLockAsync("rh:test:lock3");
            await tx.RollbackAsync();
        }

        using var second = TestDatabaseFixture.CreateContext();
        await using var tx2 = await second.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        await second.AcquireKeyLockAsync("rh:test:lock3");
        await tx2.RollbackAsync();
    }

    [Fact]
    public async Task Appel_hors_transaction_leve_InvalidOperationException()
    {
        using var context = TestDatabaseFixture.CreateContext();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => context.AcquireKeyLockAsync("rh:test:lock4"));
    }

    [Fact]
    public async Task Deux_verrous_exclusifs_meme_cle_le_second_attend()
    {
        using var contextA = TestDatabaseFixture.CreateContext();
        await using var txA = await contextA.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        await contextA.AcquireKeyLockAsync("rh:test:lock5");

        var acquired = false;
        using var contextB = TestDatabaseFixture.CreateContext();
        await using var txB = await contextB.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        var taskB = Task.Run(async () =>
        {
            await contextB.AcquireKeyLockAsync("rh:test:lock5");
            acquired = true;
            await txB.RollbackAsync();
        });

        await Task.Delay(700);
        Assert.False(acquired);

        await txA.RollbackAsync();
        await taskB;

        Assert.True(acquired);
    }

    [Fact]
    public async Task Timeout_verrou_transforme_en_RegleMetierException()
    {
        using var contextA = TestDatabaseFixture.CreateContext();
        await using var txA = await contextA.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        await contextA.AcquireKeyLockAsync("rh:test:lock6");

        using var contextB = TestDatabaseFixture.CreateContext();
        await using var txB = await contextB.BeginTransactionAsync(IsolationLevel.ReadCommitted);

        var taskB = Task.Run(async () =>
        {
            try
            {
                await contextB.AcquireKeyLockAsync("rh:test:lock6");
            }
            finally
            {
                await txB.RollbackAsync();
            }
        });

        var ex = await Assert.ThrowsAsync<RegleMetierException>(() => taskB);
        Assert.True(ex.Message.Contains("مستخدم آخر") || ex.Message.Contains("autre utilisateur") || ex.Message.Contains("modification"));

        await txA.RollbackAsync();
    }
}
