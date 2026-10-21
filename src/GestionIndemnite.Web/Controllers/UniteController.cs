using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Application.RHOrganisation.Services;
using GestionIndemnite.Application.RHOrganisation.Services.Referentiels;
using GestionIndemnite.Domain.RHOrganisation.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestionIndemnite.Web.Controllers;

public class UniteController : Controller
{
    private static readonly Dictionary<TypeUnite, string> TypesFr = new()
    {
        [TypeUnite.Unite] = "Unité",
        [TypeUnite.Departement] = "Département",
        [TypeUnite.Pole] = "Pôle",
        [TypeUnite.Administration] = "Administration",
        [TypeUnite.SecretariatGeneral] = "Secrétariat général",
        [TypeUnite.Cabinet] = "Cabinet"
    };

    public static IReadOnlyDictionary<TypeUnite, string> TypesFrPublic => TypesFr;

    private readonly UniteOrganisationnelleService _service;
    private readonly VilleService _villes;

    public UniteController(UniteOrganisationnelleService service, VilleService villes)
    {
        _service = service;
        _villes = villes;
    }

    public async Task<IActionResult> Index(bool inactifs = false, CancellationToken ct = default)
    {
        var unites = await _service.ListAsync(inactifs, ct);
        var villes = await _villes.ListAsync(null, true, ct);

        ViewBag.Inactifs = inactifs;
        ViewBag.Villes = villes.ToDictionary(v => v.Id, v => v.NomVille);
        ViewBag.TypesFr = TypesFr;

        var parents = new Dictionary<Guid, ParentOrganigrammeDto?>();
        foreach (var unite in unites)
        {
            parents[unite.Id] = await _service.GetParentAtAsync(unite.Id, null, ct);
        }
        ViewBag.Parents = parents;

        return View(unites);
    }

    public async Task<IActionResult> Create(CancellationToken ct)
    {
        await RemplirSelectsAsync(ct);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUniteOrganisationnelleCommand command, CancellationToken ct)
    {
        await _service.CreateAsync(command, ct);
        TempData["Succes"] = "L'unité organisationnelle a été créée.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var unite = await _service.GetAsync(id, ct);
        await RemplirSelectsAsync(ct, unite);
        return View(unite);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateUniteOrganisationnelleCommand command, CancellationToken ct)
    {
        await _service.UpdateAsync(id, command, ct);
        TempData["Succes"] = "L'unité organisationnelle a été mise à jour.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable(Guid id, CancellationToken ct)
    {
        await _service.DisableAsync(id, ct);
        TempData["Succes"] = "L'unité organisationnelle a été désactivée.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enable(Guid id, CancellationToken ct)
    {
        await _service.EnableAsync(id, ct);
        TempData["Succes"] = "L'unité organisationnelle a été activée.";
        return RedirectToAction(nameof(Index));
    }

    private async Task RemplirSelectsAsync(
        CancellationToken ct,
        UniteOrganisationnelleDto? selection = null)
    {
        var villes = await _villes.ListAsync(null, false, ct);
        var unites = await _service.ListAsync(false, ct);

        ViewBag.Villes = new SelectList(
            villes.Select(v => new SelectListItem(v.NomVille, v.Id.ToString())),
            "Value", "Text",
            selection?.IdVille.ToString());

        ViewBag.Parents = new SelectList(
            unites.Where(u => selection is null || u.Id != selection.Id)
                .Select(u => new SelectListItem(u.NomUnite, u.Id.ToString())),
            "Value", "Text");

        ViewBag.Types = new SelectList(
            TypesFr.Select(kv => new SelectListItem(kv.Value, ((int)kv.Key).ToString())),
            "Value", "Text",
            selection is null ? null : ((int)selection.TypeUnite).ToString());
    }
}
