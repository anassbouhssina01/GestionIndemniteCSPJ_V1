using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Application.RHOrganisation.Services;
using GestionIndemnite.Application.RHOrganisation.Services.Referentiels;
using GestionIndemnite.Domain.RHOrganisation.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestionIndemnite.Web.Controllers;

public class TribunalController : Controller
{
    private static readonly Dictionary<TypeTribunal, string> TypesFr = new()
    {
        [TypeTribunal.Generale] = "Générale",
        [TypeTribunal.Administratif] = "Administratif",
        [TypeTribunal.Commercial] = "Commercial",
        [TypeTribunal.Penal] = "Pénal",
        [TypeTribunal.Social] = "Social",
        [TypeTribunal.Civil] = "Civil"
    };

    private static readonly Dictionary<NiveauJuridiction, string> NiveauxFr = new()
    {
        [NiveauJuridiction.CJR] = "CJR",
        [NiveauJuridiction.TPI] = "TPI",
        [NiveauJuridiction.CA] = "Cour d'appel (CA)",
        [NiveauJuridiction.Cassation] = "Cour de cassation"
    };

    public static IReadOnlyDictionary<TypeTribunal, string> TypesFrPublic => TypesFr;

    public static IReadOnlyDictionary<NiveauJuridiction, string> NiveauxFrPublic => NiveauxFr;

    private readonly TribunalService _service;
    private readonly VilleService _villes;

    public TribunalController(TribunalService service, VilleService villes)
    {
        _service = service;
        _villes = villes;
    }

    public async Task<IActionResult> Index(bool inactifs = false, CancellationToken ct = default)
    {
        var tribunaux = await _service.ListAsync(inactifs, ct);
        var villes = await _villes.ListAsync(null, true, ct);

        ViewBag.Inactifs = inactifs;
        ViewBag.Villes = villes.ToDictionary(v => v.Id, v => v.NomVille);
        ViewBag.TypesFr = TypesFr;
        ViewBag.NiveauxFr = NiveauxFr;

        var parents = new Dictionary<Guid, string?>();
        foreach (var tribunal in tribunaux)
        {
            var parent = await _service.GetParentAtAsync(tribunal.Id, null, ct);
            parents[tribunal.Id] = parent?.Nom;
        }
        ViewBag.Parents = parents;

        return View(tribunaux);
    }

    public async Task<IActionResult> Create(CancellationToken ct)
    {
        await RemplirSelectsAsync(null, ct);
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTribunalCommand command, CancellationToken ct)
    {
        await _service.CreateAsync(command, ct);
        TempData["Succes"] = "Le tribunal a été créé.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disable(Guid id, CancellationToken ct)
    {
        await _service.DisableAsync(id, ct);
        TempData["Succes"] = "Le tribunal a été désactivé.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enable(Guid id, CancellationToken ct)
    {
        await _service.EnableAsync(id, ct);
        TempData["Succes"] = "Le tribunal a été activé.";
        return RedirectToAction(nameof(Index));
    }

    private async Task RemplirSelectsAsync(Guid? idExclu, CancellationToken ct)
    {
        var villes = await _villes.ListAsync(null, false, ct);
        var tribunaux = await _service.ListAsync(false, ct);

        ViewBag.Villes = new SelectList(
            villes.Select(v => new SelectListItem(v.NomVille, v.Id.ToString())),
            "Value", "Text");

        ViewBag.Parents = new SelectList(
            tribunaux.Where(t => t.Id != idExclu)
                .Select(t => new SelectListItem($"{t.NomTribunal} ({NiveauxFr[t.NiveauJuridiction]})", t.Id.ToString())),
            "Value", "Text");

        ViewBag.Types = new SelectList(
            TypesFr.Select(kv => new SelectListItem(kv.Value, ((int)kv.Key).ToString())),
            "Value", "Text");

        ViewBag.Niveaux = new SelectList(
            NiveauxFr.Select(kv => new SelectListItem(kv.Value, ((int)kv.Key).ToString())),
            "Value", "Text");
    }
}
