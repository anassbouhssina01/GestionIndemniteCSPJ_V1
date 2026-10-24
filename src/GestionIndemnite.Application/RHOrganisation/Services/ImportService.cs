using System.Data;
using System.Text;
using GestionIndemnite.Application.RHOrganisation.Common;
using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Domain.RHOrganisation.Common;
using GestionIndemnite.Domain.RHOrganisation.Entities;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GestionIndemnite.Application.RHOrganisation.Services;

public class ImportService
{
    private readonly IRHOrganisationDbContext _db;
    private readonly FonctionnaireService _fonctionnaires;
    private readonly RibService _ribs;
    private readonly HistoriqueCadreGradeService _cadreGrades;
    private readonly SituationAdministrativeService _situations;
    private readonly HistoriqueAffectationService _affectations;
    private readonly HistoriqueResponsabiliteService _responsabilites;
    private readonly ICurrentUser _currentUser;

    public ImportService(
        IRHOrganisationDbContext db,
        FonctionnaireService fonctionnaires,
        RibService ribs,
        HistoriqueCadreGradeService cadreGrades,
        SituationAdministrativeService situations,
        HistoriqueAffectationService affectations,
        HistoriqueResponsabiliteService responsabilites,
        ICurrentUser currentUser)
    {
        _db = db;
        _fonctionnaires = fonctionnaires;
        _ribs = ribs;
        _cadreGrades = cadreGrades;
        _situations = situations;
        _affectations = affectations;
        _responsabilites = responsabilites;
        _currentUser = currentUser;
    }

    public async Task<ImportResultatDto> ImporterAsync(
        Stream fichier,
        ImportOptions options,
        CancellationToken ct = default)
    {
        var workbook = await ExcelImportReader.ReadAsync(fichier, ct);

        var corps = new List<string>();
        var nombreErreurs = 0;

        foreach (var erreur in workbook.ErreursExtraction)
        {
            nombreErreurs++;
            corps.Add(FormatErreur(erreur.NumeroLigne, erreur.Feuille, erreur.Colonne, erreur.Motif));
        }

        var groupes = new Dictionary<string, GroupeImport>(StringComparer.Ordinal);
        var ordre = new List<string>();

        foreach (var ligne in workbook.Fonctionnaires)
        {
            if (!groupes.TryGetValue(ligne.Matricule, out var groupe))
            {
                groupe = new GroupeImport();
                groupes.Add(ligne.Matricule, groupe);
                ordre.Add(ligne.Matricule);
            }
            else
            {
                nombreErreurs++;
                corps.Add(FormatErreur(
                    ligne.NumeroLigne,
                    ExcelImportReader.FeuilleFonctionnaires,
                    "Matricule",
                    $"Matricule «{ligne.Matricule}» déjà présent dans le fichier"));
                groupe.EnErreur = true;
            }

            groupe.LigneFonctionnaire ??= ligne;
        }

        foreach (var ligne in workbook.Ribs)
            GetGroupe(groupes, ordre, ligne.Matricule).Ribs.Add(ligne);
        foreach (var ligne in workbook.Grades)
            GetGroupe(groupes, ordre, ligne.Matricule).Grades.Add(ligne);
        foreach (var ligne in workbook.Situations)
            GetGroupe(groupes, ordre, ligne.Matricule).Situations.Add(ligne);
        foreach (var ligne in workbook.Affectations)
            GetGroupe(groupes, ordre, ligne.Matricule).Affectations.Add(ligne);
        foreach (var ligne in workbook.Responsabilites)
            GetGroupe(groupes, ordre, ligne.Matricule).Responsabilites.Add(ligne);

        foreach (var matricule in ordre)
        {
            var groupe = groupes[matricule];

            if (groupe.EnErreur)
            {
                corps.Add($"Groupe {matricule} — REJETÉ — erreurs de saisie dans le fichier");
                continue;
            }

            var existeEnBase = await _db.Fonctionnaires
                .AnyAsync(f => f.Matricule == matricule, ct);

            if (groupe.LigneFonctionnaire is null)
            {
                if (!existeEnBase)
                {
                    nombreErreurs++;
                    corps.Add(
                        $"Groupe {matricule} — REJETÉ — Matricule introuvable dans le fichier et dans la base");
                }
                else
                {
                    corps.Add($"Groupe {matricule} — IGNORÉ — fonctionnaire déjà existant");
                }

                continue;
            }

            if (existeEnBase)
            {
                corps.Add($"Groupe {matricule} — IGNORÉ — fonctionnaire déjà importé");
                continue;
            }

            var resolution = await ResolveGroupAsync(groupe, matricule, ct);

            if (resolution.Erreurs.Count > 0)
            {
                corps.AddRange(resolution.Erreurs);
                nombreErreurs += resolution.Erreurs.Count;
                corps.Add($"Groupe {matricule} — REJETÉ — erreurs de saisie dans le fichier");
                continue;
            }

            var resultatGroupe = await ExecuterGroupeAsync(
                groupe, resolution, options.ModeValidationSeule, ct);

            corps.Add($"Groupe {matricule} — {resultatGroupe.Statut} — {resultatGroupe.Motif}");
            if (resultatGroupe.Rejete)
                nombreErreurs++;
        }

        var contenu = ConstruireContenu(
            options.NomFichier,
            options.ModeValidationSeule,
            _currentUser.UserName,
            workbook.NombreLignes,
            nombreErreurs,
            corps);

        var rapport = new RapportImport
        {
            Id = Guid.NewGuid(),
            NomFichier = string.IsNullOrWhiteSpace(options.NomFichier)
                ? "fichier.xlsx"
                : options.NomFichier.Trim(),
            ModeDryRun = options.ModeValidationSeule,
            NombreLignes = workbook.NombreLignes,
            NombreErreurs = nombreErreurs,
            ContenuRapport = contenu
        };

        _db.RapportImports.Add(rapport);
        await _db.SaveChangesAsync(ct);
        _db.ClearTrackedChanges();

        return new ImportResultatDto(
            rapport.Id,
            rapport.NomFichier,
            rapport.ModeDryRun,
            rapport.NombreLignes,
            rapport.NombreErreurs,
            rapport.ContenuRapport);
    }

    private async Task<ResolutionGroupe> ResolveGroupAsync(
        GroupeImport groupe,
        string matricule,
        CancellationToken ct)
    {
        var resolution = new ResolutionGroupe();
        var erreurs = resolution.Erreurs;
        var fonctionnaire = groupe.LigneFonctionnaire;

        if (fonctionnaire is null)
            return resolution;

        foreach (var ligne in groupe.Grades)
        {
            var cadreId = await _db.Cadres
                .Where(c => c.CodeCadre == ligne.CodeCadre)
                .Select(c => c.Id)
                .FirstOrDefaultAsync(ct);

            if (cadreId == Guid.Empty)
            {
                erreurs.Add(FormatErreur(
                    ligne.NumeroLigne,
                    ExcelImportReader.FeuilleGrades,
                    "Code cadre",
                    $"cadre «{ligne.CodeCadre}» introuvable"));
                continue;
            }

            var gradeId = await _db.Grades
                .Where(g => g.IdCadre == cadreId && g.NumeroReferencie == ligne.NumeroReferencie)
                .Select(g => g.Id)
                .FirstOrDefaultAsync(ct);

            if (gradeId == Guid.Empty)
            {
                erreurs.Add(FormatErreur(
                    ligne.NumeroLigne,
                    ExcelImportReader.FeuilleGrades,
                    "Numéro de référence",
                    $"grade «{ligne.NumeroReferencie}» introuvable pour le cadre «{ligne.CodeCadre}»"));
                continue;
            }

            resolution.Grades.Add((ligne, gradeId));
        }

        foreach (var ligne in groupe.Situations)
        {
            var statutId = await _db.StatutAdministratifs
                .Where(s => s.CodeStatut == ligne.CodeStatut)
                .Select(s => s.Id)
                .FirstOrDefaultAsync(ct);

            if (statutId == Guid.Empty)
            {
                erreurs.Add(FormatErreur(
                    ligne.NumeroLigne,
                    ExcelImportReader.FeuilleSituations,
                    "Code statut",
                    $"statut «{ligne.CodeStatut}» introuvable"));
                continue;
            }

            resolution.Situations.Add((ligne, statutId));
        }

        foreach (var ligne in groupe.Affectations)
        {
            var (idTribunal, idUnite, idUniteTravail) =
                await ResolveLieuAsync(ligne, ExcelImportReader.FeuilleAffectations, erreurs, ct);

            if (ligne.Section.HasValue && string.IsNullOrEmpty(ligne.NomTribunal))
            {
                erreurs.Add(FormatErreur(
                    ligne.NumeroLigne,
                    ExcelImportReader.FeuilleAffectations,
                    "Section",
                    "section réservée aux affectations de tribunal"));
                continue;
            }

            if (erreurs.Any(e => e.StartsWith($"Ligne {ligne.NumeroLigne} [")))
                continue;

            resolution.Affectations.Add((ligne, idTribunal, idUnite, idUniteTravail));
        }

        foreach (var ligne in groupe.Responsabilites)
        {
            var responsabiliteId = await _db.Responsabilites
                .Where(r => r.CodeResponsabilite == ligne.CodeResponsabilite)
                .Select(r => r.Id)
                .FirstOrDefaultAsync(ct);

            if (responsabiliteId == Guid.Empty)
            {
                erreurs.Add(FormatErreur(
                    ligne.NumeroLigne,
                    ExcelImportReader.FeuilleResponsabilites,
                    "Code responsabilité",
                    $"responsabilité «{ligne.CodeResponsabilite}» introuvable"));
                continue;
            }

            var (idTribunal, idUnite, _) =
                await ResolveLieuAsync(
                    new LigneAffectation(
                        ligne.NumeroLigne, ligne.Matricule, ligne.NomTribunal, ligne.NomStructure,
                        string.Empty, null, ligne.DateDebut, ligne.DateFin),
                    ExcelImportReader.FeuilleResponsabilites,
                    erreurs,
                    ct);

            if (erreurs.Any(e => e.StartsWith($"Ligne {ligne.NumeroLigne} [")))
                continue;

            resolution.Responsabilites.Add((ligne, responsabiliteId, idTribunal, idUnite));
        }

        return resolution;
    }

    private async Task<(Guid? IdTribunal, Guid? IdUnite, Guid? IdUniteTravail)> ResolveLieuAsync(
        LigneAffectation ligne,
        string feuille,
        List<string> erreurs,
        CancellationToken ct)
    {
        var numeroLigne = ligne.NumeroLigne;
        var aTribunal = !string.IsNullOrEmpty(ligne.NomTribunal);
        var aStructure = !string.IsNullOrEmpty(ligne.NomStructure);

        if (aTribunal && aStructure)
        {
            erreurs.Add(FormatErreur(
                numeroLigne, feuille, "Nom tribunal",
                "remplir uniquement le tribunal ou la structure"));
            return (null, null, null);
        }

        if (!aTribunal && !aStructure)
        {
            erreurs.Add(FormatErreur(
                numeroLigne, feuille, "Nom tribunal", "champ obligatoire"));
            return (null, null, null);
        }

        if (!string.IsNullOrEmpty(ligne.NomStructureTravail) && !aTribunal)
        {
            erreurs.Add(FormatErreur(
                numeroLigne, feuille, "Nom structure de travail",
                "réservé aux affectations de tribunal"));
            return (null, null, null);
        }

        Guid? idTribunal = null;
        Guid? idUnite = null;
        Guid? idUniteTravail = null;

        if (aTribunal)
        {
            var candidats = await _db.Tribunaux
                .Where(t => t.NomTribunal == ligne.NomTribunal)
                .Select(t => t.Id)
                .ToListAsync(ct);

            if (candidats.Count == 0)
            {
                erreurs.Add(FormatErreur(
                    numeroLigne, feuille, "Nom tribunal",
                    $"tribunal «{ligne.NomTribunal}» introuvable"));
            }
            else if (candidats.Count > 1)
            {
                erreurs.Add(FormatErreur(
                    numeroLigne, feuille, "Nom tribunal",
                    $"nom de tribunal ambigu «{ligne.NomTribunal}»"));
            }
            else
            {
                idTribunal = candidats[0];
            }
        }
        else
        {
            idUnite = await ResolveUniteIdAsync(ligne.NomStructure, feuille, numeroLigne, "Nom structure", erreurs, ct);
        }

        if (!string.IsNullOrEmpty(ligne.NomStructureTravail))
        {
            idUniteTravail = await ResolveUniteIdAsync(
                ligne.NomStructureTravail, feuille, numeroLigne, "Nom structure de travail", erreurs, ct);
        }

        return (idTribunal, idUnite, idUniteTravail);
    }

    private async Task<Guid?> ResolveUniteIdAsync(
        string nom,
        string feuille,
        int numeroLigne,
        string colonne,
        List<string> erreurs,
        CancellationToken ct)
    {
        var candidats = await _db.UnitesOrganisationnelles
            .Where(u => u.NomUnite == nom)
            .Select(u => u.Id)
            .ToListAsync(ct);

        if (candidats.Count == 0)
        {
            erreurs.Add(FormatErreur(
                numeroLigne, feuille, colonne, $"structure «{nom}» introuvable"));
            return null;
        }

        if (candidats.Count > 1)
        {
            erreurs.Add(FormatErreur(
                numeroLigne, feuille, colonne, $"nom de structure ambigu «{nom}»"));
            return null;
        }

        return candidats[0];
    }

    private async Task<ResultatGroupe> ExecuterGroupeAsync(
        GroupeImport groupe,
        ResolutionGroupe resolution,
        bool modeValidationSeule,
        CancellationToken ct)
    {
        IDbContextTransaction? transaction = null;

        try
        {
            transaction = await _db.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

            var fonctionnaire = await _fonctionnaires.CreateAsync(
                new CreateFonctionnaireCommand(
                    groupe.LigneFonctionnaire!.Matricule,
                    groupe.LigneFonctionnaire!.Cin,
                    groupe.LigneFonctionnaire!.NomAr,
                    groupe.LigneFonctionnaire!.PrenomAr,
                    groupe.LigneFonctionnaire!.NomFr,
                    groupe.LigneFonctionnaire!.PrenomFr,
                    groupe.LigneFonctionnaire!.DateNaissance,
                    groupe.LigneFonctionnaire!.DateRecrutement,
                    groupe.LigneFonctionnaire!.Genre),
                ct);

            foreach (var ligne in groupe.Ribs.OrderBy(r => r.NumeroLigne))
            {
                await _ribs.ChangeActiveAsync(
                    fonctionnaire.Id,
                    new CreateRibCommand(
                        ligne.CodeBanque, ligne.CodeVille, ligne.NumeroCompte, ligne.Cle),
                    ct);
            }

            foreach (var (ligne, idGrade) in resolution.Grades
                         .OrderBy(r => r.Ligne.DateDebut)
                         .ThenBy(r => r.Ligne.NumeroLigne))
            {
                await _cadreGrades.AddAsync(
                    new CreateHistoriqueCadreGradeCommand(
                        fonctionnaire.Id, idGrade, ligne.DateDebut, ligne.DateFin, null),
                    ct);
            }

            foreach (var (ligne, idStatut) in resolution.Situations
                         .OrderBy(r => r.Ligne.DateDebut)
                         .ThenBy(r => r.Ligne.NumeroLigne))
            {
                await _situations.AddAsync(
                    new CreateSituationAdministrativeCommand(
                        fonctionnaire.Id, idStatut, null, ligne.DateDebut, ligne.DateFin, null),
                    ct);
            }

            foreach (var (ligne, idTribunal, idUnite, idUniteTravail) in resolution.Affectations
                         .OrderBy(r => r.Ligne.DateDebut)
                         .ThenBy(r => r.Ligne.NumeroLigne))
            {
                await _affectations.AddAsync(
                    new CreateHistoriqueAffectationCommand(
                        fonctionnaire.Id, idTribunal, idUnite, idUniteTravail,
                        ligne.Section, ligne.DateDebut, ligne.DateFin, null),
                    ct);
            }

            foreach (var (ligne, idResponsabilite, idTribunal, idUnite) in resolution.Responsabilites
                         .OrderBy(r => r.Ligne.DateDebut)
                         .ThenBy(r => r.Ligne.NumeroLigne))
            {
                await _responsabilites.AddAsync(
                    new CreateHistoriqueResponsabiliteCommand(
                        fonctionnaire.Id, idResponsabilite, idTribunal, idUnite,
                        ligne.DateDebut, ligne.DateFin, null),
                    ct);
            }

            if (modeValidationSeule)
                await transaction.RollbackAsync(ct);
            else
                await transaction.CommitAsync(ct);

            _db.ClearTrackedChanges();

            return new ResultatGroupe("SUCCÈS", modeValidationSeule ? "validation seule" : "enregistré", false);
        }
        catch (Exception ex) when (ex is RegleMetierException or DbUpdateException)
        {
            if (transaction is not null)
                await transaction.RollbackAsync(ct);

            _db.ClearTrackedChanges();

            var motif = ex is RegleMetierException
                ? ex.Message
                : ex.InnerException?.Message ?? ex.Message;

            return new ResultatGroupe("REJETÉ", motif, true);
        }
        finally
        {
            if (transaction is not null)
                await transaction.DisposeAsync();
        }
    }

    private static GroupeImport GetGroupe(
        Dictionary<string, GroupeImport> groupes,
        List<string> ordre,
        string matricule)
    {
        if (!groupes.TryGetValue(matricule, out var groupe))
        {
            groupe = new GroupeImport();
            groupes.Add(matricule, groupe);
            ordre.Add(matricule);
        }

        return groupe;
    }

    private static string FormatErreur(int numeroLigne, string feuille, string colonne, string motif) =>
        $"Ligne {numeroLigne} [{feuille}] — ERREUR — {colonne} : {motif}";

    private static string ConstruireContenu(
        string nomFichier,
        bool modeValidationSeule,
        string utilisateur,
        int nombreLignes,
        int nombreErreurs,
        List<string> corps)
    {
        var sb = new StringBuilder();
        var mode = modeValidationSeule
            ? "validation seule (rien n'est enregistré)"
            : "enregistrement";

        sb.AppendLine($"Rapport d'import — {nomFichier} — mode : {mode}");
        sb.AppendLine($"Importé par : {utilisateur} — le {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss} UTC");
        sb.AppendLine();
        sb.AppendLine($"Lignes traitées : {nombreLignes} — erreurs : {nombreErreurs}");
        sb.AppendLine();

        foreach (var ligne in corps)
            sb.AppendLine(ligne);

        if (corps.Count == 0)
            sb.AppendLine("Aucune ligne à traiter.");

        return sb.ToString();
    }

    private sealed class GroupeImport
    {
        public LigneFonctionnaire? LigneFonctionnaire { get; set; }
        public bool EnErreur { get; set; }
        public List<LigneRib> Ribs { get; } = new();
        public List<LigneGrade> Grades { get; } = new();
        public List<LigneSituation> Situations { get; } = new();
        public List<LigneAffectation> Affectations { get; } = new();
        public List<LigneResponsabilite> Responsabilites { get; } = new();
    }

    private sealed class ResolutionGroupe
    {
        public List<string> Erreurs { get; } = new();
        public List<(LigneGrade Ligne, Guid IdGrade)> Grades { get; } = new();
        public List<(LigneSituation Ligne, Guid IdStatut)> Situations { get; } = new();
        public List<(LigneAffectation Ligne, Guid? IdTribunal, Guid? IdUnite, Guid? IdUniteTravail)> Affectations { get; } = new();
        public List<(LigneResponsabilite Ligne, Guid IdResponsabilite, Guid? IdTribunal, Guid? IdUnite)> Responsabilites { get; } = new();
    }

    private sealed record ResultatGroupe(string Statut, string Motif, bool Rejete);
}
