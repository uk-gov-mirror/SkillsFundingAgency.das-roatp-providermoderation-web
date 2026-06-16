using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Roatp.ProviderModeration.Web.Infrastructure;
using SFA.DAS.Roatp.ProviderModeration.Web.Models;

namespace SFA.DAS.Roatp.ProviderModeration.Web.Controllers;

[AllowAnonymous]
public class ErrorController : Controller
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    [Route("Error/{statuscode}")]
    public IActionResult HttpStatusCodeHandler(int statusCode)
    {
        switch (statusCode)
        {
            case 403:
            case 404:
                return View("PageNotFound");
            default:
                ErrorViewModel errorViewModel = new()
                {
                    HomePageUrl = Url.RouteUrl(RouteNames.GetProviderDescription)!
                };
                return View("ErrorInService", errorViewModel);
        }
    }

    [Route("Error")]
    public IActionResult ErrorInService()
    {
        var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        ErrorViewModel errorViewModel = new()
        {
            HomePageUrl = Url.RouteUrl(RouteNames.GetProviderDescription)!
        };

        if (User.Identity!.IsAuthenticated)
        {
            _logger.LogError(feature!.Error, "Unexpected error occured during request to path: {Path} by user: {User}", feature.Path, HttpContext.User.FindFirstValue(ClaimTypes.Upn));
        }
        else
        {
            _logger.LogError(feature!.Error, "Unexpected error occured during request to {Path}", feature.Path);
        }
        return View(errorViewModel);
    }
}
