using System.Reflection;
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
public class HistoriqueServicesTests
{
    private readonly TestDatabaseFixture _fixture;

    public HistoriqueServicesTests(TestDatabaseFixture fixture)
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
            $"H{tag}", $"HC{tag}", "ب", "أ", "B", "A",
            new DateOnly(1985, 1, 1), new DateOnly(2010, 9, 1), Genre.Masculin));

        var cadreSvc = new CadreService(context);
        var cadre = await cadreSvc.CreateAsync(new CreateCadreCommand($"C{tag}", "ع", "C", null));

        var gradeSvc = new GradeService(context);
        var grade = await gradeSvc.CreateAsync(new CreateGradeCommand(cadre.Id, 1, "ع", "G", null));

        var statutSvc = new StatutAdministratifService(context);
        var statut = await statutSvc.CreateAsync(new CreateStatutAdministratifCommand($"S{tag}", "ع", "S", null));

        var respSvc = new ResponsabiliteService(context);
        var resp = await respSvc.CreateAsync(new CreateResponsabiliteCommand($"R{tag}", "ع", "R", null));

        var paysSvc = new PaysService(context);
        var pays = await paysSvc.CreateAsync(new CreatePaysCommand($"P{tag}", "ب", "P", false));
        var villeSvc = new VilleService(context);
        var ville = await villeSvc.CreateAsync(new CreateVilleCommand($"V{tag}", pays.Id, null, null));

        var tribunalSvc = new TribunalService(context);
        var tribunal = await tribunalSvc.CreateAsync(new CreateTribunalCommand(
            $"T-{tag}", TypeTribunal.Generale, NiveauJuridiction.Cassation, ville.Id, null));

        var uniteSvc = new UniteOrganisationnelleService(context);
        var unite = await uniteSvc.CreateAsync(new CreateUniteOrganisationnelleCommand(
            $"U-{tag}", TypeUnite.Unite, ville.Id, null));

        return (fonc.Id, grade.Id, statut.Id, resp.Id, tribunal.Id, unite.Id);
    }

    [Fact]
    public async Task CadreGrade_ajout_reussi()
    {
        var (foncId, gradeId, _, _, _, _) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new HistoriqueCadreGradeService(context, new PeriodeService(context));

        var id = await svc.AddAsync(new CreateHistoriqueCadreGradeCommand(
            foncId, gradeId, new DateOnly(2010, 9, 1), null, null));

        Assert.NotEqual(default, id);
    }

    [Fact]
    public async Task CadreGrade_chevauchement_rejete()
    {
        var (foncId, gradeId, _, _, _, _) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new HistoriqueCadreGradeService(context, new PeriodeService(context));

        await svc.AddAsync(new CreateHistoriqueCadreGradeCommand(
            foncId, gradeId, new DateOnly(2010, 9, 1), new DateOnly(2015, 8, 31), null));

        await Assert.ThrowsAsync<RegleMetierException>(() => svc.AddAsync(
            new CreateHistoriqueCadreGradeCommand(
                foncId, gradeId, new DateOnly(2015, 1, 1), null, null)));
    }

    [Fact]
    public async Task Situation_premier_date_avant_recrutement_rejetee()
    {
        var (foncId, _, statutId, _, _, _) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new SituationAdministrativeService(context, new PeriodeService(context));

        await Assert.ThrowsAsync<RegleMetierException>(() => svc.AddAsync(
            new CreateSituationAdministrativeCommand(
                foncId, statutId, new DateOnly(2010, 8, 31), null, null)));
    }

    [Fact]
    public async Task Situation_premier_date_egal_recrutement_acceptee()
    {
        var (foncId, _, statutId, _, _, _) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new SituationAdministrativeService(context, new PeriodeService(context));

        var id = await svc.AddAsync(new CreateSituationAdministrativeCommand(
            foncId, statutId, new DateOnly(2010, 9, 1), null, null));

        Assert.NotEqual(default, id);
    }

    [Fact]
    public async Task Situation_date_anterieure_avec_historique_rejetee()
    {
        var (foncId, _, statutId, _, _, _) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new SituationAdministrativeService(context, new PeriodeService(context));

        await svc.AddAsync(new CreateSituationAdministrativeCommand(
            foncId, statutId, new DateOnly(2010, 9, 1), new DateOnly(2012, 8, 31), null));

        await Assert.ThrowsAsync<RegleMetierException>(() => svc.AddAsync(
            new CreateSituationAdministrativeCommand(
                foncId, statutId, new DateOnly(2010, 1, 1), new DateOnly(2010, 8, 31), null)));
    }

    [Fact]
    public async Task Affectation_tribunal_reussie()
    {
        var (foncId, _, _, _, tribunalId, _) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new HistoriqueAffectationService(context, new PeriodeService(context));

        var id = await svc.AddAsync(new CreateHistoriqueAffectationCommand(
            foncId, tribunalId, null, null, SectionTribunal.Parquet,
            new DateOnly(2015, 1, 1), null, null));

        Assert.NotEqual(default, id);
    }

    [Fact]
    public async Task Affectation_unite_reussie()
    {
        var (foncId, _, _, _, _, uniteId) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new HistoriqueAffectationService(context, new PeriodeService(context));

        var id = await svc.AddAsync(new CreateHistoriqueAffectationCommand(
            foncId, null, uniteId, null, null,
            new DateOnly(2015, 1, 1), null, null));

        Assert.NotEqual(default, id);
    }

    [Fact]
    public async Task Affectation_les_deux_rejetee()
    {
        var (foncId, _, _, _, tribunalId, uniteId) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new HistoriqueAffectationService(context, new PeriodeService(context));

        await Assert.ThrowsAsync<RegleMetierException>(() => svc.AddAsync(
            new CreateHistoriqueAffectationCommand(
                foncId, tribunalId, uniteId, null, null,
                new DateOnly(2015, 1, 1), null, null)));
    }

    [Fact]
    public async Task Affectation_aucun_rejetee()
    {
        var (foncId, _, _, _, _, _) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new HistoriqueAffectationService(context, new PeriodeService(context));

        await Assert.ThrowsAsync<RegleMetierException>(() => svc.AddAsync(
            new CreateHistoriqueAffectationCommand(
                foncId, null, null, null, null,
                new DateOnly(2015, 1, 1), null, null)));
    }

    [Fact]
    public async Task Affectation_section_sans_tribunal_rejetee()
    {
        var (foncId, _, _, _, _, uniteId) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new HistoriqueAffectationService(context, new PeriodeService(context));

        await Assert.ThrowsAsync<RegleMetierException>(() => svc.AddAsync(
            new CreateHistoriqueAffectationCommand(
                foncId, null, uniteId, null, SectionTribunal.Parquet,
                new DateOnly(2015, 1, 1), null, null)));
    }

    [Fact]
    public async Task Affectation_unite_travail_sans_tribunal_rejetee()
    {
        var (foncId, _, _, _, _, uniteId) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new HistoriqueAffectationService(context, new PeriodeService(context));

        await Assert.ThrowsAsync<RegleMetierException>(() => svc.AddAsync(
            new CreateHistoriqueAffectationCommand(
                foncId, null, uniteId, uniteId, null,
                new DateOnly(2015, 1, 1), null, null)));
    }

    [Fact]
    public async Task Affectation_check_exactement_un_viole_depuis_la_base()
    {
        var (foncId, _, _, _, tribunalId, uniteId) = await SeedAsync();

        using var context = TestDatabaseFixture.CreateContext();
        context.HistoriqueAffectations.Add(new HistoriqueAffectation
        {
            Id = Guid.NewGuid(),
            IdFonctionnaire = foncId,
            IdTribunalAffectation = tribunalId,
            IdUniteAffectation = uniteId,
            DateDebut = new DateOnly(2015, 1, 1)
        });

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task Responsabilite_tribunal_reussie()
    {
        var (foncId, _, _, respId, tribunalId, _) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new HistoriqueResponsabiliteService(context, new PeriodeService(context));

        var id = await svc.AddAsync(new CreateHistoriqueResponsabiliteCommand(
            foncId, respId, tribunalId, null,
            new DateOnly(2015, 1, 1), null, null));

        Assert.NotEqual(default, id);
    }

    [Fact]
    public async Task Responsabilite_simultanees_differentes_acceptees()
    {
        var (foncId, _, _, respId, tribunalId, uniteId) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new HistoriqueResponsabiliteService(context, new PeriodeService(context));

        await svc.AddAsync(new CreateHistoriqueResponsabiliteCommand(
            foncId, respId, tribunalId, null,
            new DateOnly(2015, 1, 1), null, null));
        await svc.AddAsync(new CreateHistoriqueResponsabiliteCommand(
            foncId, respId, null, uniteId,
            new DateOnly(2015, 1, 1), null, null));
    }

    [Fact]
    public async Task Responsabilite_les_deux_rejetee()
    {
        var (foncId, _, _, respId, tribunalId, uniteId) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new HistoriqueResponsabiliteService(context, new PeriodeService(context));

        await Assert.ThrowsAsync<RegleMetierException>(() => svc.AddAsync(
            new CreateHistoriqueResponsabiliteCommand(
                foncId, respId, tribunalId, uniteId,
                new DateOnly(2015, 1, 1), null, null)));
    }

    [Fact]
    public async Task OrganigrammeTribunal_changement_parent_reussi()
    {
        var (_, _, _, _, cassationId, _) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var tribunalSvc = new TribunalService(context);
        var villeId = context.Tribunaux.First().IdVille;

        var cassation2 = await tribunalSvc.CreateAsync(new CreateTribunalCommand(
            "CassChg2", TypeTribunal.Generale, NiveauJuridiction.Cassation, villeId, null));
        var ca = await tribunalSvc.CreateAsync(new CreateTribunalCommand(
            "CAChg", TypeTribunal.Generale, NiveauJuridiction.CA, villeId, cassationId));

        using var second = TestDatabaseFixture.CreateContext();
        var svc = new HistoriqueOrganigrammeTribunalService(second, new PeriodeService(second));

        var id = await svc.ChangeParentAsync(ca.Id, new ChangeParentTribunalCommand(
            cassation2.Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1), null));

        Assert.NotEqual(default, id);
    }

    [Fact]
    public async Task OrganigrammeTribunal_changement_parent_mauvais_niveau_rejete()
    {
        var (_, _, _, _, cassationId, _) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var tribunalSvc = new TribunalService(context);
        var villeId = context.Tribunaux.First().IdVille;

        var ca = await tribunalSvc.CreateAsync(new CreateTribunalCommand(
            "CAMauvais", TypeTribunal.Generale, NiveauJuridiction.CA, villeId, cassationId));
        var tpi = await tribunalSvc.CreateAsync(new CreateTribunalCommand(
            "TpiMauvais", TypeTribunal.Generale, NiveauJuridiction.TPI, villeId, ca.Id));

        using var second = TestDatabaseFixture.CreateContext();
        var svc = new HistoriqueOrganigrammeTribunalService(second, new PeriodeService(second));

        await Assert.ThrowsAsync<RegleMetierException>(() => svc.ChangeParentAsync(
            ca.Id,
            new ChangeParentTribunalCommand(
                tpi.Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1), null)));
    }

    [Fact]
    public async Task OrganigrammeTribunal_self_parent_rejete()
    {
        var (_, _, _, _, tribunalId, _) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new HistoriqueOrganigrammeTribunalService(context, new PeriodeService(context));

        await Assert.ThrowsAsync<RegleMetierException>(() => svc.ChangeParentAsync(
            tribunalId,
            new ChangeParentTribunalCommand(
                tribunalId, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1), null)));
    }

    [Fact]
    public async Task OrganigrammeUnite_changement_parent_reussi()
    {
        var (_, _, _, _, _, uniteId) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var uniteSvc = new UniteOrganisationnelleService(context);

        var parent = await uniteSvc.CreateAsync(new CreateUniteOrganisationnelleCommand(
            "ParentU", TypeUnite.Pole, context.UnitesOrganisationnelles.First().IdVille, null));

        using var second = TestDatabaseFixture.CreateContext();
        var svc = new HistoriqueOrganigrammeUniteService(second, new PeriodeService(second));

        var id = await svc.ChangeParentAsync(uniteId, new ChangeParentUniteCommand(
            parent.Id, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1), null));

        Assert.NotEqual(default, id);
    }

    [Fact]
    public async Task OrganigrammeUnite_self_parent_rejete()
    {
        var (_, _, _, _, _, uniteId) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new HistoriqueOrganigrammeUniteService(context, new PeriodeService(context));

        await Assert.ThrowsAsync<RegleMetierException>(() => svc.ChangeParentAsync(
            uniteId,
            new ChangeParentUniteCommand(
                uniteId, DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1), null)));
    }

    [Fact]
    public async Task Deux_enregistrements_ouverts_cadre_grade_rejetes_par_la_base()
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
        second.HistoriqueCadreGrades.Add(new HistoriqueCadreGrade
        {
            Id = Guid.NewGuid(),
            IdFonctionnaire = foncId,
            IdGrade = gradeId,
            DateDebut = new DateOnly(2012, 9, 1),
            DateFin = null
        });

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => second.SaveChangesAsync());
    }

    [Fact]
    public async Task Affectation_check_unite_travail_viole_depuis_la_base()
    {
        var (foncId, _, _, _, _, uniteId) = await SeedAsync();

        using var context = TestDatabaseFixture.CreateContext();
        context.HistoriqueAffectations.Add(new HistoriqueAffectation
        {
            Id = Guid.NewGuid(),
            IdFonctionnaire = foncId,
            IdUniteAffectation = uniteId,
            IdUniteTravail = uniteId,
            DateDebut = new DateOnly(2015, 1, 1)
        });

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task Affectation_check_section_sans_tribunal_viole_depuis_la_base()
    {
        var (foncId, _, _, _, _, uniteId) = await SeedAsync();

        using var context = TestDatabaseFixture.CreateContext();
        context.HistoriqueAffectations.Add(new HistoriqueAffectation
        {
            Id = Guid.NewGuid(),
            IdFonctionnaire = foncId,
            IdUniteAffectation = uniteId,
            SectionTribunal = SectionTribunal.Presidence,
            DateDebut = new DateOnly(2015, 1, 1)
        });

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task Grade_utilise_ne_peut_etre_desactive()
    {
        var (foncId, gradeId, _, _, _, _) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var histSvc = new HistoriqueCadreGradeService(context, new PeriodeService(context));
        await histSvc.AddAsync(new CreateHistoriqueCadreGradeCommand(
            foncId, gradeId, new DateOnly(2010, 9, 1), null, null));

        using var second = TestDatabaseFixture.CreateContext();
        var gradeSvc = new GradeService(second);

        await Assert.ThrowsAsync<RegleMetierException>(() => gradeSvc.DisableAsync(gradeId));
    }

    [Fact]
    public async Task Statut_utilise_ne_peut_etre_desactive()
    {
        var (foncId, _, statutId, _, _, _) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var histSvc = new SituationAdministrativeService(context, new PeriodeService(context));
        await histSvc.AddAsync(new CreateSituationAdministrativeCommand(
            foncId, statutId, new DateOnly(2010, 9, 1), null, null));

        using var second = TestDatabaseFixture.CreateContext();
        var statutSvc = new StatutAdministratifService(second);

        await Assert.ThrowsAsync<RegleMetierException>(() => statutSvc.DisableAsync(statutId));
    }

    [Fact]
    public async Task Responsabilite_utilisee_ne_peut_etre_desactivee()
    {
        var (foncId, _, _, respId, tribunalId, _) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var histSvc = new HistoriqueResponsabiliteService(context, new PeriodeService(context));
        await histSvc.AddAsync(new CreateHistoriqueResponsabiliteCommand(
            foncId, respId, tribunalId, null, new DateOnly(2015, 1, 1), null, null));

        using var second = TestDatabaseFixture.CreateContext();
        var respSvc = new ResponsabiliteService(second);

        await Assert.ThrowsAsync<RegleMetierException>(() => respSvc.DisableAsync(respId));
    }

    [Fact]
    public async Task Cadre_utilise_ne_peut_etre_desactive()
    {
        var (foncId, gradeId, _, _, _, _) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var histSvc = new HistoriqueCadreGradeService(context, new PeriodeService(context));
        await histSvc.AddAsync(new CreateHistoriqueCadreGradeCommand(
            foncId, gradeId, new DateOnly(2010, 9, 1), null, null));

        var idCadre = context.Grades.AsNoTracking().First(g => g.Id == gradeId).IdCadre;

        using var second = TestDatabaseFixture.CreateContext();
        var cadreSvc = new CadreService(second);

        await Assert.ThrowsAsync<RegleMetierException>(() => cadreSvc.DisableAsync(idCadre));
    }

    [Fact]
    public async Task Fonctionnaire_avec_historique_ne_peut_etre_supprime()
    {
        var (foncId, gradeId, _, _, _, _) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var histSvc = new HistoriqueCadreGradeService(context, new PeriodeService(context));
        await histSvc.AddAsync(new CreateHistoriqueCadreGradeCommand(
            foncId, gradeId, new DateOnly(2010, 9, 1), null, null));

        using var second = TestDatabaseFixture.CreateContext();
        var foncSvc = new FonctionnaireService(second);

        await Assert.ThrowsAsync<RegleMetierException>(() => foncSvc.DeleteAsync(foncId));
    }

    [Fact]
    public async Task CadreGrade_changement_cloture_l_ancienne_periode()
    {
        var (foncId, gradeId, _, _, _, _) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new HistoriqueCadreGradeService(context, new PeriodeService(context));

        await svc.AddAsync(new CreateHistoriqueCadreGradeCommand(
            foncId, gradeId, new DateOnly(2010, 9, 1), null, null));

        var idCadre = context.Grades.AsNoTracking().First(g => g.Id == gradeId).IdCadre;
        var grade2 = await new GradeService(context).CreateAsync(
            new CreateGradeCommand(idCadre, 2, "ع2", "G2", null));

        await svc.ChangeAsync(foncId, grade2.Id, new DateOnly(2015, 1, 1), null);

        using var check = TestDatabaseFixture.CreateContext();
        var periodes = await check.HistoriqueCadreGrades.AsNoTracking()
            .Where(h => h.IdFonctionnaire == foncId)
            .OrderBy(h => h.DateDebut)
            .ToListAsync();

        Assert.Equal(2, periodes.Count);
        Assert.Equal(new DateOnly(2014, 12, 31), periodes[0].DateFin);
        Assert.Equal(grade2.Id, periodes[1].IdGrade);
        Assert.Null(periodes[1].DateFin);
    }

    [Fact]
    public async Task Situation_changement_cloture_l_ancienne_periode()
    {
        var (foncId, _, statutId, _, _, _) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new SituationAdministrativeService(context, new PeriodeService(context));

        await svc.AddAsync(new CreateSituationAdministrativeCommand(
            foncId, statutId, new DateOnly(2010, 9, 1), null, null));

        var tag = Guid.NewGuid().ToString("N")[..6];
        var statut2 = await new StatutAdministratifService(context).CreateAsync(
            new CreateStatutAdministratifCommand($"S2{tag}", "ع2", "S2", null));

        await svc.ChangeAsync(foncId, statut2.Id, new DateOnly(2016, 1, 1), null);

        using var check = TestDatabaseFixture.CreateContext();
        var periodes = await check.SituationAdministratives.AsNoTracking()
            .Where(s => s.IdFonctionnaire == foncId)
            .OrderBy(s => s.DateDebut)
            .ToListAsync();

        Assert.Equal(2, periodes.Count);
        Assert.Equal(new DateOnly(2015, 12, 31), periodes[0].DateFin);
        Assert.Equal(statut2.Id, periodes[1].IdStatut);
        Assert.Null(periodes[1].DateFin);
    }

    [Fact]
    public async Task Affectation_changement_cloture_l_ancienne_periode()
    {
        var (foncId, _, _, _, tribunalId, uniteId) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new HistoriqueAffectationService(context, new PeriodeService(context));

        await svc.AddAsync(new CreateHistoriqueAffectationCommand(
            foncId, tribunalId, null, null, SectionTribunal.Parquet,
            new DateOnly(2015, 1, 1), null, null));

        await svc.ChangeAsync(
            foncId, null, uniteId, null, null, new DateOnly(2017, 1, 1), null);

        using var check = TestDatabaseFixture.CreateContext();
        var periodes = await check.HistoriqueAffectations.AsNoTracking()
            .Where(h => h.IdFonctionnaire == foncId)
            .OrderBy(h => h.DateDebut)
            .ToListAsync();

        Assert.Equal(2, periodes.Count);
        Assert.Equal(new DateOnly(2016, 12, 31), periodes[0].DateFin);
        Assert.Equal(uniteId, periodes[1].IdUniteAffectation);
        Assert.Null(periodes[1].DateFin);
    }

    [Fact]
    public async Task Responsabilite_cloture_reussie()
    {
        var (foncId, _, _, respId, tribunalId, _) = await SeedAsync();
        using var context = TestDatabaseFixture.CreateContext();
        var svc = new HistoriqueResponsabiliteService(context, new PeriodeService(context));

        var id = await svc.AddAsync(new CreateHistoriqueResponsabiliteCommand(
            foncId, respId, tribunalId, null,
            new DateOnly(2015, 1, 1), null, null));

        await svc.CloseAsync(id, new DateOnly(2020, 12, 31));

        using var check = TestDatabaseFixture.CreateContext();
        var periode = await check.HistoriqueResponsabilites.AsNoTracking()
            .FirstAsync(h => h.Id == id);

        Assert.Equal(new DateOnly(2020, 12, 31), periode.DateFin);
    }

    [Fact]
    public void Test_51_services_ne_retournent_pas_d_entites_domaine()
    {
        const string entityNamespace = "GestionIndemnite.Domain.RHOrganisation.Entities";

        var serviceTypes = typeof(FonctionnaireService).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Namespace is not null
                && t.Namespace.StartsWith("GestionIndemnite.Application.RHOrganisation.Services"))
            .ToList();

        Assert.NotEmpty(serviceTypes);

        foreach (var type in serviceTypes)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName);

            foreach (var method in methods)
            {
                foreach (var returned in ExpandReturnType(method.ReturnType))
                {
                    Assert.False(
                        returned.Namespace == entityNamespace,
                        $"{type.Name}.{method.Name} retourne {returned.FullName} " +
                        "(entité Domain — interdit)");
                }
            }
        }
    }

    private static IEnumerable<Type> ExpandReturnType(Type type)
    {
        yield return type;

        if (!type.IsGenericType)
            yield break;

        foreach (var arg in type.GetGenericArguments())
        {
            foreach (var inner in ExpandReturnType(arg))
                yield return inner;
        }
    }
}
