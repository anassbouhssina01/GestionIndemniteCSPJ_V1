using GestionIndemnite.Domain.RHOrganisation.Entities;
using GestionIndemnite.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace GestionIndemnite.Tests;

[Collection("SqlServer")]
public class AuditTests
{
    private readonly TestDatabaseFixture _fixture;

    public AuditTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Create_remplit_CreatedAt_et_CreatedBy()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var cadre = NewCadre("CAD-A1");

        context.Cadres.Add(cadre);
        await context.SaveChangesAsync();

        Assert.NotEqual(default, cadre.CreatedAt);
        Assert.Equal("test-user", cadre.CreatedBy);
    }

    [Fact]
    public async Task Update_remplit_UpdatedAt_et_UpdatedBy()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var cadre = NewCadre("CAD-A2");
        context.Cadres.Add(cadre);
        await context.SaveChangesAsync();

        cadre.LibelleCadreFr = "Modifié";
        await context.SaveChangesAsync();

        Assert.NotNull(cadre.UpdatedAt);
        Assert.Equal("test-user", cadre.UpdatedBy);
    }

    [Fact]
    public async Task Remove_positionne_IsDeleted_sans_effacer_la_ligne()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var cadre = NewCadre("CAD-A3");
        context.Cadres.Add(cadre);
        await context.SaveChangesAsync();

        context.Cadres.Remove(cadre);
        await context.SaveChangesAsync();

        using var verify = TestDatabaseFixture.CreateContext();
        var fromDb = await verify.Cadres
            .IgnoreQueryFilters()
            .SingleAsync(c => c.Id == cadre.Id);

        Assert.True(fromDb.IsDeleted);
    }

    [Fact]
    public async Task Requete_ordinaire_ne_retourne_pas_les_supprimes()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var cadre = NewCadre("CAD-A4");
        context.Cadres.Add(cadre);
        await context.SaveChangesAsync();

        context.Cadres.Remove(cadre);
        await context.SaveChangesAsync();

        using var verify = TestDatabaseFixture.CreateContext();
        var found = await verify.Cadres.FirstOrDefaultAsync(c => c.Id == cadre.Id);

        Assert.Null(found);
    }

    [Fact]
    public async Task IgnoreQueryFilters_retourne_les_supprimes()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var cadre = NewCadre("CAD-A5");
        context.Cadres.Add(cadre);
        await context.SaveChangesAsync();

        context.Cadres.Remove(cadre);
        await context.SaveChangesAsync();

        using var verify = TestDatabaseFixture.CreateContext();
        var found = await verify.Cadres
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == cadre.Id);

        Assert.NotNull(found);
        Assert.True(found.IsDeleted);
    }

    [Fact]
    public async Task CodeCadre_unique_viole_depuis_la_base()
    {
        using var context = TestDatabaseFixture.CreateContext();
        context.Cadres.Add(NewCadre("CAD-DUP"));
        await context.SaveChangesAsync();

        using var second = TestDatabaseFixture.CreateContext();
        second.Cadres.Add(NewCadre("CAD-DUP"));

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => second.SaveChangesAsync());
    }

    [Fact]
    public async Task CodeCadre_reutilise_apres_suppression_logique()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var first = NewCadre("CAD-REUSE");
        context.Cadres.Add(first);
        await context.SaveChangesAsync();

        context.Cadres.Remove(first);
        await context.SaveChangesAsync();

        using var second = TestDatabaseFixture.CreateContext();
        second.Cadres.Add(NewCadre("CAD-REUSE"));

        await second.SaveChangesAsync();
    }

    private static Cadre NewCadre(string code) => new()
    {
        Id = Guid.NewGuid(),
        CodeCadre = code,
        LibelleCadreAr = "إطار",
        LibelleCadreFr = "Cadre"
    };
}
