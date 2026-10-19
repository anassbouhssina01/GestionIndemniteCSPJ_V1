using ClosedXML.Excel;
using GestionIndemnite.Application.RHOrganisation.Common;
using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Application.RHOrganisation.Services;
using GestionIndemnite.Application.RHOrganisation.Services.Referentiels;
using GestionIndemnite.Domain.RHOrganisation.Enums;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using GestionIndemnite.Infrastructure.RHOrganisation.Persistence;
using GestionIndemnite.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace GestionIndemnite.Tests;

[Collection("SqlServer")]
public class ImportServiceTests
{
    private readonly TestDatabaseFixture _fixture;

    public ImportServiceTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private static async Task<(string Tag, string CodeCadre, int Numero, string CodeStatut,
        string CodeResp, string NomTribunal, string NomUnite)> SeedReferentielsAsync()
    {
        using var context = TestDatabaseFixture.CreateContext();
        var tag = Guid.NewGuid().ToString("N")[..6];

        var cadre = await new CadreService(context).CreateAsync(
            new CreateCadreCommand($"IC{tag}", "ع", "C", null));
        var grade = await new GradeService(context).CreateAsync(
            new CreateGradeCommand(cadre.Id, 1, "ع", "G", null));
        var statut = await new StatutAdministratifService(context).CreateAsync(
            new CreateStatutAdministratifCommand($"IS{tag}", "ع", "S", null));
        var resp = await new ResponsabiliteService(context).CreateAsync(
            new CreateResponsabiliteCommand($"IR{tag}", "ع", "R", null));
        var pays = await new PaysService(context).CreateAsync(
            new CreatePaysCommand($"IP{tag}", "ب", "P", false));
        var ville = await new VilleService(context).CreateAsync(
            new CreateVilleCommand($"IV{tag}", pays.Id, null, null));
        var tribunal = await new TribunalService(context).CreateAsync(
            new CreateTribunalCommand($"IT-{tag}", TypeTribunal.Generale, NiveauJuridiction.Cassation, ville.Id, null));
        var unite = await new UniteOrganisationnelleService(context).CreateAsync(
            new CreateUniteOrganisationnelleCommand($"IU-{tag}", TypeUnite.Unite, ville.Id, null));

        return (tag, $"IC{tag}", 1, $"IS{tag}", $"IR{tag}", $"IT-{tag}", $"IU-{tag}");
    }

    private static ImportService CreerService(RHOrganisationDbContext context, ICurrentUser? user = null)
    {
        var periodes = new PeriodeService(context);
        return new ImportService(
            context,
            new FonctionnaireService(context),
            new RibService(context),
            new HistoriqueCadreGradeService(context, periodes),
            new SituationAdministrativeService(context, periodes),
            new HistoriqueAffectationService(context, periodes),
            new HistoriqueResponsabiliteService(context, periodes),
            user ?? new UtilisateurTest("test-user"));
    }

    [Fact]
    public async Task Import_ligne_erronnee_n_arrete_pas_les_autres()
    {
        var ms = new ClasseurTest();
        ms.Fonctionnaire("M1", "C1", "ب", "أ", "N1", "P1", "01/01/1980", "01/09/2010", "Masculin");
        ms.Fonctionnaire("M2", "C2", "ب", "أ", "N2", "P2", "13/45/2020", "01/09/2010", "Masculin");
        ms.Fonctionnaire("M3", "C3", "ب", "أ", "N3", "P3", "01/01/1982", "01/09/2011", "Féminin");
        using var fichier = ms.Terminer();

        using var context = TestDatabaseFixture.CreateContext();
        var resultat = await CreerService(context).ImporterAsync(
            fichier, new ImportOptions(false, "lignes.xlsx"));

        Assert.True(resultat.NombreErreurs >= 1);
        Assert.Contains("Date de naissance", resultat.ContenuRapport);
        Assert.Contains("format attendu jj/mm/aaaa", resultat.ContenuRapport);

        using var verif = TestDatabaseFixture.CreateContext();
        Assert.True(await verif.Fonctionnaires.AnyAsync(f => f.Matricule == "M1"));
        Assert.True(await verif.Fonctionnaires.AnyAsync(f => f.Matricule == "M3"));
        Assert.False(await verif.Fonctionnaires.AnyAsync(f => f.Matricule == "M2"));
    }

    [Fact]
    public async Task Import_relance_ne_duplique_rien()
    {
        var ms = new ClasseurTest();
        ms.Fonctionnaire("RL1", "RLC1", "ب", "أ", "N", "P", "01/01/1980", "01/09/2010", "Masculin");
        ms.Rib("RL1", "123", "456", "1234567890123456", "10");

        var octets = ms.SauverOctets();

        using (var premier = new MemoryStream(octets))
        using (var context = TestDatabaseFixture.CreateContext())
        {
            await CreerService(context).ImporterAsync(
                premier, new ImportOptions(false, "relance.xlsx"));
        }

        using (var second = new MemoryStream(octets))
        using (var context2 = TestDatabaseFixture.CreateContext())
        {
            var resultat2 = await CreerService(context2).ImporterAsync(
                second, new ImportOptions(false, "relance.xlsx"));

            Assert.Contains("fonctionnaire déjà importé", resultat2.ContenuRapport);
        }

        using var verif = TestDatabaseFixture.CreateContext();
        Assert.Equal(1, await verif.Fonctionnaires.CountAsync(f => f.Matricule == "RL1"));
        var foncId = await verif.Fonctionnaires
            .Where(f => f.Matricule == "RL1")
            .Select(f => f.Id)
            .FirstAsync();
        Assert.Equal(1, await verif.Ribs.CountAsync(r => r.IdFonctionnaire == foncId));
    }

    [Fact]
    public async Task Import_referentiel_inexistant_rejete()
    {
        var (_, codeCadre, numero, _, _, _, _) = await SeedReferentielsAsync();

        var ms = new ClasseurTest();
        ms.Fonctionnaire("RF1", "RFC1", "ب", "أ", "N", "P", "01/01/1980", "01/09/2010", "Masculin");
        ms.Grade("RF1", "CADRE-INEXISTANT", numero.ToString(), "01/09/2010", "");
        ms.Grade("RF1", codeCadre, "99999", "01/09/2012", "");
        using var fichier = ms.Terminer();

        using var context = TestDatabaseFixture.CreateContext();
        var resultat = await CreerService(context).ImporterAsync(
            fichier, new ImportOptions(false, "ref.xlsx"));

        Assert.Contains("cadre «CADRE-INEXISTANT» introuvable", resultat.ContenuRapport);
        Assert.Contains("introuvable pour le cadre", resultat.ContenuRapport);
        Assert.Contains("Groupe RF1 — REJETÉ", resultat.ContenuRapport);

        using var verif = TestDatabaseFixture.CreateContext();
        Assert.False(await verif.Fonctionnaires.AnyAsync(f => f.Matricule == "RF1"));
    }

    [Fact]
    public async Task Import_chevauchement_rejete_comme_regle_6_1()
    {
        var (_, _, _, codeStatut, _, _, _) = await SeedReferentielsAsync();

        var ms = new ClasseurTest();
        ms.Fonctionnaire("CH1", "CHC1", "ب", "أ", "N", "P", "01/01/1980", "01/09/2010", "Masculin");
        ms.Situation("CH1", codeStatut, "01/09/2010", "31/08/2012");
        ms.Situation("CH1", codeStatut, "01/01/2012", "31/12/2014");
        using var fichier = ms.Terminer();

        using var context = TestDatabaseFixture.CreateContext();
        var resultat = await CreerService(context).ImporterAsync(
            fichier, new ImportOptions(false, "chevauchement.xlsx"));

        Assert.Contains("توجد فترة متداخلة مع سجل موجود", resultat.ContenuRapport);
        Assert.Contains("Groupe CH1 — REJETÉ", resultat.ContenuRapport);

        using var verif = TestDatabaseFixture.CreateContext();
        Assert.False(await verif.Fonctionnaires.AnyAsync(f => f.Matricule == "CH1"));
        var ch1Id = await verif.Fonctionnaires
            .Where(f => f.Matricule == "CH1")
            .Select(f => f.Id)
            .FirstOrDefaultAsync();
        Assert.Equal(Guid.Empty, ch1Id);
        Assert.Equal(0, await verif.SituationAdministratives.CountAsync(
            s => s.IdFonctionnaire == ch1Id));
    }

    [Fact]
    public async Task Import_rapport_enregistre_l_utilisateur()
    {
        var utilisateur = new UtilisateurTest("import-user");
        var options = new DbContextOptionsBuilder<RHOrganisationDbContext>()
            .UseSqlServer(TestDatabaseFixture.ConnectionString)
            .Options;

        var ms = new ClasseurTest();
        ms.Fonctionnaire("U1", "UC1", "ب", "أ", "N", "P", "01/01/1980", "01/09/2010", "Masculin");
        using var fichier = ms.Terminer();

        using (var context = new RHOrganisationDbContext(options, utilisateur))
        {
            await CreerService(context, utilisateur).ImporterAsync(
                fichier, new ImportOptions(false, "utilisateur.xlsx"));
        }

        using var verif = TestDatabaseFixture.CreateContext();
        var rapport = await verif.RapportImports.AsNoTracking()
            .Where(r => r.NomFichier == "utilisateur.xlsx")
            .OrderByDescending(r => r.CreatedAt)
            .FirstAsync();

        Assert.Equal("import-user", rapport.CreatedBy);
        Assert.Contains("Importé par : import-user", rapport.ContenuRapport);
        Assert.Contains("le ", rapport.ContenuRapport);
    }

    [Fact]
    public async Task Import_validation_seule_rien_n_est_enregistre()
    {
        var (_, codeCadre, numero, codeStatut, _, _, _) = await SeedReferentielsAsync();

        var ms = new ClasseurTest();
        ms.Fonctionnaire("V1", "VC1", "ب", "أ", "N", "P", "01/01/1980", "01/09/2010", "Masculin");
        ms.Grade("V1", codeCadre, numero.ToString(), "01/09/2010", "");
        ms.Situation("V1", codeStatut, "01/09/2010", "");
        using var fichier = ms.Terminer();

        using var context = TestDatabaseFixture.CreateContext();
        var resultat = await CreerService(context).ImporterAsync(
            fichier, new ImportOptions(true, "seule.xlsx"));

        Assert.True(resultat.ModeValidationSeule);
        Assert.Contains("validation seule", resultat.ContenuRapport);
        Assert.Contains("Groupe V1 — SUCCÈS", resultat.ContenuRapport);
        Assert.Equal(0, resultat.NombreErreurs);

        using var verif = TestDatabaseFixture.CreateContext();
        Assert.False(await verif.Fonctionnaires.AnyAsync(f => f.Matricule == "V1"));
        Assert.Equal(1, await verif.RapportImports.CountAsync(r => r.NomFichier == "seule.xlsx"));
    }

    [Fact]
    public async Task Import_validation_seule_signale_erreur_par_ligne_colonne()
    {
        var ms = new ClasseurTest();
        ms.Fonctionnaire("VE1", "VEC1", "ب", "أ", "N", "P", "pas-une-date", "01/09/2010", "Masculin");
        ms.Fonctionnaire("VE2", "VEC2", "ب", "أ", "N", "P", "01/01/1980", "01/09/2010", "Inconnu");
        ms.Fonctionnaire("", "VEC3", "ب", "أ", "N", "P", "01/01/1980", "01/09/2010", "Masculin");
        using var fichier = ms.Terminer();

        using var context = TestDatabaseFixture.CreateContext();
        var resultat = await CreerService(context).ImporterAsync(
            fichier, new ImportOptions(true, "erreurs.xlsx"));

        Assert.Equal(3, resultat.NombreErreurs);
        Assert.Contains("Ligne 2 [Fonctionnaires] — ERREUR — Date de naissance", resultat.ContenuRapport);
        Assert.Contains("Ligne 3 [Fonctionnaires] — ERREUR — Genre", resultat.ContenuRapport);
        Assert.Contains("Ligne 4 [Fonctionnaires] — ERREUR — Matricule", resultat.ContenuRapport);
        Assert.Contains("champ obligatoire", resultat.ContenuRapport);

        using var verif = TestDatabaseFixture.CreateContext();
        Assert.Equal(0, await verif.Fonctionnaires.CountAsync(
            f => f.Matricule == "VE1" || f.Matricule == "VE2" || f.Matricule == "VE3"));
    }

    [Fact]
    public async Task Import_rib_invalide_rejete_le_groupe()
    {
        var ms = new ClasseurTest();
        ms.Fonctionnaire("RB1", "RBC1", "ب", "أ", "N", "P", "01/01/1980", "01/09/2010", "Masculin");
        ms.Rib("RB1", "AB", "456", "1234567890123456", "10");
        using var fichier = ms.Terminer();

        using var context = TestDatabaseFixture.CreateContext();
        var resultat = await CreerService(context).ImporterAsync(
            fichier, new ImportOptions(false, "rib.xlsx"));

        Assert.Contains("رمز البنك", resultat.ContenuRapport);
        Assert.Contains("Groupe RB1 — REJETÉ", resultat.ContenuRapport);

        using var verif = TestDatabaseFixture.CreateContext();
        Assert.False(await verif.Fonctionnaires.AnyAsync(f => f.Matricule == "RB1"));
    }

    [Fact]
    public async Task Import_fichier_superieur_a_5_mega_rejete()
    {
        using var fichier = new MemoryStream(new byte[ExcelImportReader.TailleMaxOctets + 1]);

        using var context = TestDatabaseFixture.CreateContext();
        var ex = await Assert.ThrowsAsync<RegleMetierException>(() =>
            CreerService(context).ImporterAsync(fichier, new ImportOptions(false, "gros.xlsx")));

        Assert.Contains("5 ميغا", ex.Message);
    }

    [Fact]
    public async Task Import_fichier_non_excel_rejete()
    {
        using var fichier = new MemoryStream(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });

        using var context = TestDatabaseFixture.CreateContext();
        var ex = await Assert.ThrowsAsync<RegleMetierException>(() =>
            CreerService(context).ImporterAsync(fichier, new ImportOptions(false, "faux.xlsx")));

        Assert.Contains("ليس ملف Excel صالح", ex.Message);
    }

    [Fact]
    public async Task Import_historiques_tries_par_date_debut()
    {
        var (_, _, _, codeStatut, _, _, _) = await SeedReferentielsAsync();

        var ms = new ClasseurTest();
        ms.Fonctionnaire("TR1", "TRC1", "ب", "أ", "N", "P", "01/01/1980", "01/09/2010", "Masculin");
        ms.Situation("TR1", codeStatut, "01/09/2015", "31/12/2017");
        ms.Situation("TR1", codeStatut, "01/09/2010", "31/08/2015");
        using var fichier = ms.Terminer();

        using var context = TestDatabaseFixture.CreateContext();
        var resultat = await CreerService(context).ImporterAsync(
            fichier, new ImportOptions(false, "tri.xlsx"));

        Assert.Equal(0, resultat.NombreErreurs);
        Assert.Contains("Groupe TR1 — SUCCÈS", resultat.ContenuRapport);

        using var verif = TestDatabaseFixture.CreateContext();
        var tr1Id = await verif.Fonctionnaires
            .Where(f => f.Matricule == "TR1")
            .Select(f => f.Id)
            .FirstAsync();
        var situations = await verif.SituationAdministratives.AsNoTracking()
            .Where(s => s.IdFonctionnaire == tr1Id)
            .OrderBy(s => s.DateDebut)
            .ToListAsync();

        Assert.Equal(2, situations.Count);
        Assert.Equal(new DateOnly(2010, 9, 1), situations[0].DateDebut);
        Assert.Equal(new DateOnly(2015, 9, 1), situations[1].DateDebut);
    }

    [Fact]
    public async Task Import_matricule_hors_fichier_et_hors_base_rejete()
    {
        var (_, codeCadre, numero, _, _, _, _) = await SeedReferentielsAsync();

        var ms = new ClasseurTest();
        ms.Fonctionnaire("HI1", "HIC1", "ب", "أ", "N", "P", "01/01/1980", "01/09/2010", "Masculin");
        ms.Grade("HI1", codeCadre, numero.ToString(), "01/09/2010", "");
        ms.Grade("MAT-ORPHELINE", codeCadre, numero.ToString(), "01/09/2012", "");
        using var fichier = ms.Terminer();

        using var context = TestDatabaseFixture.CreateContext();
        var resultat = await CreerService(context).ImporterAsync(
            fichier, new ImportOptions(false, "orpheline.xlsx"));

        Assert.Contains("Groupe MAT-ORPHELINE — REJETÉ — Matricule introuvable", resultat.ContenuRapport);

        using var verif = TestDatabaseFixture.CreateContext();
        Assert.True(await verif.Fonctionnaires.AnyAsync(f => f.Matricule == "HI1"));
        Assert.False(await verif.Fonctionnaires.AnyAsync(f => f.Matricule == "MAT-ORPHELINE"));
        var hi1Id = await verif.Fonctionnaires
            .Where(f => f.Matricule == "HI1")
            .Select(f => f.Id)
            .FirstAsync();
        Assert.Equal(1, await verif.HistoriqueCadreGrades.CountAsync(
            h => h.IdFonctionnaire == hi1Id));
    }

    [Fact]
    public async Task Import_matricule_duplique_dans_le_fichier_rejete_le_groupe()
    {
        var ms = new ClasseurTest();
        ms.Fonctionnaire("DP1", "DPC1", "ب", "أ", "N", "P", "01/01/1980", "01/09/2010", "Masculin");
        ms.Fonctionnaire("DP1", "DPC2", "ب", "أ", "N2", "P2", "01/01/1981", "01/09/2011", "Féminin");
        using var fichier = ms.Terminer();

        using var context = TestDatabaseFixture.CreateContext();
        var resultat = await CreerService(context).ImporterAsync(
            fichier, new ImportOptions(false, "duplique.xlsx"));

        Assert.Contains("déjà présent dans le fichier", resultat.ContenuRapport);
        Assert.Contains("Groupe DP1 — REJETÉ", resultat.ContenuRapport);

        using var verif = TestDatabaseFixture.CreateContext();
        Assert.False(await verif.Fonctionnaires.AnyAsync(f => f.Matricule == "DP1"));
    }

    [Fact]
    public async Task Import_avec_rib_grade_situation_affectation_responsabilite()
    {
        var (_, codeCadre, numero, codeStatut, codeResp, nomTribunal, nomUnite) =
            await SeedReferentielsAsync();

        var ms = new ClasseurTest();
        ms.Fonctionnaire("FP1", "FPC1", "ب", "أ", "N", "P", "01/01/1980", "01/09/2010", "Masculin");
        ms.Rib("FP1", "123", "456", "1234567890123456", "10");
        ms.Grade("FP1", codeCadre, numero.ToString(), "01/09/2010", "");
        ms.Situation("FP1", codeStatut, "01/09/2010", "");
        ms.Affectation("FP1", nomTribunal, "", nomUnite, "Parquet", "01/09/2012", "31/12/2015");
        ms.Responsabilite("FP1", codeResp, nomTribunal, "", "01/01/2016", "");
        using var fichier = ms.Terminer();

        using var context = TestDatabaseFixture.CreateContext();
        var resultat = await CreerService(context).ImporterAsync(
            fichier, new ImportOptions(false, "complet.xlsx"));

        Assert.Equal(0, resultat.NombreErreurs);
        Assert.Contains("Groupe FP1 — SUCCÈS", resultat.ContenuRapport);

        using var verif = TestDatabaseFixture.CreateContext();
        var fonc = await verif.Fonctionnaires.AsNoTracking()
            .FirstAsync(f => f.Matricule == "FP1");

        Assert.Equal(1, await verif.Ribs.CountAsync(r => r.IdFonctionnaire == fonc.Id && r.EstActif));
        Assert.Equal(1, await verif.HistoriqueCadreGrades.CountAsync(h => h.IdFonctionnaire == fonc.Id));
        Assert.Equal(1, await verif.SituationAdministratives.CountAsync(s => s.IdFonctionnaire == fonc.Id));

        var affectation = await verif.HistoriqueAffectations.AsNoTracking()
            .FirstAsync(h => h.IdFonctionnaire == fonc.Id);
        Assert.Equal(SectionTribunal.Parquet, affectation.SectionTribunal);
        Assert.NotNull(affectation.IdTribunalAffectation);
        Assert.NotNull(affectation.IdUniteTravail);

        var responsabilite = await verif.HistoriqueResponsabilites.AsNoTracking()
            .FirstAsync(h => h.IdFonctionnaire == fonc.Id);
        Assert.NotNull(responsabilite.IdTribunal);
        Assert.Null(responsabilite.IdUniteOrganisationnelle);
    }

    [Fact]
    public async Task Import_feuille_manquante_rejete()
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Fonctionnaires");
        ws.Cell(1, 1).Value = "Matricule";
        var ms = new MemoryStream();
        workbook.SaveAs(ms);
        ms.Position = 0;
        using var fichier = ms;

        using var context = TestDatabaseFixture.CreateContext();
        var ex = await Assert.ThrowsAsync<RegleMetierException>(() =>
            CreerService(context).ImporterAsync(fichier, new ImportOptions(false, "incomplet.xlsx")));

        Assert.Contains("كل الأوراق المطلوبة", ex.Message);
    }

    private sealed class UtilisateurTest : ICurrentUser
    {
        public UtilisateurTest(string userName) => UserName = userName;

        public string UserName { get; }
    }

    private sealed class ClasseurTest
    {
        private readonly XLWorkbook _workbook = new();

        public ClasseurTest()
        {
            var feuilles = new[]
            {
                (ExcelImportReader.FeuilleFonctionnaires, new[] { "Matricule", "CIN", "Nom arabe", "Prénom arabe", "Nom français", "Prénom français", "Date de naissance", "Date de recrutement", "Genre" }),
                (ExcelImportReader.FeuilleRib, new[] { "Matricule", "Code banque", "Code ville", "Numéro de compte", "Clé" }),
                (ExcelImportReader.FeuilleGrades, new[] { "Matricule", "Code cadre", "Numéro de référence", "Date de début", "Date de fin" }),
                (ExcelImportReader.FeuilleSituations, new[] { "Matricule", "Code statut", "Date de début", "Date de fin" }),
                (ExcelImportReader.FeuilleAffectations, new[] { "Matricule", "Nom tribunal", "Nom structure", "Nom structure de travail", "Section", "Date de début", "Date de fin" }),
                (ExcelImportReader.FeuilleResponsabilites, new[] { "Matricule", "Code responsabilité", "Nom tribunal", "Nom structure", "Date de début", "Date de fin" })
            };

            foreach (var (nom, entetes) in feuilles)
            {
                var ws = _workbook.Worksheets.Add(nom);
                for (var i = 0; i < entetes.Length; i++)
                    ws.Cell(1, i + 1).Value = entetes[i];
            }
        }

        public void Fonctionnaire(
            string matricule, string cin, string nomAr, string prenomAr,
            string nomFr, string prenomFr, string dateNaissance, string dateRecrutement, string genre) =>
            Ajouter(ExcelImportReader.FeuilleFonctionnaires, matricule, cin, nomAr, prenomAr,
                nomFr, prenomFr, dateNaissance, dateRecrutement, genre);

        public void Rib(string matricule, string codeBanque, string codeVille, string numeroCompte, string cle) =>
            Ajouter(ExcelImportReader.FeuilleRib, matricule, codeBanque, codeVille, numeroCompte, cle);

        public void Grade(string matricule, string codeCadre, string numero, string dateDebut, string dateFin) =>
            Ajouter(ExcelImportReader.FeuilleGrades, matricule, codeCadre, numero, dateDebut, dateFin);

        public void Situation(string matricule, string codeStatut, string dateDebut, string dateFin) =>
            Ajouter(ExcelImportReader.FeuilleSituations, matricule, codeStatut, dateDebut, dateFin);

        public void Affectation(
            string matricule, string nomTribunal, string nomStructure,
            string nomStructureTravail, string section, string dateDebut, string dateFin) =>
            Ajouter(ExcelImportReader.FeuilleAffectations, matricule, nomTribunal, nomStructure,
                nomStructureTravail, section, dateDebut, dateFin);

        public void Responsabilite(
            string matricule, string codeResponsabilite, string nomTribunal,
            string nomStructure, string dateDebut, string dateFin) =>
            Ajouter(ExcelImportReader.FeuilleResponsabilites, matricule, codeResponsabilite,
                nomTribunal, nomStructure, dateDebut, dateFin);

        public byte[] SauverOctets()
        {
            using var ms = new MemoryStream();
            _workbook.SaveAs(ms);
            return ms.ToArray();
        }

        public MemoryStream Terminer()
        {
            var ms = new MemoryStream();
            _workbook.SaveAs(ms);
            ms.Position = 0;
            _workbook.Dispose();
            return ms;
        }

        private void Ajouter(string feuille, params string[] valeurs)
        {
            var ws = _workbook.Worksheet(feuille);
            var numeroLigne = (ws.LastRowUsed()?.RowNumber() ?? 1) + 1;

            for (var i = 0; i < valeurs.Length; i++)
            {
                if (!string.IsNullOrEmpty(valeurs[i]))
                    ws.Cell(numeroLigne, i + 1).Value = valeurs[i];
            }
        }
    }
}
