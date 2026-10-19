using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Application.RHOrganisation.Services.Referentiels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestionIndemnite.Web.Controllers;

public class VilleController : Controller
{
    private readonly VilleService _service;
    private readonly PaysService _pays;

    public VilleController(VilleService service, PaysService pays)
    {
        _service = service;
        _pays = pays;
    }

    public async Task<IActionResult> Index(Guid? idPays = null, bool inactifs = false, CancellationToken ct = default)
    {
        var villes = await _service.ListAsync(idPays, inactifs, ct);
        var listePays = await _pays.ListAsync(true, ct);

        ViewBag.Pays = listePays.ToDictionary(p => p.Id, p => p.NomPaysFr);
        ViewBag.IdPays = idPays;
        ViewBag.Inactifs = inactifs;
        ViewBag.FiltrePays = new SelectList(
            listePays.Select(p => new SelectListItem(p.NomPaysFr, p.Id.ToString())),
            "Value", "Text");
        return View(villes);
    }

    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewBag.FiltrePays = await CreerSelectPaysAsync(ct);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateVilleCommand command, CancellationToken ct)
    {
        await _service.CreateAsync(command, ct);
        TempData["Succes"] = "La ville a été créée.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var ville = await _service.GetAsync(id, ct);
        var listePays = await _pays.ListAsync(true, ct);
        ViewBag.Pays = listePays.ToDictionary(p => p.Id, p => p.NomPaysFr);
        return View(ville);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateVilleCommand command, CancellationToken ct)
    {
        await _service.UpdateAsync(id, command, ct);
        TempData["Succes"] = "La ville a été mise à jour.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable(Guid id, CancellationToken ct)
    {
        await _service.DisableAsync(id, ct);
        TempData["Succes"] = "La ville a été désactivée.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enable(Guid id, CancellationToken ct)
    {
        await _service.EnableAsync(id, ct);
        TempData["Succes"] = "La ville a été activée.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<SelectList> CreerSelectPaysAsync(CancellationToken ct)
    {
        var listePays = await _pays.ListAsync(false, ct);
        var items = listePays
            .Select(p => new SelectListItem(p.NomPaysFr, p.Id.ToString()))
            .ToList();
        return new SelectList(items, "Value", "Text");
    }
}
