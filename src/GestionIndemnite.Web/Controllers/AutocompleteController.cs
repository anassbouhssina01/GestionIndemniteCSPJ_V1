using GestionIndemnite.Application.RHOrganisation.DTOs;
using GestionIndemnite.Application.RHOrganisation.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestionIndemnite.Web.Controllers;

/// <summary>
/// Fournit les suggestions d'autocomplétion des champs de recherche.
/// </summary>
public class AutocompleteController : Controller
{
    private const int MaxSuggestions = 15;

    private readonly FonctionnaireService _fonctionnaires;
    private readonly TribunalService _tribunaux;
    private readonly UniteOrganisationnelleService _unites;

    public AutocompleteController(
        FonctionnaireService fonctionnaires,
        TribunalService tribunaux,
        UniteOrganisationnelleService unites)
    {
        _fonctionnaires = fonctionnaires;
        _tribunaux = tribunaux;
        _unites = unites;
    }

    [HttpGet]
    public async Task<IActionResult> Fonctionnaires(
        string champ,
        string? terme,
        CancellationToken ct = default)
    {
        var champNormalise = (champ ?? string.Empty).Trim().ToLowerInvariant();

        IReadOnlyList<FonctionnaireDto> resultats = champNormalise switch
        {
            "matricule" => await _fonctionnaires.SearchAsync(matricule: terme, ct: ct),
            "cin" => await _fonctionnaires.SearchAsync(cin: terme, ct: ct),
            "nom" => await _fonctionnaires.SearchAsync(nom: terme, ct: ct),
            _ => Array.Empty<FonctionnaireDto>()
        };

        IEnumerable<string> valeurs = champNormalise switch
        {
            "matricule" => resultats.Select(f => f.Matricule),
            "cin" => resultats.Select(f => f.Cin),
            "nom" => resultats.SelectMany(f => new[] { f.NomFr, f.NomAr }),
            _ => Enumerable.Empty<string>()
        };

        return Json(Nettoyer(valeurs));
    }

    [HttpGet]
    public async Task<IActionResult> Tribunaux(string? terme, CancellationToken ct = default)
    {
        var valeur = terme?.Trim();
        var valeurs = (await _tribunaux.ListAsync(includeInactive: true, ct: ct))
            .Where(t => string.IsNullOrWhiteSpace(valeur)
                || t.NomTribunal.Contains(valeur, StringComparison.OrdinalIgnoreCase)
                || t.NomTribunalAr.Contains(valeur, StringComparison.OrdinalIgnoreCase))
            .Select(t => t.NomTribunal);

        return Json(Nettoyer(valeurs));
    }

    [HttpGet]
    public async Task<IActionResult> Unites(string? terme, CancellationToken ct = default)
    {
        var valeur = terme?.Trim();
        var valeurs = (await _unites.ListAsync(includeInactive: true, ct: ct))
            .Where(u => string.IsNullOrWhiteSpace(valeur)
                || u.NomUnite.Contains(valeur, StringComparison.OrdinalIgnoreCase)
                || u.NomUniteAr.Contains(valeur, StringComparison.OrdinalIgnoreCase))
            .Select(u => u.NomUnite);

        return Json(Nettoyer(valeurs));
    }

    private static IEnumerable<string> Nettoyer(IEnumerable<string> valeurs) =>
        valeurs
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(v => v)
            .Take(MaxSuggestions);
}
