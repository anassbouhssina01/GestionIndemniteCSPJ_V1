using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Application.RHOrganisation.Services;
using GestionIndemnite.Application.RHOrganisation.Services.Referentiels;
using GestionIndemnite.Domain.RHOrganisation.Enums;
using GestionIndemnite.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestionIndemnite.Web.Controllers;

public class FonctionnaireController : Controller
{
    private readonly FonctionnaireService _service;
    private readonly RibService _ribs;
    private readonly HistoriqueCadreGradeService _gradePeriods;
    private readonly SituationAdministrativeService _situationPeriods;
    private readonly HistoriqueAffectationService _affectationPeriods;
    private readonly HistoriqueResponsabiliteService _responsabilitePeriods;
    private readonly GradeService _grades;
    private readonly StatutAdministratifService _statuts;
    private readonly TribunalService _tribunaux;
    private readonly UniteOrganisationnelleService _unites;
    private readonly ResponsabiliteService _responsabilites;
    private readonly PieceJointeService _pieces;
    private readonly CadreService _cadres;
    private readonly VilleService _villes;

    public FonctionnaireController(
        FonctionnaireService service,
        RibService ribs,
        HistoriqueCadreGradeService gradePeriods,
        SituationAdministrativeService situationPeriods,
        HistoriqueAffectationService affectationPeriods,
        HistoriqueResponsabiliteService responsabilitePeriods,
        GradeService grades,
        StatutAdministratifService statuts,
        TribunalService tribunaux,
        UniteOrganisationnelleService unites,
        ResponsabiliteService responsabilites,
        PieceJointeService pieces,
        CadreService cadres,
        VilleService villes)
    {
        _service = service;
        _ribs = ribs;
        _gradePeriods = gradePeriods;
        _situationPeriods = situationPeriods;
        _affectationPeriods = affectationPeriods;
        _responsabilitePeriods = responsabilitePeriods;
        _grades = grades;
        _statuts = statuts;
        _tribunaux = tribunaux;
        _unites = unites;
        _responsabilites = responsabilites;
        _pieces = pieces;
        _cadres = cadres;
        _villes = villes;
    }

    public async Task<IActionResult> Index(
        string? matricule,
        string? cin,
        string? nom,
        CancellationToken ct = default)
    {
        ViewBag.Matricule = matricule;
        ViewBag.Cin = cin;
        ViewBag.Nom = nom;
        return View(await _service.SearchAsync(matricule, cin, nom, ct));
    }

    public IActionResult Create()
    {
        ViewBag.Genres = CreerSelectGenres();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateFonctionnaireCommand command, CancellationToken ct)
    {
        var fonctionnaire = await _service.CreateAsync(command, ct);
        TempData["Succes"] = "Le fonctionnaire a été créé.";
        return RedirectToAction(nameof(Details), new { id = fonctionnaire.Id });
    }

    /// <summary>
    /// Dossier complet : identité, RIB, situation actuelle et tous les historiques.
    /// </summary>
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var fonctionnaire = await _service.GetAsync(id, ct);

        var modele = new DossierFonctionnaireViewModel
        {
            Fonctionnaire = fonctionnaire,
            RibActif = await _ribs.GetActiveAsync(id, ct),
            Ribs = await _ribs.ListHistoryAsync(id, ct),
            GradePeriods = await _gradePeriods.ListByFonctionnaireAsync(id, ct),
            SituationPeriods = await _situationPeriods.ListByFonctionnaireAsync(id, ct),
            AffectationPeriods = await _affectationPeriods.ListByFonctionnaireAsync(id, ct),
            ResponsabilitePeriods = await _responsabilitePeriods.ListByFonctionnaireAsync(id, ct),
            Grades = await _grades.ListAsync(null, true, ct),
            Statuts = await _statuts.ListAsync(true, ct),
            Tribunaux = await _tribunaux.ListAsync(false, ct),
            Unites = await _unites.ListAsync(false, ct),
            Responsabilites = await _responsabilites.ListAsync(false, ct),
            Documents = await _pieces.SearchAsync(null, null, null, ct),
            Cadres = await _cadres.ListAsync(false, ct),
            Villes = await _villes.ListAsync(null, false, ct)
        };

        return View(modele);
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var fonctionnaire = await _service.GetAsync(id, ct);
        ViewBag.Genres = CreerSelectGenres();
        return View(fonctionnaire);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateFonctionnaireCommand command, CancellationToken ct)
    {
        await _service.UpdateAsync(id, command, ct);
        TempData["Succes"] = "Le fonctionnaire a été mis à jour.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public IActionResult AjoutEnMasse()
    {
        ViewBag.Genres = CreerSelectGenres();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AjoutEnMasse(IFormCollection form, CancellationToken ct)
    {
        // Traitement en masse simple - à affiner selon les besoins
        var lignes = form.Keys
            .Where(k => k.StartsWith("Matricule["))
            .Select(k => k.Substring("Matricule[".Length))
            .Select(k => k.Substring(0, k.Length - 1))
            .Distinct()
            .ToList();

        var count = 0;
        foreach (var idx in lignes)
        {
            var matricule = form[$"Matricule[{idx}]"];
            if (string.IsNullOrWhiteSpace(matricule)) continue;
            var cin = form[$"Cin[{idx}]"];
            var nomAr = form[$"NomAr[{idx}]"];
            var prenomAr = form[$"PrenomAr[{idx}]"];
            var nomFr = form[$"NomFr[{idx}]"];
            var prenomFr = form[$"PrenomFr[{idx}]"];
            var dateNaiss = form[$"DateNaissance[{idx}]"];
            var dateRecrut = form[$"DateRecrutement[{idx}]"];
            var genreStr = form[$"Genre[{idx}]"];

            if (string.IsNullOrWhiteSpace(cin) || string.IsNullOrWhiteSpace(nomFr) || string.IsNullOrWhiteSpace(dateNaiss) || string.IsNullOrWhiteSpace(dateRecrut)) continue;

            var cmd = new CreateFonctionnaireCommand(
                matricule.ToString().Trim(),
                cin.ToString().Trim(),
                nomAr.ToString().Trim(),
                prenomAr.ToString().Trim(),
                nomFr.ToString().Trim(),
                prenomFr.ToString().Trim(),
                DateOnly.Parse(dateNaiss.ToString()),
                DateOnly.Parse(dateRecrut.ToString()),
                genreStr.ToString() == "1" ? Genre.Feminin : Genre.Masculin);

            try
            {
                await _service.CreateAsync(cmd, ct);
                count++;
            }
            catch { /* ignore duplicate/invalid rows */ }
        }

        TempData["Succes"] = $"{count} fonctionnaire(s) ajouté(s) en masse.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> MiseAJourEnMasse(CancellationToken ct)
    {
        ViewBag.Genres = CreerSelectGenres();
        return View(await _service.SearchAsync(null, null, null, ct));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangerRib(Guid id, CreateRibCommand command, CancellationToken ct)
    {
        await _ribs.ChangeActiveAsync(id, command, ct);
        TempData["Succes"] = "Le nouveau RIB est devenu le RIB actif.";
        return RedirectToAction(nameof(Details), controllerName: null, routeValues: new { id }, fragment: "tab-rib");
    }

    // ---- Actions du dossier : changement de grade / situation / affectation ----

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangerGrade(
        Guid id,
        Guid idGrade,
        DateOnly dateDebut,
        Guid? idDocument,
        CancellationToken ct)
    {
        await _gradePeriods.ChangeAsync(id, idGrade, dateDebut, idDocument, ct);
        TempData["Succes"] = "Le grade a été changé et l'ancienne période a été clôturée.";
        return RedirectToAction(nameof(Details), controllerName: null, routeValues: new { id }, fragment: "tab-grade");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangerSituation(
        Guid id,
        Guid idStatut,
        DateOnly dateDebut,
        Guid? idDocument,
        CancellationToken ct)
    {
        await _situationPeriods.ChangeAsync(id, idStatut, dateDebut, idDocument, ct);
        TempData["Succes"] = "La situation administrative a été changée.";
        return RedirectToAction(nameof(Details), controllerName: null, routeValues: new { id }, fragment: "tab-situation");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangerAffectation(
        Guid id,
        Guid? idTribunalAffectation,
        Guid? idUniteAffectation,
        Guid? idUniteTravail,
        SectionTribunal? sectionTribunal,
        DateOnly dateDebut,
        Guid? idDocument,
        CancellationToken ct)
    {
        await _affectationPeriods.ChangeAsync(
            id, idTribunalAffectation, idUniteAffectation, idUniteTravail,
            sectionTribunal, dateDebut, idDocument, ct);

        TempData["Succes"] = "L'affectation a été changée et l'ancienne période a été clôturée.";
        return RedirectToAction(nameof(Details), controllerName: null, routeValues: new { id }, fragment: "tab-affectation");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AjouterResponsabilite(
        Guid id,
        Guid idResponsabilite,
        Guid? idTribunal,
        Guid? idUniteOrganisationnelle,
        DateOnly dateDebut,
        DateOnly? dateFin,
        Guid? idDocument,
        CancellationToken ct)
    {
        await _responsabilitePeriods.AddAsync(new CreateHistoriqueResponsabiliteCommand(
            id, idResponsabilite, idTribunal, idUniteOrganisationnelle,
            dateDebut, dateFin, idDocument), ct);

        TempData["Succes"] = "La responsabilité a été attribuée.";
        return RedirectToAction(nameof(Details), controllerName: null, routeValues: new { id }, fragment: "tab-responsabilites");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CloturerResponsabilite(
        Guid id,
        Guid idPeriode,
        DateOnly dateFin,
        CancellationToken ct)
    {
        await _responsabilitePeriods.CloseAsync(idPeriode, dateFin, ct);
        TempData["Succes"] = "La responsabilité a été clôturée.";
        return RedirectToAction(nameof(Details), controllerName: null, routeValues: new { id }, fragment: "tab-responsabilites");
    }

    private static SelectList CreerSelectGenres() =>
        new(new[]
        {
            new SelectListItem("Masculin", "0"),
            new SelectListItem("Féminin", "1")
        }, "Value", "Text");
}
