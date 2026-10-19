using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Application.RHOrganisation.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestionIndemnite.Web.Controllers;

public class HistoriqueOrganigrammeTribunalController : Controller
{
    private readonly HistoriqueOrganigrammeTribunalService _service;
    private readonly TribunalService _tribunaux;
    private readonly PieceJointeService _pieces;

    public HistoriqueOrganigrammeTribunalController(
        HistoriqueOrganigrammeTribunalService service,
        TribunalService tribunaux,
        PieceJointeService pieces)
    {
        _service = service;
        _tribunaux = tribunaux;
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
            var tribunaux = await _tribunaux.ListAsync(true, ct);
            if (!string.IsNullOrWhiteSpace(nom))
            {
                tribunaux = tribunaux
                    .Where(t => t.NomTribunal.Contains(nom, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            ViewBag.Nom = nom;
            ViewBag.TypesFr = TribunalController.TypesFrPublic;
            ViewBag.NiveauxFr = TribunalController.NiveauxFrPublic;
            return View("Selection", tribunaux);
        }

        var tribunal = await _tribunaux.GetAsync(id.Value, ct);
        var parentActuel = await _tribunaux.GetParentAtAsync(id.Value, null, ct);
        var historique = await _service.ListHistoryAsync(id.Value, ct);
        var candidats = await _tribunaux.ListAsync(false, ct);
        var documents = await _pieces.SearchAsync(null, null, null, ct);

        ViewData["Tribunal"] = tribunal;
        ViewBag.ParentActuel = parentActuel;
        ViewBag.Candidats = candidats.Where(t => t.Id != id.Value).ToList();
        ViewBag.Documents = documents;

        return View(historique);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangerParent(
        Guid id,
        ChangeParentTribunalCommand cmd,
        CancellationToken ct)
    {
        await _service.ChangeParentAsync(id, cmd, ct);

        TempData["Succes"] = "Le nouveau parent est enregistré à partir de la date indiquée.";
        return RedirectToAction(nameof(Index), new { id });
    }
}
