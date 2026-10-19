using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using GestionIndemnite.Web.RHOrganisation.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;

namespace GestionIndemnite.Tests;

public class RegleMetierExceptionFilterTests
{
    private static ExceptionContext CreerContexte(
        Exception exception,
        string path = "/Fonctionnaire/Create",
        string? referer = "https://localhost/Fonctionnaire/Create")
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "POST";
        httpContext.Request.Host = new HostString("localhost");
        httpContext.Request.Path = path;
        if (referer is not null)
            httpContext.Request.Headers.Referer = referer;

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
    public async Task Filtre_ignore_les_autres_exceptions()
    {
        var contexte = CreerContexte(new InvalidOperationException("panne technique"));

        await CreerFiltre().OnExceptionAsync(contexte);

        Assert.False(contexte.ExceptionHandled);
        Assert.Null(contexte.Result);
    }
}
