using GestionIndemnite.Application.RHOrganisation.Common;
using GestionIndemnite.Infrastructure.RHOrganisation.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionIndemnite.Tests.Infrastructure;

public sealed class TestDatabaseFixture : IAsyncLifetime
{
    public const string ConnectionString =
        "Server=(localdb)\\GestionIndemnite;Database=GestionIndemnite_Test;Trusted_Connection=True;TrustServerCertificate=True";

    private static int _initialized;

    public async Task InitializeAsync()
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 1)
        {
            return;
        }

        using var context = CreateContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public static RHOrganisationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RHOrganisationDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new TestDbContext(options, new TestCurrentUser());
    }

    public static TestDbContext CreateTestContext()
    {
        var options = new DbContextOptionsBuilder<RHOrganisationDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new TestDbContext(options, new TestCurrentUser());
    }

    private sealed class TestCurrentUser : ICurrentUser
    {
        public string UserName => "test-user";
    }
}

[CollectionDefinition("SqlServer")]
public class SqlServerCollection : ICollectionFixture<TestDatabaseFixture>
{
}
