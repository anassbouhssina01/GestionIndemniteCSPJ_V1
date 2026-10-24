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
        return RedirectToAction("Index", "Fonctionnaire");
    }

    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        var piece = await _service.GetAsync(id, ct);
        var flux = await _service.DownloadAsync(id, ct);
        return File(flux, "application/pdf", piece.NomFichier ?? $"{piece.NumeroDocument}.pdf");
    }
}
