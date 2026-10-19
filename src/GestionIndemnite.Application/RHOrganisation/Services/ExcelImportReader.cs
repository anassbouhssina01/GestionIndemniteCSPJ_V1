using System.Globalization;
using ClosedXML.Excel;
using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Domain.RHOrganisation.Enums;
using GestionIndemnite.Domain.RHOrganisation.Exceptions;

namespace GestionIndemnite.Application.RHOrganisation.Services;

public static class ExcelImportReader
{
    public const int TailleMaxOctets = 5 * 1024 * 1024;
    public const int LignesMax = 5000;
    public const string FormatDate = "dd/MM/yyyy";

    public const string FeuilleFonctionnaires = "Fonctionnaires";
    public const string FeuilleRib = "RIB";
    public const string FeuilleGrades = "Grades";
    public const string FeuilleSituations = "Situations";
    public const string FeuilleAffectations = "Affectations";
    public const string FeuilleResponsabilites = "Responsabilités";

    private static readonly string[] FeuillesRequises =
    {
        FeuilleFonctionnaires, FeuilleRib, FeuilleGrades,
        FeuilleSituations, FeuilleAffectations, FeuilleResponsabilites
    };

    public static async Task<ImportWorkbook> ReadAsync(Stream stream, CancellationToken ct = default)
    {
        using var tampon = new MemoryStream();
        var octets = new byte[81920];
        long totalOctets = 0;
        int lu;

        while ((lu = await stream.ReadAsync(octets, ct)) > 0)
        {
            totalOctets += lu;
            if (totalOctets > TailleMaxOctets)
                throw new RegleMetierException("حجم الملف يتجاوز 5 ميغا");
            tampon.Write(octets, 0, lu);
        }

        tampon.Position = 0;

        XLWorkbook workbook;
        try
        {
            workbook = new XLWorkbook(tampon);
        }
        catch
        {
            throw new RegleMetierException("الملف ليس ملف Excel صالح");
        }

        using (workbook)
        {
            foreach (var nomFeuille in FeuillesRequises)
            {
                if (!workbook.Worksheets.TryGetWorksheet(nomFeuille, out _))
                    throw new RegleMetierException("الملف يجب أن يحتوي على كل الأوراق المطلوبة");
            }

            var erreurs = new List<ErreurLigne>();
            var fonctionnaires = new List<LigneFonctionnaire>();
            var ribs = new List<LigneRib>();
            var grades = new List<LigneGrade>();
            var situations = new List<LigneSituation>();
            var affectations = new List<LigneAffectation>();
            var responsabilites = new List<LigneResponsabilite>();

            ParseFonctionnaires(workbook.Worksheet(FeuilleFonctionnaires), fonctionnaires, erreurs);
            ParseRibs(workbook.Worksheet(FeuilleRib), ribs, erreurs);
            ParseGrades(workbook.Worksheet(FeuilleGrades), grades, erreurs);
            ParseSituations(workbook.Worksheet(FeuilleSituations), situations, erreurs);
            ParseAffectations(workbook.Worksheet(FeuilleAffectations), affectations, erreurs);
            ParseResponsabilites(workbook.Worksheet(FeuilleResponsabilites), responsabilites, erreurs);

            var nombreLignes = fonctionnaires.Count + ribs.Count + grades.Count
                + situations.Count + affectations.Count + responsabilites.Count + erreurs.Count;

            if (nombreLignes > LignesMax)
                throw new RegleMetierException("عدد الأسطر يتجاوز 5000");

            return new ImportWorkbook(
                nombreLignes,
                fonctionnaires,
                ribs,
                grades,
                situations,
                affectations,
                responsabilites,
                erreurs);
        }
    }

    private static void ParseFonctionnaires(
        IXLWorksheet ws,
        List<LigneFonctionnaire> lignes,
        List<ErreurLigne> erreurs)
    {
        const string feuille = FeuilleFonctionnaires;

        foreach (var row in DataRows(ws, 9))
        {
            var ligne = row.RowNumber();
            var avant = erreurs.Count;

            var matricule = RequireText(row.Cell(1), feuille, ligne, "Matricule", erreurs);
            var cin = RequireText(row.Cell(2), feuille, ligne, "CIN", erreurs);
            var nomAr = RequireText(row.Cell(3), feuille, ligne, "Nom arabe", erreurs);
            var prenomAr = RequireText(row.Cell(4), feuille, ligne, "Prénom arabe", erreurs);
            var nomFr = RequireText(row.Cell(5), feuille, ligne, "Nom français", erreurs);
            var prenomFr = RequireText(row.Cell(6), feuille, ligne, "Prénom français", erreurs);
            var dateNaissance = ParseDateCell(row.Cell(7), true, feuille, ligne, "Date de naissance", erreurs);
            var dateRecrutement = ParseDateCell(row.Cell(8), true, feuille, ligne, "Date de recrutement", erreurs);
            var genre = ParseGenre(row.Cell(9), ligne, erreurs);

            if (erreurs.Count == avant)
            {
                lignes.Add(new LigneFonctionnaire(
                    ligne, matricule, cin, nomAr, prenomAr, nomFr, prenomFr,
                    dateNaissance!.Value, dateRecrutement!.Value, genre!.Value));
            }
        }
    }

    private static void ParseRibs(
        IXLWorksheet ws,
        List<LigneRib> lignes,
        List<ErreurLigne> erreurs)
    {
        const string feuille = FeuilleRib;

        foreach (var row in DataRows(ws, 5))
        {
            var ligne = row.RowNumber();
            var avant = erreurs.Count;

            var matricule = RequireText(row.Cell(1), feuille, ligne, "Matricule", erreurs);
            var codeBanque = RequireText(row.Cell(2), feuille, ligne, "Code banque", erreurs);
            var codeVille = RequireText(row.Cell(3), feuille, ligne, "Code ville", erreurs);
            var numeroCompte = RequireText(row.Cell(4), feuille, ligne, "Numéro de compte", erreurs);
            var cle = RequireText(row.Cell(5), feuille, ligne, "Clé", erreurs);

            if (erreurs.Count == avant)
                lignes.Add(new LigneRib(ligne, matricule, codeBanque, codeVille, numeroCompte, cle));
        }
    }

    private static void ParseGrades(
        IXLWorksheet ws,
        List<LigneGrade> lignes,
        List<ErreurLigne> erreurs)
    {
        const string feuille = FeuilleGrades;

        foreach (var row in DataRows(ws, 5))
        {
            var ligne = row.RowNumber();
            var avant = erreurs.Count;

            var matricule = RequireText(row.Cell(1), feuille, ligne, "Matricule", erreurs);
            var codeCadre = RequireText(row.Cell(2), feuille, ligne, "Code cadre", erreurs);
            var numero = ParseIntCell(row.Cell(3), feuille, ligne, "Numéro de référence", erreurs);
            var dateDebut = ParseDateCell(row.Cell(4), true, feuille, ligne, "Date de début", erreurs);
            var dateFin = ParseDateCell(row.Cell(5), false, feuille, ligne, "Date de fin", erreurs);

            if (erreurs.Count == avant)
                lignes.Add(new LigneGrade(ligne, matricule, codeCadre, numero!.Value, dateDebut!.Value, dateFin));
        }
    }

    private static void ParseSituations(
        IXLWorksheet ws,
        List<LigneSituation> lignes,
        List<ErreurLigne> erreurs)
    {
        const string feuille = FeuilleSituations;

        foreach (var row in DataRows(ws, 4))
        {
            var ligne = row.RowNumber();
            var avant = erreurs.Count;

            var matricule = RequireText(row.Cell(1), feuille, ligne, "Matricule", erreurs);
            var codeStatut = RequireText(row.Cell(2), feuille, ligne, "Code statut", erreurs);
            var dateDebut = ParseDateCell(row.Cell(3), true, feuille, ligne, "Date de début", erreurs);
            var dateFin = ParseDateCell(row.Cell(4), false, feuille, ligne, "Date de fin", erreurs);

            if (erreurs.Count == avant)
                lignes.Add(new LigneSituation(ligne, matricule, codeStatut, dateDebut!.Value, dateFin));
        }
    }

    private static void ParseAffectations(
        IXLWorksheet ws,
        List<LigneAffectation> lignes,
        List<ErreurLigne> erreurs)
    {
        const string feuille = FeuilleAffectations;

        foreach (var row in DataRows(ws, 7))
        {
            var ligne = row.RowNumber();
            var avant = erreurs.Count;

            var matricule = RequireText(row.Cell(1), feuille, ligne, "Matricule", erreurs);
            var nomTribunal = OptionalText(row.Cell(2));
            var nomStructure = OptionalText(row.Cell(3));
            var nomStructureTravail = OptionalText(row.Cell(4));
            var section = ParseSection(row.Cell(5), ligne, erreurs);
            var dateDebut = ParseDateCell(row.Cell(6), true, feuille, ligne, "Date de début", erreurs);
            var dateFin = ParseDateCell(row.Cell(7), false, feuille, ligne, "Date de fin", erreurs);

            if (erreurs.Count == avant)
            {
                lignes.Add(new LigneAffectation(
                    ligne, matricule, nomTribunal, nomStructure, nomStructureTravail,
                    section, dateDebut!.Value, dateFin));
            }
        }
    }

    private static void ParseResponsabilites(
        IXLWorksheet ws,
        List<LigneResponsabilite> lignes,
        List<ErreurLigne> erreurs)
    {
        const string feuille = FeuilleResponsabilites;

        foreach (var row in DataRows(ws, 6))
        {
            var ligne = row.RowNumber();
            var avant = erreurs.Count;

            var matricule = RequireText(row.Cell(1), feuille, ligne, "Matricule", erreurs);
            var codeResponsabilite = RequireText(row.Cell(2), feuille, ligne, "Code responsabilité", erreurs);
            var nomTribunal = OptionalText(row.Cell(3));
            var nomStructure = OptionalText(row.Cell(4));
            var dateDebut = ParseDateCell(row.Cell(5), true, feuille, ligne, "Date de début", erreurs);
            var dateFin = ParseDateCell(row.Cell(6), false, feuille, ligne, "Date de fin", erreurs);

            if (erreurs.Count == avant)
            {
                lignes.Add(new LigneResponsabilite(
                    ligne, matricule, codeResponsabilite, nomTribunal, nomStructure,
                    dateDebut!.Value, dateFin));
            }
        }
    }

    private static IEnumerable<IXLRow> DataRows(IXLWorksheet ws, int nbColonnes)
    {
        var lastRow = ws.LastRowUsed();
        if (lastRow is null)
            yield break;

        for (var r = 2; r <= lastRow.RowNumber(); r++)
        {
            var row = ws.Row(r);
            if (!IsRowBlank(row, nbColonnes))
                yield return row;
        }
    }

    private static bool IsRowBlank(IXLRow row, int nbColonnes)
    {
        for (var c = 1; c <= nbColonnes; c++)
        {
            if (!row.Cell(c).IsEmpty())
                return false;
        }

        return true;
    }

    private static string CellText(IXLCell cell)
    {
        return cell.DataType switch
        {
            XLDataType.Blank => string.Empty,
            XLDataType.Text => cell.GetString().Trim(),
            XLDataType.Number => FormatNumber(cell.GetDouble()),
            XLDataType.DateTime => cell.GetDateTime().ToString(FormatDate, CultureInfo.InvariantCulture),
            XLDataType.Boolean => cell.GetBoolean() ? "Vrai" : "Faux",
            _ => cell.GetFormattedString().Trim()
        };
    }

    private static string FormatNumber(double valeur)
    {
        return valeur == double.Truncate(valeur) && System.Math.Abs(valeur) < long.MaxValue
            ? ((long)valeur).ToString(CultureInfo.InvariantCulture)
            : valeur.ToString(CultureInfo.InvariantCulture);
    }

    private static string RequireText(
        IXLCell cell, string feuille, int ligne, string colonne, List<ErreurLigne> erreurs)
    {
        var texte = CellText(cell);
        if (string.IsNullOrEmpty(texte))
        {
            erreurs.Add(new ErreurLigne(ligne, feuille, colonne, "champ obligatoire"));
            return string.Empty;
        }

        return texte;
    }

    private static string OptionalText(IXLCell cell) => CellText(cell);

    private static DateOnly? ParseDateCell(
        IXLCell cell, bool requise, string feuille, int ligne, string colonne, List<ErreurLigne> erreurs)
    {
        if (cell.IsEmpty() || cell.DataType == XLDataType.Blank)
        {
            if (requise)
                erreurs.Add(new ErreurLigne(ligne, feuille, colonne, "champ obligatoire"));
            return null;
        }

        if (cell.DataType == XLDataType.DateTime)
            return DateOnly.FromDateTime(cell.GetDateTime());

        if (cell.DataType == XLDataType.Text)
        {
            var texte = cell.GetString().Trim();
            if (string.IsNullOrEmpty(texte))
            {
                if (requise)
                    erreurs.Add(new ErreurLigne(ligne, feuille, colonne, "champ obligatoire"));
                return null;
            }

            if (DateOnly.TryParseExact(
                    texte, FormatDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;
        }

        erreurs.Add(new ErreurLigne(
            ligne, feuille, colonne, "date invalide ou incomplète (format attendu jj/mm/aaaa)"));
        return null;
    }

    private static int? ParseIntCell(
        IXLCell cell, string feuille, int ligne, string colonne, List<ErreurLigne> erreurs)
    {
        if (cell.IsEmpty() || cell.DataType == XLDataType.Blank)
        {
            erreurs.Add(new ErreurLigne(ligne, feuille, colonne, "champ obligatoire"));
            return null;
        }

        if (cell.DataType == XLDataType.Number)
        {
            var valeur = cell.GetDouble();
            if (valeur == double.Truncate(valeur) && valeur is >= int.MinValue and <= int.MaxValue)
                return (int)valeur;

            erreurs.Add(new ErreurLigne(ligne, feuille, colonne, "nombre entier attendu"));
            return null;
        }

        var texte = CellText(cell);
        if (int.TryParse(texte, NumberStyles.Integer, CultureInfo.InvariantCulture, out var entier))
            return entier;

        erreurs.Add(new ErreurLigne(ligne, feuille, colonne, "nombre entier attendu"));
        return null;
    }

    private static Genre? ParseGenre(IXLCell cell, int ligne, List<ErreurLigne> erreurs)
    {
        var texte = CellText(cell);
        if (string.IsNullOrEmpty(texte))
        {
            erreurs.Add(new ErreurLigne(ligne, FeuilleFonctionnaires, "Genre", "champ obligatoire"));
            return null;
        }

        if (texte.Equals("Masculin", StringComparison.OrdinalIgnoreCase)
            || texte.Equals("M", StringComparison.OrdinalIgnoreCase))
            return Genre.Masculin;

        if (texte.Equals("Féminin", StringComparison.OrdinalIgnoreCase)
            || texte.Equals("Feminin", StringComparison.OrdinalIgnoreCase)
            || texte.Equals("F", StringComparison.OrdinalIgnoreCase))
            return Genre.Feminin;

        erreurs.Add(new ErreurLigne(
            ligne, FeuilleFonctionnaires, "Genre", "genre attendu : Masculin ou Féminin"));
        return null;
    }

    private static SectionTribunal? ParseSection(
        IXLCell cell, int ligne, List<ErreurLigne> erreurs)
    {
        var texte = CellText(cell);
        if (string.IsNullOrEmpty(texte))
            return null;

        if (texte.Equals("Parquet", StringComparison.OrdinalIgnoreCase))
            return SectionTribunal.Parquet;

        if (texte.Equals("Présidence", StringComparison.OrdinalIgnoreCase)
            || texte.Equals("Presidence", StringComparison.OrdinalIgnoreCase))
            return SectionTribunal.Presidence;

        erreurs.Add(new ErreurLigne(
            ligne, FeuilleAffectations, "Section", "section attendue : Parquet ou Présidence"));
        return null;
    }
}
