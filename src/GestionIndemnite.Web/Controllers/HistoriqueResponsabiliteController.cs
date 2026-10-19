using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Application.RHOrganisation.Services;
using GestionIndemnite.Application.RHOrganisation.Services.Referentiels;
using Microsoft.AspNetCore.Mvc;

namespace GestionIndemnite.Web.Controllers;

public class HistoriqueResponsabiliteController : Controller
{
    private readonly HistoriqueResponsabiliteService _service;
    private readonly FonctionnaireService _fonctionnaires;
    private readonly ResponsabiliteService _responsabilites;
    private readonly TribunalService _tribunaux;
    private readonly UniteOrganisationnelleService _unites;
    private readonly PieceJointeService _pieces;

    public HistoriqueResponsabiliteController(
        HistoriqueResponsabiliteService service,
        FonctionnaireService fonctionnaires,
        ResponsabiliteService responsabilites,
        TribunalService tribunaux,
        UniteOrganisationnelleService unites,
        PieceJointeService pieces)
    {
        _service = service;
        _fonctionnaires = fonctionnaires;
        _responsabilites = responsabilites;
        _tribunaux = tribunaux;
        _unites = unites;
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
        var responsabilites = await _responsabilites.ListAsync(false, ct);
        var tribunaux = await _tribunaux.ListAsync(false, ct);
        var unites = await _unites.ListAsync(false, ct);
        var documents = await _pieces.SearchAsync(null, null, null, ct);

        ViewData["Fonctionnaire"] = fonctionnaire;
        ViewBag.Responsabilites = responsabilites;
        ViewBag.Tribunaux = tribunaux;
        ViewBag.Unites = unites;
        ViewBag.Documents = documents;

        return View(historique);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ajouter(
        Guid id,
        CreateHistoriqueResponsabiliteCommand cmd,
        CancellationToken ct)
    {
        await _service.AddAsync(
            cmd with { IdFonctionnaire = id }, ct);

        TempData["Succes"] = "La responsabilité a été ajoutée avec succès.";
        return RedirectToAction(nameof(Index), new { id });
    }
}
