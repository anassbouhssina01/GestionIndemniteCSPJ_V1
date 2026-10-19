using GestionIndemnite.Domain.RHOrganisation.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GestionIndemnite.Web.RHOrganisation.Filters;

public sealed class RegleMetierExceptionFilter : IAsyncExceptionFilter
{
    private readonly ILogger<RegleMetierExceptionFilter> _logger;

    public RegleMetierExceptionFilter(ILogger<RegleMetierExceptionFilter> logger)
    {
        _logger = logger;
    }

    public Task OnExceptionAsync(ExceptionContext context)
    {
        if (context.Exception is not RegleMetierException exception)
            return Task.CompletedTask;

        _logger.LogWarning(exception, "Règle métier violée : {Message}", exception.Message);

        context.ExceptionHandled = true;
        context.Result = CreerRedirection(context);

        return Task.CompletedTask;
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
