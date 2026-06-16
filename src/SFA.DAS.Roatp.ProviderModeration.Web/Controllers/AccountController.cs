using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SFA.DAS.Roatp.ProviderModeration.Web.Configuration;
using SFA.DAS.Roatp.ProviderModeration.Web.Models;

namespace SFA.DAS.Roatp.ProviderModeration.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationConfiguration _applicationConfiguration;

        public AccountController(ILogger<AccountController> logger, IOptions<ApplicationConfiguration> applicationConfiguration)
        {
            _logger = logger;
            _applicationConfiguration = applicationConfiguration.Value;
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            _logger.LogInformation("Start of Sign In");
            var challengeScheme = _applicationConfiguration.UseDfeSignIn
                ? OpenIdConnectDefaults.AuthenticationScheme
                : WsFederationDefaults.AuthenticationScheme;
            var redirectUrl = Url.Action("PostSignIn", "Account");
            return Challenge(
                new AuthenticationProperties { RedirectUri = redirectUrl },
                challengeScheme);
        }

        [HttpGet]
        public IActionResult PostSignIn()
        {
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult SignOut()
        {
            var callbackUrl = Url.Action("SignedOut", "Account", values: null, protocol: Request.Scheme);

            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            var authScheme = _applicationConfiguration.UseDfeSignIn
                ? OpenIdConnectDefaults.AuthenticationScheme
                : WsFederationDefaults.AuthenticationScheme;

            return SignOut(
                new AuthenticationProperties
                {
                    RedirectUri = callbackUrl,
                    AllowRefresh = true
                },
                CookieAuthenticationDefaults.AuthenticationScheme,
                authScheme);
        }

        [HttpGet]
        public IActionResult SignedOut()
        {
            return View("SignedOut");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            if (HttpContext.User != null)
            {
                var userName = HttpContext.User.Identity.Name ?? HttpContext.User.FindFirstValue(ClaimTypes.Upn);
                var roles = HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.Role || c.Type == Roles.RoleClaimType).Select(c => c.Value);

                _logger.LogError("AccessDenied - User '{UserName}' does not have a valid role. They have the following roles: {Roles}", userName, string.Join(",", roles));
            }

            var model = new Error403ViewModel
            {
                UseDfESignIn = _applicationConfiguration.UseDfeSignIn,
                HelpPageLink = _applicationConfiguration.DfESignInServiceHelpUrl
            };

            return View("AccessDenied", model);
        }
    }
}
