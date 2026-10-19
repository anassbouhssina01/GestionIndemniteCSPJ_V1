using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Application.RHOrganisation.Services.Referentiels;
using Microsoft.AspNetCore.Mvc;

namespace GestionIndemnite.Web.Controllers;

public class PaysController : Controller
{
    private readonly PaysService _service;

    public PaysController(PaysService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(bool inactifs = false, CancellationToken ct = default)
    {
        ViewData["Inactifs"] = inactifs;
        return View(await _service.ListAsync(inactifs, ct));
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePaysCommand command, CancellationToken ct)
    {
        await _service.CreateAsync(command, ct);
        TempData["Succes"] = "Le pays a été créé.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        return View(await _service.GetAsync(id, ct));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdatePaysCommand command, CancellationToken ct)
    {
        await _service.UpdateAsync(id, command, ct);
        TempData["Succes"] = "Le pays a été mis à jour.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable(Guid id, CancellationToken ct)
    {
        await _service.DisableAsync(id, ct);
        TempData["Succes"] = "Le pays a été désactivé.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enable(Guid id, CancellationToken ct)
    {
        await _service.EnableAsync(id, ct);
        TempData["Succes"] = "Le pays a été activé.";
        return RedirectToAction(nameof(Index));
    }
}
