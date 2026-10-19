using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Application.RHOrganisation.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestionIndemnite.Web.Controllers;

public class PieceJointeController : Controller
{
    private readonly PieceJointeService _service;

    public PieceJointeController(PieceJointeService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(
        string? numeroDocument,
        string? emetteur,
        int? annee,
        CancellationToken ct = default)
    {
        ViewBag.NumeroDocument = numeroDocument;
        ViewBag.Emetteur = emetteur;
        ViewBag.Annee = annee;
        return View(await _service.SearchAsync(numeroDocument, emetteur, annee, ct));
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        UploadPieceJointeCommand command,
        IFormFile? fichier,
        CancellationToken ct)
    {
        if (fichier is null || fichier.Length == 0)
        {
            ModelState.AddModelError("fichier", "Le fichier PDF est obligatoire.");
            return View();
        }

        await using var flux = fichier.OpenReadStream();
        await _service.UploadAsync(flux, command, ct);

        TempData["Succes"] = "Le document a été téléversé.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var piece = await _service.GetAsync(id, ct);
        var flux = await _service.DownloadAsync(id, ct);
        return File(flux, "application/pdf", piece.NomFichier ?? $"{piece.NumeroDocument}.pdf");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        TempData["Succes"] = "Le document a été supprimé.";
        return RedirectToAction(nameof(Index));
    }
}
