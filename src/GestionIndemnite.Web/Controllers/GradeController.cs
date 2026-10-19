using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Application.RHOrganisation.Services.Referentiels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestionIndemnite.Web.Controllers;

public class GradeController : Controller
{
    private readonly GradeService _service;
    private readonly CadreService _cadres;

    public GradeController(GradeService service, CadreService cadres)
    {
        _service = service;
        _cadres = cadres;
    }

    public async Task<IActionResult> Index(Guid? idCadre = null, bool inactifs = false, CancellationToken ct = default)
    {
        var grades = await _service.ListAsync(idCadre, inactifs, ct);
        var cadres = await _cadres.ListAsync(true, ct);

        ViewBag.Cadres = cadres.ToDictionary(c => c.Id, c => c.LibelleCadreFr);
        ViewBag.IdCadre = idCadre;
        ViewBag.Inactifs = inactifs;
        ViewBag.FiltreCadres = new SelectList(
            cadres.Select(c => new SelectListItem(c.LibelleCadreFr, c.Id.ToString(), c.Id == idCadre)),
            "Value", "Text", "");
        return View(grades);
    }

    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewBag.FiltreCadres = await CreerSelectCadresAsync(ct);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateGradeCommand command, CancellationToken ct)
    {
        await _service.CreateAsync(command, ct);
        TempData["Succes"] = "Le grade a été créé.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var grade = await _service.GetAsync(id, ct);
        ViewBag.FiltreCadres = await CreerSelectCadresAsync(ct);
        ViewBag.Cadres = (await _cadres.ListAsync(true, ct))
            .ToDictionary(c => c.Id, c => c.LibelleCadreFr);
        return View(grade);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateGradeCommand command, CancellationToken ct)
    {
        await _service.UpdateAsync(id, command, ct);
        TempData["Succes"] = "Le grade a été mis à jour.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable(Guid id, CancellationToken ct)
    {
        await _service.DisableAsync(id, ct);
        TempData["Succes"] = "Le grade a été désactivé.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enable(Guid id, CancellationToken ct)
    {
        await _service.EnableAsync(id, ct);
        TempData["Succes"] = "Le grade a été activé.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<SelectList> CreerSelectCadresAsync(CancellationToken ct)
    {
        var cadres = await _cadres.ListAsync(false, ct);
        var items = cadres
            .Select(c => new SelectListItem(c.LibelleCadreFr, c.Id.ToString()))
            .ToList();
        return new SelectList(items, "Value", "Text");
    }
}
