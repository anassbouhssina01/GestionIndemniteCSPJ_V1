using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Application.RHOrganisation.Services.Referentiels;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using GestionIndemnite.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace GestionIndemnite.Tests;

[Collection("SqlServer")]
public class ReferentielsTests
{
    private readonly TestDatabaseFixture _fixture;

    public ReferentielsTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Cadre_CodeCadre_unique_depuis_la_base()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new CadreService(context);

        await svc.CreateAsync(new CreateCadreCommand("C-UNIQ", "إطار", "Cadre", null));

        await Assert.ThrowsAsync<RegleMetierException>(
            () => svc.CreateAsync(new CreateCadreCommand("C-UNIQ", "إطار2", "Cadre2", null)));
    }

    [Fact]
    public async Task Cadre_reutilisation_code_apres_suppression_logique()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new CadreService(context);

        var created = await svc.CreateAsync(new CreateCadreCommand("C-REUSE", "إطار", "Cadre", null));
        await svc.DisableAsync(created.Id);

        using var verify = TestDatabaseFixture.CreateContext();
        using var raw = verify;
        var entity = await raw.Cadres.IgnoreQueryFilters().FirstAsync(c => c.Id == created.Id);
        entity.IsDeleted = true;
        await raw.SaveChangesAsync();

        using var second = TestDatabaseFixture.CreateContext();
        var svc2 = new CadreService(second);
        var recreated = await svc2.CreateAsync(new CreateCadreCommand("C-REUSE", "إطار", "Cadre", null));
        Assert.NotEqual(default, recreated.Id);
    }

    [Fact]
    public async Task Cadre_desactivation_ne_supprime_pas()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new CadreService(context);

        var created = await svc.CreateAsync(new CreateCadreCommand("C-DISA", "إطار", "Cadre", null));
        await svc.DisableAsync(created.Id);

        using var verify = TestDatabaseFixture.CreateContext();
        var stillExists = await verify.Cadres.IgnoreQueryFilters().AnyAsync(c => c.Id == created.Id);
        Assert.True(stillExists);
    }

    [Fact]
    public async Task Grade_meme_numero_dans_deux_cadres_distincts_accepte()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var cadreSvc = new CadreService(context);
        var gradeSvc = new GradeService(context);

        var c1 = await cadreSvc.CreateAsync(new CreateCadreCommand("GC-1", "إطار1", "Cadre1", null));
        var c2 = await cadreSvc.CreateAsync(new CreateCadreCommand("GC-2", "إطار2", "Cadre2", null));

        var g1 = await gradeSvc.CreateAsync(
            new CreateGradeCommand(c1.Id, 1, "رتبة1", "Grade1", null));
        var g2 = await gradeSvc.CreateAsync(
            new CreateGradeCommand(c2.Id, 1, "رتبة2", "Grade2", null));

        Assert.NotEqual(g1.Id, g2.Id);
    }

    [Fact]
    public async Task Grade_meme_numero_dans_un_meme_cadre_rejete()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var cadreSvc = new CadreService(context);
        var gradeSvc = new GradeService(context);

        var c = await cadreSvc.CreateAsync(new CreateCadreCommand("GC-DUP", "إطار", "Cadre", null));
        await gradeSvc.CreateAsync(new CreateGradeCommand(c.Id, 5, "رتبة", "Grade", null));

        await Assert.ThrowsAsync<RegleMetierException>(
            () => gradeSvc.CreateAsync(new CreateGradeCommand(c.Id, 5, "رتبة2", "Grade2", null)));
    }

    [Fact]
    public async Task Grade_ux_composite_viole_depuis_la_base()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var cadreSvc = new CadreService(context);
        var gradeSvc = new GradeService(context);

        var c = await cadreSvc.CreateAsync(new CreateCadreCommand("GC-BASE", "إطار", "Cadre", null));
        await gradeSvc.CreateAsync(new CreateGradeCommand(c.Id, 10, "رتبة", "Grade", null));

        using var second = TestDatabaseFixture.CreateContext();
        second.Grades.Add(new Domain.RHOrganisation.Entities.Grade
        {
            Id = Guid.NewGuid(),
            IdCadre = c.Id,
            NumeroReferencie = 10,
            LibelleGradeAr = "x",
            LibelleGradeFr = "x"
        });

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => second.SaveChangesAsync());
    }

    [Fact]
    public async Task Pays_second_local_rejete()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new PaysService(context);
        var tag = Guid.NewGuid().ToString("N")[..8];

        try
        {
            await svc.CreateAsync(new CreatePaysCommand($"L{tag}", "المغرب", "Maroc", true));
        }
        catch (RegleMetierException)
        {
        }

        using var second = TestDatabaseFixture.CreateContext();
        var svc2 = new PaysService(second);
        await Assert.ThrowsAsync<RegleMetierException>(
            () => svc2.CreateAsync(new CreatePaysCommand($"F{tag}", "فرنسا", "France", true)));
    }

    [Fact]
    public async Task Pays_non_local_deuxieme_accepte()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new PaysService(context);
        var tag = Guid.NewGuid().ToString("N")[..8];

        try
        {
            await svc.CreateAsync(new CreatePaysCommand($"M{tag}", "المغرب", "Maroc", true));
        }
        catch (RegleMetierException)
        {
        }

        var fr = await svc.CreateAsync(new CreatePaysCommand($"N{tag}", "فرنسا", "France", false));

        Assert.False(fr.EstLocal);
    }

    [Fact]
    public async Task Ville_latitude_hors_plage_rejetee_par_la_base()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var paysSvc = new PaysService(context);
        var villeSvc = new VilleService(context);

        var pays = await paysSvc.CreateAsync(new CreatePaysCommand("TST", "بلد", "Pays", false));

        using var second = TestDatabaseFixture.CreateContext();
        second.Villes.Add(new Domain.RHOrganisation.Entities.Ville
        {
            Id = Guid.NewGuid(),
            NomVille = "VilleBad",
            IdPays = pays.Id,
            Latitude = 95m,
            Longitude = 0m
        });

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => second.SaveChangesAsync());
    }

    [Fact]
    public async Task Ville_longitude_hors_plage_rejetee_par_la_base()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var paysSvc = new PaysService(context);

        var pays = await paysSvc.CreateAsync(new CreatePaysCommand("TST2", "بلد2", "Pays2", false));

        using var second = TestDatabaseFixture.CreateContext();
        second.Villes.Add(new Domain.RHOrganisation.Entities.Ville
        {
            Id = Guid.NewGuid(),
            NomVille = "VilleBad2",
            IdPays = pays.Id,
            Latitude = 0m,
            Longitude = 200m
        });

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => second.SaveChangesAsync());
    }

    [Fact]
    public async Task Ville_coordonnees_partielles_rejetees()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var paysSvc = new PaysService(context);
        var villeSvc = new VilleService(context);

        var pays = await paysSvc.CreateAsync(new CreatePaysCommand("TST3", "بلد3", "Pays3", false));

        await Assert.ThrowsAsync<RegleMetierException>(
            () => villeSvc.CreateAsync(new CreateVilleCommand("VillePartial", pays.Id, 33.5m, null)));
    }

    [Fact]
    public async Task Ville_noms_francais_et_arabe_enregistres()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var paysSvc = new PaysService(context);
        var villeSvc = new VilleService(context);

        var tag = Guid.NewGuid().ToString("N")[..6];
        var pays = await paysSvc.CreateAsync(new CreatePaysCommand($"PA{tag}", "بلد", "Pays", false));

        var created = await villeSvc.CreateAsync(
            new CreateVilleCommand($"Ville{tag}", pays.Id, null, null)
            {
                NomVilleAr = "مدينة الاختبار"
            });

        Assert.Equal("مدينة الاختبار", created.NomVilleAr);

        var updated = await villeSvc.UpdateAsync(created.Id,
            new UpdateVilleCommand($"Ville{tag}Bis", null, null, true)
            {
                NomVilleAr = "مدينة محدثة"
            });

        Assert.Equal("مدينة محدثة", updated.NomVilleAr);
        Assert.Equal($"Ville{tag}Bis", updated.NomVille);
    }

    [Fact]
    public async Task Statut_desactivation_puis_activation()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new StatutAdministratifService(context);

        var created = await svc.CreateAsync(
            new CreateStatutAdministratifCommand("ACTIF", "وضعية", "Statut", null));

        await svc.DisableAsync(created.Id);
        var list1 = await svc.ListAsync(includeInactive: false);
        Assert.DoesNotContain(list1, s => s.Id == created.Id);

        await svc.EnableAsync(created.Id);
        var list2 = await svc.ListAsync(includeInactive: false);
        Assert.Contains(list2, s => s.Id == created.Id);
    }

    [Fact]
    public async Task Responsabilite_Code_unique_depuis_la_base()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new ResponsabiliteService(context);

        await svc.CreateAsync(new CreateResponsabiliteCommand("RESP-1", "مسؤولية", "Resp", null));

        await Assert.ThrowsAsync<RegleMetierException>(
            () => svc.CreateAsync(new CreateResponsabiliteCommand("RESP-1", "مسؤولية2", "Resp2", null)));
    }
}
