using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using GestionIndemnite.Web.RHOrganisation.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace GestionIndemnite.Tests;

public class RegleMetierExceptionFilterTests
{
    private sealed class TempDataDictionaryFake : ITempDataDictionary
    {
        private readonly Dictionary<string, object?> _valeurs = new(StringComparer.Ordinal);

        public object? this[string key]
        {
            get => _valeurs[key];
            set => _valeurs[key] = value;
        }

        public ICollection<string> Keys => _valeurs.Keys;
        public ICollection<object?> Values => _valeurs.Values;
        public int Count => _valeurs.Count;
        public bool IsReadOnly => false;

        public void Add(string key, object? value) => _valeurs.Add(key, value);
        public void Add(KeyValuePair<string, object?> item) => _valeurs.Add(item.Key, item.Value);
        public void Clear() => _valeurs.Clear();
        public bool Contains(KeyValuePair<string, object?> item) => ((IDictionary<string, object?>)_valeurs).Contains(item);
        public bool ContainsKey(string key) => _valeurs.ContainsKey(key);
        public void CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex) => ((IDictionary<string, object?>)_valeurs).CopyTo(array, arrayIndex);
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => _valeurs.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _valeurs.GetEnumerator();
        public bool Remove(string key) => _valeurs.Remove(key);
        public bool Remove(KeyValuePair<string, object?> item) => ((IDictionary<string, object?>)_valeurs).Remove(item);
        public bool TryGetValue(string key, out object? value) => _valeurs.TryGetValue(key, out value);
        public void Load() { }
        public void Save() { }
        public void Keep() { }
        public void Keep(string key) { }
        public object? Peek(string key) => _valeurs.TryGetValue(key, out var valeur) ? valeur : null;
    }

    private sealed class TempDataFactoryFake : ITempDataDictionaryFactory
    {
        public TempDataDictionaryFake Dictionary { get; } = new();

        public ITempDataDictionary GetTempData(HttpContext httpContext) => Dictionary;
    }

    private static ExceptionContext CreerContexte(
        Exception exception,
        string path = "/Fonctionnaire/Create",
        string? referer = "https://localhost/Fonctionnaire/Create",
        TempDataFactoryFake? tempData = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "POST";
        httpContext.Request.Host = new HostString("localhost");
        httpContext.Request.Path = path;
        if (referer is not null)
            httpContext.Request.Headers.Referer = referer;

        httpContext.RequestServices = new ServiceCollection()
            .AddSingleton<ITempDataDictionaryFactory>(tempData ?? new TempDataFactoryFake())
            .BuildServiceProvider();

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = exception
        };
    }

    private static RegleMetierExceptionFilter CreerFiltre() =>
        new(NullLogger<RegleMetierExceptionFilter>.Instance);

    [Fact]
    public async Task Filtre_redirige_vers_referer_si_meme_hote_et_page_differente()
    {
        var contexte = CreerContexte(
            new RegleMetierException("الفترة غير صالحة"),
            path: "/Fonctionnaire/Create",
            referer: "https://localhost/Fonctionnaire/Index");

        await CreerFiltre().OnExceptionAsync(contexte);

        Assert.True(contexte.ExceptionHandled);
        var redirection = Assert.IsType<RedirectResult>(contexte.Result);
        Assert.Equal("https://localhost/Fonctionnaire/Index", redirection.Url);
    }

    [Fact]
    public async Task Filtre_sans_referer_redirige_vers_accueil()
    {
        var contexte = CreerContexte(new RegleMetierException("الفترة غير صالحة"), referer: null);

        await CreerFiltre().OnExceptionAsync(contexte);

        Assert.True(contexte.ExceptionHandled);
        var redirection = Assert.IsType<RedirectToActionResult>(contexte.Result);
        Assert.Equal("Index", redirection.ActionName);
        Assert.Equal("Home", redirection.ControllerName);
    }

    [Fact]
    public async Task Filtre_referer_meme_page_redirige_vers_accueil()
    {
        var contexte = CreerContexte(
            new RegleMetierException("الفترة غير صالحة"),
            path: "/Fonctionnaire/Create",
            referer: "https://localhost/Fonctionnaire/Create");

        await CreerFiltre().OnExceptionAsync(contexte);

        Assert.IsType<RedirectToActionResult>(contexte.Result);
    }

    [Fact]
    public async Task Filtre_referer_hote_different_redirige_vers_accueil()
    {
        var contexte = CreerContexte(
            new RegleMetierException("الفترة غير صالحة"),
            referer: "https://exemple.test/Fonctionnaire/Index");

        await CreerFiltre().OnExceptionAsync(contexte);

        Assert.IsType<RedirectToActionResult>(contexte.Result);
    }

    [Fact]
    public async Task Filtre_transforme_les_autres_exceptions_en_message_generique()
    {
        var tempData = new TempDataFactoryFake();
        var contexte = CreerContexte(
            new InvalidOperationException("panne technique"),
            referer: "https://localhost/Fonctionnaire/Index",
            tempData: tempData);

        await CreerFiltre().OnExceptionAsync(contexte);

        Assert.True(contexte.ExceptionHandled);
        Assert.Equal(
            "Une erreur inattendue s'est produite. Veuillez réessayer.",
            tempData.Dictionary["Erreur"]);
    }

    [Fact]
    public async Task Filtre_stocke_le_message_dans_tempdata_pour_affichage_en_popup()
    {
        var tempData = new TempDataFactoryFake();
        var contexte = CreerContexte(
            new RegleMetierException("La période demandée est invalide."),
            referer: "https://localhost/Fonctionnaire/Index",
            tempData: tempData);

        await CreerFiltre().OnExceptionAsync(contexte);

        Assert.True(contexte.ExceptionHandled);
        Assert.Equal("La période demandée est invalide.", tempData.Dictionary["Erreur"]);
    }
}
