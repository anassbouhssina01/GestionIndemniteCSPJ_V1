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

    public async Task<IActionResult> Index(
        bool inactifs = false,
        string? nom = null,
        TypeTribunal? type = null,
        NiveauJuridiction? niveau = null,
        Guid? idVille = null,
        Guid? idRessort = null,
        CancellationToken ct = default)
    {
        var tous = await _service.ListAsync(true, ct);
        var villes = await _villes.ListAsync(null, true, ct);
        var byId = tous.ToDictionary(t => t.Id);

        var parentIds = new Dictionary<Guid, Guid?>();
        foreach (var tribunal in tous)
        {
            var parent = await _service.GetParentAtAsync(tribunal.Id, null, ct);
            parentIds[tribunal.Id] = parent?.Id;
        }

        var ressorts = tous.ToDictionary(
            t => t.Id,
            t => ResoudreRessortId(t.Id, byId, parentIds));

        var liste = tous.Where(t => inactifs || t.IsActive);

        if (!string.IsNullOrWhiteSpace(nom))
        {
            var terme = nom.Trim();
            liste = liste.Where(t =>
                t.NomTribunal.Contains(terme, StringComparison.OrdinalIgnoreCase)
                || (t.NomTribunalAr ?? string.Empty).Contains(terme, StringComparison.OrdinalIgnoreCase));
        }

        if (type.HasValue)
        {
            var valeur = type.Value;
            liste = liste.Where(t => t.TypeTribunal == valeur);
        }

        if (niveau.HasValue)
        {
            var valeur = niveau.Value;
            liste = liste.Where(t => t.NiveauJuridiction == valeur);
        }

        if (idVille.HasValue)
        {
            var valeur = idVille.Value;
            liste = liste.Where(t => t.IdVille == valeur);
        }

        if (idRessort.HasValue)
        {
            var valeur = idRessort.Value;
            liste = liste.Where(t => ressorts[t.Id] == valeur);
        }

        var resultat = liste.ToList();

        ViewBag.Inactifs = inactifs;
        ViewBag.Nom = nom;
        ViewBag.TypeFiltre = type;
        ViewBag.NiveauFiltre = niveau;
        ViewBag.IdVilleFiltre = idVille;
        ViewBag.IdRessortFiltre = idRessort;
        ViewBag.Villes = villes.ToDictionary(v => v.Id, v => v.NomVille);
        ViewBag.TypesFr = TypesFr;
        ViewBag.NiveauxFr = NiveauxFr;
        ViewBag.ById = byId;
        ViewBag.Ressorts = ressorts;

        var parents = new Dictionary<Guid, string?>();
        foreach (var kv in parentIds)
        {
            parents[kv.Key] = kv.Value is Guid parentId && byId.TryGetValue(parentId, out var parent)
                ? parent.NomTribunal
                : null;
        }
        ViewBag.Parents = parents;

        return View(resultat);
    }

    private static Guid? ResoudreRessortId(
        Guid id,
        IReadOnlyDictionary<Guid, TribunalDto> byId,
        IReadOnlyDictionary<Guid, Guid?> parentIds)
    {
        var courant = id;
        var vus = new HashSet<Guid>();

        while (vus.Add(courant) && byId.TryGetValue(courant, out var tribunal))
        {
            if (tribunal.NiveauJuridiction is NiveauJuridiction.CA or NiveauJuridiction.Cassation)
                return tribunal.Id;

            if (!parentIds.TryGetValue(courant, out var parentId) || parentId is null)
                return null;

            courant = parentId.Value;
        }

        return null;
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
