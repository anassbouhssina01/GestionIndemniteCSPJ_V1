using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Application.RHOrganisation.Services;
using GestionIndemnite.Application.RHOrganisation.Services.Referentiels;
using GestionIndemnite.Domain.RHOrganisation.Entities;
using GestionIndemnite.Domain.RHOrganisation.Enums;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using GestionIndemnite.Tests.Infrastructure;

namespace GestionIndemnite.Tests;

[Collection("SqlServer")]
public class RapportServiceTests
{
    private readonly TestDatabaseFixture _fixture;

    public RapportServiceTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task<(Guid FoncId, Guid GradeId, Guid StatutId, Guid RespId,
        Guid TribunalId, Guid UniteId)> SeedAsync()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var tag = Guid.NewGuid().ToString("N")[..6];

        var foncSvc = new FonctionnaireService(context);
        var fonc = await foncSvc.CreateAsync(new CreateFonctionnaireCommand(
            $"RP{tag}", $"RPC{tag}", "ب", "أ", "B", "A",
            new DateOnly(1985, 1, 1), new DateOnly(2010, 9, 1), Genre.Masculin));

        var cadreSvc = new CadreService(context);
        var cadre = await cadreSvc.CreateAsync(new CreateCadreCommand($"RC{tag}", "ع", "C", null));

        var gradeSvc = new GradeService(context);
        var grade = await gradeSvc.CreateAsync(new CreateGradeCommand(cadre.Id, 1, "ع", "G", null));

        var statutSvc = new StatutAdministratifService(context);
        var statut = await statutSvc.CreateAsync(new CreateStatutAdministratifCommand($"RS{tag}", "ع", "S", null));

        var respSvc = new ResponsabiliteService(context);
        var resp = await respSvc.CreateAsync(new CreateResponsabiliteCommand($"RR{tag}", "ع", "R", null));

        var paysSvc = new PaysService(context);
        var pays = await paysSvc.CreateAsync(new CreatePaysCommand($"PP{tag}", "ب", "P", false));
        var villeSvc = new VilleService(context);
        var ville = await villeSvc.CreateAsync(new CreateVilleCommand($"RV{tag}", pays.Id, null, null));

        var tribunalSvc = new TribunalService(context);
        var tribunal = await tribunalSvc.CreateAsync(new CreateTribunalCommand(
            $"RT-{tag}", TypeTribunal.Generale, NiveauJuridiction.Cassation, ville.Id, null));

        var uniteSvc = new UniteOrganisationnelleService(context);
        var unite = await uniteSvc.CreateAsync(new CreateUniteOrganisationnelleCommand(
            $"RU-{tag}", TypeUnite.Unite, ville.Id, null));

        return (fonc.Id, grade.Id, statut.Id, resp.Id, tribunal.Id, unite.Id);
    }

    [Fact]
    public async Task Trous_deux_periodes_avec_trou()
    {
        var (foncId, gradeId, _, _, _, _) = await SeedAsync();

        using var context = TestDatabaseFixture.CreateContext();
        context.HistoriqueCadreGrades.AddRange(
            new HistoriqueCadreGrade
            {
                Id = Guid.NewGuid(),
                IdFonctionnaire = foncId,
                IdGrade = gradeId,
                DateDebut = new DateOnly(2010, 9, 1),
                DateFin = new DateOnly(2012, 8, 31)
            },
            new HistoriqueCadreGrade
            {
                Id = Guid.NewGuid(),
                IdFonctionnaire = foncId,
                IdGrade = gradeId,
                DateDebut = new DateOnly(2015, 9, 1),
                DateFin = new DateOnly(2017, 8, 31)
            });
        await context.SaveChangesAsync();

        using var second = TestDatabaseFixture.CreateContext();
        var svc = new RapportService(second, new PeriodeService(second));

        var trous = await svc.GetTrousAsync(TypeHistoriqueRapport.CadreGrade, foncId);

        var trou = Assert.Single(trous);
        Assert.Equal(new DateOnly(2012, 9, 1), trou.DateDebut);
        Assert.Equal(new DateOnly(2015, 8, 31), trou.DateFin);
    }

    [Fact]
    public async Task Trous_periodes_contigues_pas_de_trou()
    {
        var (foncId, gradeId, _, _, _, _) = await SeedAsync();

        using var context = TestDatabaseFixture.CreateContext();
        context.HistoriqueCadreGrades.AddRange(
            new HistoriqueCadreGrade
            {
                Id = Guid.NewGuid(),
                IdFonctionnaire = foncId,
                IdGrade = gradeId,
                DateDebut = new DateOnly(2010, 9, 1),
                DateFin = new DateOnly(2012, 8, 31)
            },
            new HistoriqueCadreGrade
            {
                Id = Guid.NewGuid(),
                IdFonctionnaire = foncId,
                IdGrade = gradeId,
                DateDebut = new DateOnly(2012, 9, 1),
                DateFin = new DateOnly(2015, 8, 31)
            });
        await context.SaveChangesAsync();

        using var second = TestDatabaseFixture.CreateContext();
        var svc = new RapportService(second, new PeriodeService(second));

        var trous = await svc.GetTrousAsync(TypeHistoriqueRapport.CadreGrade, foncId);

        Assert.Empty(trous);
    }

    [Fact]
    public async Task Trous_type_invalide_rejete()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new RapportService(context, new PeriodeService(context));

        await Assert.ThrowsAsync<RegleMetierException>(() => svc.GetTrousAsync(
            (TypeHistoriqueRapport)99, Guid.NewGuid()));
    }

    [Fact]
    public async Task SansJustificatif_detecte()
    {
        var (foncId, gradeId, statutId, _, _, _) = await SeedAsync();

        using var context = TestDatabaseFixture.CreateContext();
        var doc = new PieceJointe
        {
            Id = Guid.NewGuid(),
            NumeroDocument = "DOC-RAP-1",
            NumeroDocumentNormalise = "DOCRAP1",
            DateDocument = new DateOnly(2010, 9, 1),
            AnneeDocument = 2010,
            Emetteur = "TGI",
            EmetteurNormalise = "TGI",
            TypeDocument = "Arrete",
            Objet = "Nomination",
            DateAjout = new DateOnly(2010, 9, 1)
        };
        context.PieceJointes.Add(doc);

        context.HistoriqueCadreGrades.Add(new HistoriqueCadreGrade
        {
            Id = Guid.NewGuid(),
            IdFonctionnaire = foncId,
            IdGrade = gradeId,
            DateDebut = new DateOnly(2010, 9, 1),
            DateFin = null
        });
        context.SituationAdministratives.Add(new SituationAdministrative
        {
            Id = Guid.NewGuid(),
            IdFonctionnaire = foncId,
            IdStatut = statutId,
            DateDebut = new DateOnly(2010, 9, 1),
            DateFin = null,
            IdDocument = doc.Id
        });
        await context.SaveChangesAsync();

        using var second = TestDatabaseFixture.CreateContext();
        var svc = new RapportService(second, new PeriodeService(second));

        var lignes = await svc.GetSansJustificatifAsync();

        var ligne = Assert.Single(lignes, l => l.IdCle == foncId);
        Assert.Equal(TypeHistoriqueRapport.CadreGrade, ligne.Type);
        Assert.Null(ligne.DateFin);
    }

    [Fact]
    public async Task SansJustificatif_filtre_par_type()
    {
        var (foncId, gradeId, statutId, _, _, _) = await SeedAsync();

        using var context = TestDatabaseFixture.CreateContext();
        context.HistoriqueCadreGrades.Add(new HistoriqueCadreGrade
        {
            Id = Guid.NewGuid(),
            IdFonctionnaire = foncId,
            IdGrade = gradeId,
            DateDebut = new DateOnly(2010, 9, 1),
            DateFin = null
        });
        context.SituationAdministratives.Add(new SituationAdministrative
        {
            Id = Guid.NewGuid(),
            IdFonctionnaire = foncId,
            IdStatut = statutId,
            DateDebut = new DateOnly(2010, 9, 1),
            DateFin = null
        });
        await context.SaveChangesAsync();

        using var second = TestDatabaseFixture.CreateContext();
        var svc = new RapportService(second, new PeriodeService(second));

        var lignes = await svc.GetSansJustificatifAsync(TypeHistoriqueRapport.SituationAdministrative);

        var ligne = Assert.Single(lignes, l => l.IdCle == foncId);
        Assert.Equal(TypeHistoriqueRapport.SituationAdministrative, ligne.Type);
    }

    [Fact]
    public async Task Chevauchement_detecte_depuis_la_base()
    {
        var (foncId, gradeId, _, _, _, _) = await SeedAsync();

        using var context = TestDatabaseFixture.CreateContext();
        context.HistoriqueCadreGrades.AddRange(
            new HistoriqueCadreGrade
            {
                Id = Guid.NewGuid(),
                IdFonctionnaire = foncId,
                IdGrade = gradeId,
                DateDebut = new DateOnly(2010, 9, 1),
                DateFin = new DateOnly(2015, 8, 31)
            },
            new HistoriqueCadreGrade
            {
                Id = Guid.NewGuid(),
                IdFonctionnaire = foncId,
                IdGrade = gradeId,
                DateDebut = new DateOnly(2013, 1, 1),
                DateFin = new DateOnly(2017, 8, 31)
            });
        await context.SaveChangesAsync();

        using var second = TestDatabaseFixture.CreateContext();
        var svc = new RapportService(second, new PeriodeService(second));

        var chevauchements = await svc.ControleChevauchementsAsync();

        var chev = Assert.Single(chevauchements, c => c.IdCle == foncId);
        Assert.Equal(TypeHistoriqueRapport.CadreGrade, chev.Type);
        Assert.Equal(new DateOnly(2013, 1, 1), chev.DateDebut2);
    }

    [Fact]
    public async Task Chevauchement_aucun_sur_donnees_propres()
    {
        var (foncId, gradeId, _, _, _, _) = await SeedAsync();

        using var context = TestDatabaseFixture.CreateContext();
        context.HistoriqueCadreGrades.Add(new HistoriqueCadreGrade
        {
            Id = Guid.NewGuid(),
            IdFonctionnaire = foncId,
            IdGrade = gradeId,
            DateDebut = new DateOnly(2010, 9, 1),
            DateFin = null
        });
        await context.SaveChangesAsync();

        using var second = TestDatabaseFixture.CreateContext();
        var svc = new RapportService(second, new PeriodeService(second));

        var chevauchements = await svc.ControleChevauchementsAsync();

        Assert.DoesNotContain(chevauchements, c => c.IdCle == foncId);
    }

    [Fact]
    public async Task Trous_affectation_depuis_la_base()
    {
        var (foncId, _, _, _, tribunalId, _) = await SeedAsync();

        using var context = TestDatabaseFixture.CreateContext();
        context.HistoriqueAffectations.AddRange(
            new HistoriqueAffectation
            {
                Id = Guid.NewGuid(),
                IdFonctionnaire = foncId,
                IdTribunalAffectation = tribunalId,
                DateDebut = new DateOnly(2015, 1, 1),
                DateFin = new DateOnly(2016, 12, 31)
            },
            new HistoriqueAffectation
            {
                Id = Guid.NewGuid(),
                IdFonctionnaire = foncId,
                IdTribunalAffectation = tribunalId,
                DateDebut = new DateOnly(2018, 1, 1),
                DateFin = null
            });
        await context.SaveChangesAsync();

        using var second = TestDatabaseFixture.CreateContext();
        var svc = new RapportService(second, new PeriodeService(second));

        var trous = await svc.GetTrousAsync(TypeHistoriqueRapport.Affectation, foncId);

        var trou = Assert.Single(trous);
        Assert.Equal(new DateOnly(2017, 1, 1), trou.DateDebut);
        Assert.Equal(new DateOnly(2017, 12, 31), trou.DateFin);
    }

    [Fact]
    public async Task Chevauchement_type_invalide_rejete()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new RapportService(context, new PeriodeService(context));

        await Assert.ThrowsAsync<RegleMetierException>(() => svc.ControleChevauchementsAsync(
            (TypeHistoriqueRapport)99));
    }

    [Fact]
    public async Task SansJustificatif_type_invalide_rejete()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new RapportService(context, new PeriodeService(context));

        await Assert.ThrowsAsync<RegleMetierException>(() => svc.GetSansJustificatifAsync(
            (TypeHistoriqueRapport)99));
    }
}
