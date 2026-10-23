using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Application.RHOrganisation.Services;
using GestionIndemnite.Application.RHOrganisation.Services.Referentiels;
using GestionIndemnite.Domain.RHOrganisation.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GestionIndemnite.Web.Controllers;

public class TribunalController : Controller
{
    public record TribunalRessortItem(
        TribunalDto Tribunal,
        ParentOrganigrammeDto? CurrentParent,
        Guid? RessortId,
        DateOnly? RattachementDepuis,
        DateOnly? RattachementJusquA,
        bool IsCurrentRattachement);

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

        var parents = new Dictionary<Guid, ParentOrganigrammeDto?>();
        foreach (var tribunal in tous)
        {
            parents[tribunal.Id] = await _service.GetParentAtAsync(tribunal.Id, null, ct);
        }

        var historiqueParTribunal = new Dictionary<Guid, IReadOnlyList<HistoriqueRattachementDto>>();
        foreach (var tribunal in tous)
        {
            historiqueParTribunal[tribunal.Id] = await _service.GetHistoriqueRattachementsAsync(tribunal.Id, ct);
        }

        async Task<Guid?> GetRessortForParentAsync(Guid parentId, DateOnly at)
        {
            var courant = parentId;
            var vus = new HashSet<Guid>();
            var guard = 0;
            while (vus.Add(courant) && guard++ < 100 && byId.TryGetValue(courant, out var tribunal))
            {
                if (tribunal.NiveauJuridiction is NiveauJuridiction.CA or NiveauJuridiction.Cassation)
                    return tribunal.Id;

                var parentAt = await _service.GetParentAtAsync(courant, at, ct);
                if (parentAt is null)
                    return null;
                courant = parentAt.Id;
            }
            return null;
        }

        var items = new List<TribunalRessortItem>();
        foreach (var tribunal in tous)
        {
            var hist = historiqueParTribunal[tribunal.Id];
            if (hist.Count == 0)
            {
                var currentParent = parents[tribunal.Id];
                var ressortId = currentParent is null
                    ? (tribunal.NiveauJuridiction is NiveauJuridiction.CA or NiveauJuridiction.Cassation ? tribunal.Id : null)
                    : await GetRessortForParentAsync(currentParent.Id, currentParent.DateDebut);
                items.Add(new TribunalRessortItem(tribunal, currentParent, ressortId, currentParent?.DateDebut, currentParent?.DateFin, true));
                continue;
            }

            foreach (var h in hist)
            {
                if (h.IdTribunalParent is null)
                    continue;
                var ressortId = await GetRessortForParentAsync(h.IdTribunalParent.Value, h.DateDebut);
                if (ressortId is null)
                    continue;
                var current = parents[tribunal.Id];
                var isCurrent = h.DateFin == null && current is not null && current.Id == h.IdTribunalParent;
                items.Add(new TribunalRessortItem(tribunal, new ParentOrganigrammeDto(h.IdTribunalParent.Value, h.NomTribunalParent ?? string.Empty, h.DateDebut, h.DateFin), ressortId, h.DateDebut, h.DateFin, isCurrent));
            }

            if (!hist.Any(h => h.DateFin == null && h.IdTribunalParent.HasValue))
            {
                var currentParent = parents[tribunal.Id];
                if (currentParent is not null)
                {
                    var ressortId = await GetRessortForParentAsync(currentParent.Id, currentParent.DateDebut);
                    if (ressortId.HasValue && !items.Any(x => x.Tribunal.Id == tribunal.Id && x.RessortId == ressortId && x.IsCurrentRattachement))
                        items.Add(new TribunalRessortItem(tribunal, currentParent, ressortId, currentParent.DateDebut, currentParent.DateFin, true));
                }
            }
        }

        var listeItems = items.Where(i => inactifs || i.Tribunal.IsActive).ToList();

        if (!string.IsNullOrWhiteSpace(nom))
        {
            var terme = nom.Trim();
            listeItems = listeItems.Where(i =>
                i.Tribunal.NomTribunal.Contains(terme, StringComparison.OrdinalIgnoreCase)
                || (i.Tribunal.NomTribunalAr ?? string.Empty).Contains(terme, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (type.HasValue)
        {
            var valeur = type.Value;
            listeItems = listeItems.Where(i => i.Tribunal.TypeTribunal == valeur).ToList();
        }

        if (niveau.HasValue)
        {
            var valeur = niveau.Value;
            listeItems = listeItems.Where(i => i.Tribunal.NiveauJuridiction == valeur).ToList();
        }

        if (idVille.HasValue)
        {
            var valeur = idVille.Value;
            listeItems = listeItems.Where(i => i.Tribunal.IdVille == valeur).ToList();
        }

        if (idRessort.HasValue)
        {
            var valeur = idRessort.Value;
            listeItems = listeItems.Where(i => i.RessortId == valeur).ToList();
        }

        var ressortsMap = tous.ToDictionary(
            t => t.Id,
            t => ResoudreRessortId(t.Id, byId, parents));

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
        ViewBag.Ressorts = ressortsMap;
        ViewBag.Parents = parents;
        ViewBag.Items = listeItems;

        return View(tous.Where(t => inactifs || t.IsActive).DistinctBy(t => t.Id).ToList());
    }

    private static Guid? ResoudreRessortId(
        Guid id,
        IReadOnlyDictionary<Guid, TribunalDto> byId,
        IReadOnlyDictionary<Guid, ParentOrganigrammeDto?> parents)
    {
        var courant = id;
        var vus = new HashSet<Guid>();

        while (vus.Add(courant) && byId.TryGetValue(courant, out var tribunal))
        {
            if (tribunal.NiveauJuridiction is NiveauJuridiction.CA or NiveauJuridiction.Cassation)
                return tribunal.Id;

            if (!parents.TryGetValue(courant, out var parent) || parent is null)
                return null;

            courant = parent.Id;
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

    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var tribunal = await _service.GetAsync(id, ct);
        await RemplirSelectsAsync(id, ct, tribunal);
        return View(tribunal);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateTribunalCommand command, CancellationToken ct)
    {
        await _service.UpdateAsync(id, command, ct);
        TempData["Succes"] = "Le tribunal a été mis à jour.";
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

    private async Task RemplirSelectsAsync(
        Guid? idExclu,
        CancellationToken ct,
        TribunalDto? selection = null)
    {
        var villes = await _villes.ListAsync(null, false, ct);
        var tribunaux = await _service.ListAsync(false, ct);

        ViewBag.Villes = new SelectList(
            villes.Select(v => new SelectListItem(v.NomVille, v.Id.ToString())),
            "Value", "Text",
            selection?.IdVille.ToString());

        ViewBag.Parents = new SelectList(
            tribunaux.Where(t => t.Id != idExclu)
                .Select(t => new SelectListItem($"{t.NomTribunal} ({NiveauxFr[t.NiveauJuridiction]})", t.Id.ToString())),
            "Value", "Text");

        ViewBag.Types = new SelectList(
            TypesFr.Select(kv => new SelectListItem(kv.Value, ((int)kv.Key).ToString())),
            "Value", "Text",
            selection is null ? null : ((int)selection.TypeTribunal).ToString());

        ViewBag.Niveaux = new SelectList(
            NiveauxFr.Select(kv => new SelectListItem(kv.Value, ((int)kv.Key).ToString())),
            "Value", "Text",
            selection is null ? null : ((int)selection.NiveauJuridiction).ToString());
    }
}
