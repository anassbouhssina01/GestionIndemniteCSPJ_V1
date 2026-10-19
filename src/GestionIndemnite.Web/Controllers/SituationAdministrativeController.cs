using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Application.RHOrganisation.Services;
using GestionIndemnite.Application.RHOrganisation.Services.Referentiels;
using Microsoft.AspNetCore.Mvc;

namespace GestionIndemnite.Web.Controllers;

public class SituationAdministrativeController : Controller
{
    private readonly SituationAdministrativeService _service;
    private readonly FonctionnaireService _fonctionnaires;
    private readonly StatutAdministratifService _statuts;
    private readonly PieceJointeService _pieces;

    public SituationAdministrativeController(
        SituationAdministrativeService service,
        FonctionnaireService fonctionnaires,
        StatutAdministratifService statuts,
        PieceJointeService pieces)
    {
        _service = service;
        _fonctionnaires = fonctionnaires;
        _statuts = statuts;
        _pieces = pieces;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        Guid? id,
        string? matricule,
        string? cin,
        string? nom,
        CancellationToken ct)
    {
        if (!id.HasValue)
        {
            ViewBag.Matricule = matricule;
            ViewBag.Cin = cin;
            ViewBag.Nom = nom;

            var rechercheFaite = !string.IsNullOrWhiteSpace(matricule)
                || !string.IsNullOrWhiteSpace(cin)
                || !string.IsNullOrWhiteSpace(nom);

            ViewBag.RechercheFaite = rechercheFaite;
            ViewBag.Resultats = rechercheFaite
                ? await _fonctionnaires.SearchAsync(matricule, cin, nom, ct)
                : Array.Empty<FonctionnaireDto>();

            return View();
        }

        var fonctionnaire = await _fonctionnaires.GetAsync(id.Value, ct);
        var historique = await _service.ListByFonctionnaireAsync(id.Value, ct);
        var statuts = await _statuts.ListAsync(true, ct);
        var documents = await _pieces.SearchAsync(null, null, null, ct);

        ViewData["Fonctionnaire"] = fonctionnaire;
        ViewBag.Statuts = statuts;
        ViewBag.Documents = documents;

        return View(historique);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ajouter(
        Guid id,
        CreateSituationAdministrativeCommand cmd,
        CancellationToken ct)
    {
        await _service.AddAsync(
            cmd with { IdFonctionnaire = id }, ct);

        TempData["Succes"] = "La situation administrative a été ajoutée avec succès.";
        return RedirectToAction(nameof(Index), new { id });
    }
}
