using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace GestionIndemnite.Web.RHOrganisation.Filters;

/// <summary>
/// Filtre global : toute exception levée pendant l'exécution d'une action est
/// transformée en message affiché dans une fenêtre popup (toast), en français.
/// </summary>
public sealed class RegleMetierExceptionFilter : IAsyncExceptionFilter
{
    internal const string MessageErreurGenerique =
        "Une erreur inattendue s'est produite. Veuillez réessayer.";

    private readonly ILogger<RegleMetierExceptionFilter> _logger;

    public RegleMetierExceptionFilter(ILogger<RegleMetierExceptionFilter> logger)
    {
        _logger = logger;
    }

    public Task OnExceptionAsync(ExceptionContext context)
    {
        var message = ObtenirMessage(context.Exception);

        if (context.Exception is RegleMetierException regleMetier)
            _logger.LogWarning(regleMetier, "Règle métier violée : {Message}", regleMetier.Message);
        else
            _logger.LogError(context.Exception, "Erreur non gérée : {Message}", context.Exception.Message);

        context.ExceptionHandled = true;

        StockerMessage(context, message);
        context.Result = CreerRedirection(context);

        return Task.CompletedTask;
    }

    private static string ObtenirMessage(Exception exception) => exception switch
    {
        RegleMetierException regleMetier when !string.IsNullOrWhiteSpace(regleMetier.Message)
            => regleMetier.Message,
        _ => MessageErreurGenerique
    };

    private static void StockerMessage(ExceptionContext context, string message)
    {
        var tempDataFactory = context.HttpContext.RequestServices.GetService<ITempDataDictionaryFactory>();
        if (tempDataFactory is null)
            return;

        var tempData = tempDataFactory.GetTempData(context.HttpContext);
        tempData["Erreur"] = message;
    }

    private static IActionResult CreerRedirection(ExceptionContext context)
    {
        var http = context.HttpContext;
        var referer = http.Request.Headers.Referer.ToString();

        if (!string.IsNullOrEmpty(referer)
            && Uri.TryCreate(referer, UriKind.Absolute, out var uri)
            && uri.Host == http.Request.Host.Host
            && uri.AbsolutePath != http.Request.Path)
        {
            return new RedirectResult(referer);
        }

        return new RedirectToActionResult("Index", "Home", null);
    }
}
