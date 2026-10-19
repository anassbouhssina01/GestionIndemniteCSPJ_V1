using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Application.RHOrganisation.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestionIndemnite.Web.Controllers;

public class HistoriqueOrganigrammeUniteController : Controller
{
    private readonly HistoriqueOrganigrammeUniteService _service;
    private readonly UniteOrganisationnelleService _unites;
    private readonly PieceJointeService _pieces;

    public HistoriqueOrganigrammeUniteController(
        HistoriqueOrganigrammeUniteService service,
        UniteOrganisationnelleService unites,
        PieceJointeService pieces)
    {
        _service = service;
        _unites = unites;
        _pieces = pieces;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        Guid? id,
        string? nom,
        CancellationToken ct)
    {
        if (!id.HasValue)
        {
            var unites = await _unites.ListAsync(true, ct);
            if (!string.IsNullOrWhiteSpace(nom))
            {
                unites = unites
                    .Where(u => u.NomUnite.Contains(nom, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            ViewBag.Nom = nom;
            ViewBag.TypesFr = UniteController.TypesFrPublic;
            return View("Selection", unites);
        }

        var unite = await _unites.GetAsync(id.Value, ct);
        var parentActuel = await _unites.GetParentAtAsync(id.Value, null, ct);
        var historique = await _service.ListHistoryAsync(id.Value, ct);
        var candidats = await _unites.ListAsync(false, ct);
        var documents = await _pieces.SearchAsync(null, null, null, ct);

        ViewData["Unite"] = unite;
        ViewBag.ParentActuel = parentActuel;
        ViewBag.Candidats = candidats.Where(u => u.Id != id.Value).ToList();
        ViewBag.Documents = documents;

        return View(historique);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangerParent(
        Guid id,
        ChangeParentUniteCommand cmd,
        CancellationToken ct)
    {
        await _service.ChangeParentAsync(id, cmd, ct);

        TempData["Succes"] = "Le nouveau parent est enregistré à partir de la date indiquée.";
        return RedirectToAction(nameof(Index), new { id });
    }
}
