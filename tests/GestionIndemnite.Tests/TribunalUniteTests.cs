using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Application.RHOrganisation.Services;
using GestionIndemnite.Application.RHOrganisation.Services.Referentiels;
using GestionIndemnite.Domain.RHOrganisation.Entities;
using GestionIndemnite.Domain.RHOrganisation.Enums;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using GestionIndemnite.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace GestionIndemnite.Tests;

[Collection("SqlServer")]
public class TribunalUniteTests
{
    private readonly TestDatabaseFixture _fixture;

    public TribunalUniteTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task<(Guid PaysId, Guid VilleId)> SeedVilleAsync()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var paysSvc = new PaysService(context);
        var villeSvc = new VilleService(context);

        var tag = Guid.NewGuid().ToString("N")[..6];
        var pays = await paysSvc.CreateAsync(new CreatePaysCommand($"P{tag}", "بلد", "Pays", false));
        var ville = await villeSvc.CreateAsync(new CreateVilleCommand($"V{tag}", pays.Id, null, null));
        return (pays.Id, ville.Id);
    }

    private static CreateTribunalCommand NewTribunal(
        string nom,
        NiveauJuridiction niveau,
        TypeTribunal type,
        Guid villeId,
        Guid? parent) =>
        new(nom, type, niveau, villeId, parent);

    [Fact]
    public async Task Creation_tribunal_et_unite_avec_noms_arabes()
    {
        var (_, villeId) = await SeedVilleAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var tribunalSvc = new TribunalService(context);
        var uniteSvc = new UniteOrganisationnelleService(context);
        var tag = Guid.NewGuid().ToString("N")[..6];

        var tribunal = await tribunalSvc.CreateAsync(
            new CreateTribunalCommand($"TAr{tag}", TypeTribunal.Generale,
                NiveauJuridiction.Cassation, villeId, null)
            {
                NomTribunalAr = "المحكمة الابتدائية"
            });

        var unite = await uniteSvc.CreateAsync(
            new CreateUniteOrganisationnelleCommand($"UAr{tag}", TypeUnite.Unite, villeId, null)
            {
                NomUniteAr = "قسم الموارد البشرية"
            });

        Assert.Equal("المحكمة الابتدائية", tribunal.NomTribunalAr);
        Assert.Equal("قسم الموارد البشرية", unite.NomUniteAr);

        var relu = await tribunalSvc.GetAsync(tribunal.Id);
        Assert.Equal("المحكمة الابتدائية", relu.NomTribunalAr);

        var reluUnite = await uniteSvc.GetAsync(unite.Id);
        Assert.Equal("قسم الموارد البشرية", reluUnite.NomUniteAr);
    }

    [Fact]
    public async Task Creation_cassation_sans_parent_reussie()
    {
        var (_, villeId) = await SeedVilleAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new TribunalService(context);

        var created = await svc.CreateAsync(NewTribunal(
            "Cour de Cassation", NiveauJuridiction.Cassation, TypeTribunal.Generale, villeId, null));

        Assert.NotEqual(default, created.Id);

        var hist = await context.HistoriqueOrganigrammeTribunalSet
            .IgnoreQueryFilters()
            .Where(h => h.IdTribunal == created.Id)
            .ToListAsync();
        Assert.Single(hist);
        Assert.Null(hist[0].IdTribunalParent);
    }

    [Fact]
    public async Task Creation_hierarchie_complete_cjr_vers_cassation()
    {
        var (_, villeId) = await SeedVilleAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new TribunalService(context);

        var cassation = await svc.CreateAsync(NewTribunal(
            "Cassation", NiveauJuridiction.Cassation, TypeTribunal.Generale, villeId, null));
        var appel = await svc.CreateAsync(NewTribunal(
            "Appel", NiveauJuridiction.CA, TypeTribunal.Generale, villeId, cassation.Id));
        var tpi = await svc.CreateAsync(NewTribunal(
            "TPI", NiveauJuridiction.TPI, TypeTribunal.Generale, villeId, appel.Id));
        var cjr = await svc.CreateAsync(NewTribunal(
            "CJR", NiveauJuridiction.CJR, TypeTribunal.Generale, villeId, tpi.Id));

        var parent = await svc.GetParentAtAsync(cjr.Id);
        Assert.NotNull(parent);
        Assert.Equal(tpi.Id, parent.Id);
    }

    [Fact]
    public async Task Creation_cassation_avec_type_autre_que_generale_rejetee_par_la_base()
    {
        var (_, villeId) = await SeedVilleAsync();

        using var context = TestDatabaseFixture.CreateContext();
        context.Tribunaux.Add(new Tribunal
        {
            Id = Guid.NewGuid(),
            NomTribunal = "BadCassation",
            TypeTribunal = TypeTribunal.Administratif,
            NiveauJuridiction = NiveauJuridiction.Cassation,
            IdVille = villeId,
            IsActive = true
        });

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task Parent_mauvais_niveau_rejete()
    {
        var (_, villeId) = await SeedVilleAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new TribunalService(context);

        var cassation = await svc.CreateAsync(NewTribunal(
            "CassX", NiveauJuridiction.Cassation, TypeTribunal.Generale, villeId, null));

        await Assert.ThrowsAsync<RegleMetierException>(
            () => svc.CreateAsync(NewTribunal(
                "BadLevel", NiveauJuridiction.CJR, TypeTribunal.Generale, villeId, cassation.Id)));
    }

    [Fact]
    public async Task Non_cassation_sans_parent_rejetee()
    {
        var (_, villeId) = await SeedVilleAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new TribunalService(context);

        await Assert.ThrowsAsync<RegleMetierException>(
            () => svc.CreateAsync(NewTribunal(
                "NoParent", NiveauJuridiction.TPI, TypeTribunal.Generale, villeId, null)));
    }

    [Fact]
    public async Task Cassation_avec_parent_rejetee_par_le_service()
    {
        var (_, villeId) = await SeedVilleAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new TribunalService(context);

        var autre = await svc.CreateAsync(NewTribunal(
            "Root", NiveauJuridiction.Cassation, TypeTribunal.Generale, villeId, null));

        await Assert.ThrowsAsync<RegleMetierException>(
            () => svc.CreateAsync(NewTribunal(
                "CassWithParent", NiveauJuridiction.Cassation, TypeTribunal.Generale, villeId, autre.Id)));
    }

    [Fact]
    public async Task Creation_tribunal_atomique_un_seul_enregistrement()
    {
        var (_, villeId) = await SeedVilleAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new TribunalService(context);

        var before = await context.Tribunaux.IgnoreQueryFilters().CountAsync();

        var created = await svc.CreateAsync(NewTribunal(
            "Atomic", NiveauJuridiction.Cassation, TypeTribunal.Generale, villeId, null));

        using var second = TestDatabaseFixture.CreateContext();
        var tribunaux = await second.Tribunaux.IgnoreQueryFilters()
            .Where(t => t.Id == created.Id).ToListAsync();
        var hist = await second.HistoriqueOrganigrammeTribunalSet.IgnoreQueryFilters()
            .Where(h => h.IdTribunal == created.Id).ToListAsync();

        Assert.Single(tribunaux);
        Assert.Single(hist);
    }

    [Fact]
    public async Task Parent_a_une_date_anterieure_lu_depuis_historique()
    {
        var (_, villeId) = await SeedVilleAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new TribunalService(context);

        var parentOld = await svc.CreateAsync(NewTribunal(
            "OldParent", NiveauJuridiction.Cassation, TypeTribunal.Generale, villeId, null));
        var parentNew = await svc.CreateAsync(NewTribunal(
            "NewParent", NiveauJuridiction.Cassation, TypeTribunal.Generale, villeId, null));
        var child = await svc.CreateAsync(NewTribunal(
            "Child", NiveauJuridiction.CA, TypeTribunal.Generale, villeId, parentNew.Id));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var oldDate = today.AddDays(-30);

        context.HistoriqueOrganigrammeTribunalSet.Add(new HistoriqueOrganigrammeTribunal
        {
            Id = Guid.NewGuid(),
            IdTribunal = child.Id,
            IdTribunalParent = parentOld.Id,
            DateDebut = oldDate,
            DateFin = today.AddDays(-1)
        });
        await context.SaveChangesAsync();

        using var second = TestDatabaseFixture.CreateContext();
        var svc2 = new TribunalService(second);

        var currentParent = await svc2.GetParentAtAsync(child.Id);
        Assert.NotNull(currentParent);
        Assert.Equal(parentNew.Id, currentParent.Id);

        var pastParent = await svc2.GetParentAtAsync(child.Id, oldDate.AddDays(5));
        Assert.NotNull(pastParent);
        Assert.Equal(parentOld.Id, pastParent.Id);
    }

    [Fact]
    public async Task Unite_sans_parent_reussie()
    {
        var (_, villeId) = await SeedVilleAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new UniteOrganisationnelleService(context);

        var created = await svc.CreateAsync(new CreateUniteOrganisationnelleCommand(
            "Administration", TypeUnite.Administration, villeId, null));

        Assert.NotEqual(default, created.Id);
    }

    [Fact]
    public async Task Unite_avec_parent_reussie()
    {
        var (_, villeId) = await SeedVilleAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new UniteOrganisationnelleService(context);

        var root = await svc.CreateAsync(new CreateUniteOrganisationnelleCommand(
            "SG", TypeUnite.SecretariatGeneral, villeId, null));
        var child = await svc.CreateAsync(new CreateUniteOrganisationnelleCommand(
            "Dept1", TypeUnite.Departement, villeId, root.Id));

        var parent = await svc.GetParentAtAsync(child.Id);
        Assert.NotNull(parent);
        Assert.Equal(root.Id, parent.Id);
    }

    [Fact]
    public async Task Unite_parent_inexistant_rejete()
    {
        var (_, villeId) = await SeedVilleAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new UniteOrganisationnelleService(context);

        await Assert.ThrowsAsync<RegleMetierException>(
            () => svc.CreateAsync(new CreateUniteOrganisationnelleCommand(
                "Orphan", TypeUnite.Unite, villeId, Guid.NewGuid())));
    }

    [Fact]
    public async Task Unite_cycle_rejete()
    {
        var (_, villeId) = await SeedVilleAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new UniteOrganisationnelleService(context);

        var a = await svc.CreateAsync(new CreateUniteOrganisationnelleCommand(
            "A", TypeUnite.Unite, villeId, null));

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var current = await context.HistoriqueOrganigrammeUniteSet
            .FirstAsync(h => h.IdUnite == a.Id && h.DateFin == null);
        current.DateDebut = today.AddDays(-10);
        current.DateFin = today.AddDays(-1);
        await context.SaveChangesAsync();

        context.HistoriqueOrganigrammeUniteSet.Add(new HistoriqueOrganigrammeUnite
        {
            Id = Guid.NewGuid(),
            IdUnite = a.Id,
            IdUniteParent = a.Id,
            DateDebut = today,
            DateFin = null
        });
        await context.SaveChangesAsync();

        using var second = TestDatabaseFixture.CreateContext();
        var svc2 = new UniteOrganisationnelleService(second);

        await Assert.ThrowsAsync<RegleMetierException>(
            () => svc2.CreateAsync(new CreateUniteOrganisationnelleCommand(
                "B", TypeUnite.Unite, villeId, a.Id)));
    }

    [Fact]
    public async Task Periode_check_viole_depuis_la_base()
    {
        var (_, villeId) = await SeedVilleAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new TribunalService(context);

        var t = await svc.CreateAsync(NewTribunal(
            "TChk", NiveauJuridiction.Cassation, TypeTribunal.Generale, villeId, null));

        context.HistoriqueOrganigrammeTribunalSet.Add(new HistoriqueOrganigrammeTribunal
        {
            Id = Guid.NewGuid(),
            IdTribunal = t.Id,
            DateDebut = new DateOnly(2024, 6, 1),
            DateFin = new DateOnly(2024, 1, 1)
        });

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task Second_enregistrement_ouvert_viole_index_depuis_la_base()
    {
        var (_, villeId) = await SeedVilleAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new TribunalService(context);

        var t = await svc.CreateAsync(NewTribunal(
            "TOuv", NiveauJuridiction.Cassation, TypeTribunal.Generale, villeId, null));

        context.HistoriqueOrganigrammeTribunalSet.Add(new HistoriqueOrganigrammeTribunal
        {
            Id = Guid.NewGuid(),
            IdTribunal = t.Id,
            DateDebut = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-5),
            DateFin = null
        });

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task Tribunaux_meme_nom_acceptes()
    {
        var (_, villeId) = await SeedVilleAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new TribunalService(context);

        var tag = Guid.NewGuid().ToString("N")[..6];
        var a = await svc.CreateAsync(NewTribunal(
            $"TPI-{tag}", NiveauJuridiction.Cassation, TypeTribunal.Generale, villeId, null));
        var b = await svc.CreateAsync(NewTribunal(
            $"TPI-{tag}", NiveauJuridiction.Cassation, TypeTribunal.Generale, villeId, null));

        Assert.NotEqual(a.Id, b.Id);
    }
}
