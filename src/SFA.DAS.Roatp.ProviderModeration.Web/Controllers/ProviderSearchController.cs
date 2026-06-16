using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Roatp.ProviderModeration.Application.Providers.Queries.GetProvider;
using SFA.DAS.Roatp.ProviderModeration.Domain.ApiModels;
using SFA.DAS.Roatp.ProviderModeration.Web.Configuration;
using SFA.DAS.Roatp.ProviderModeration.Web.Extensions;
using SFA.DAS.Roatp.ProviderModeration.Web.Infrastructure;
using SFA.DAS.Roatp.ProviderModeration.Web.Models;

namespace SFA.DAS.Roatp.ProviderModeration.Web.Controllers
{
    [Authorize(Roles = Roles.RoatpTribalTeam)]
    public class ProviderSearchController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProviderSearchController> _logger;
        private readonly IValidator<ProviderSearchSubmitModel> _validator;
        public const string ProviderNotAvailable = "This UKPRN could not be found. This is because the UKPRN does not exist or is inactive on the UK Register of Learning Providers.";
        public ProviderSearchController(IMediator mediator, ILogger<ProviderSearchController> logger, IValidator<ProviderSearchSubmitModel> validator)
        {
            _mediator = mediator;
            _logger = logger;
            _validator = validator;
        }

        [HttpGet]
        [Route("providers/provider-description", Name = RouteNames.GetProviderDescription)]
        public IActionResult Index()
        {
            return View("~/Views/ProviderSearch/Index.cshtml");
        }

        [HttpPost]
        [Route("providers/provider-description", Name = RouteNames.PostProviderDescription)]
        public async Task<IActionResult> GetProviderDescription(ProviderSearchSubmitModel submitModel)
        {
            _logger.LogInformation("Provider description gathering for {Ukprn}", submitModel.Ukprn);

            var validatedModel = await _validator.ValidateAsync(submitModel);

            if (!validatedModel.IsValid)
            {
                ModelState.AddValidationErrors(validatedModel.Errors);
            }

            if (!ModelState.IsValid)
            {
                return View("~/Views/ProviderSearch/Index.cshtml", submitModel);
            }
            try
            {
                var providerSearchResult = await _mediator.Send(new GetProviderQuery(int.Parse(submitModel.Ukprn)));

                if (providerSearchResult != null && (providerSearchResult.Provider.ProviderType != ProviderType.Main || providerSearchResult.Provider.ProviderStatusType == ProviderStatusType.ActiveButNotTakingOnApprentices))
                {
                    ModelState.AddModelError("Ukprn", ProviderNotAvailable);
                    return View("~/Views/ProviderSearch/Index.cshtml", submitModel);
                }
                TempData.Remove("ProviderDescription");
                return RedirectToRoute(RouteNames.GetProviderDetails, new { submitModel.Ukprn });
            }
            catch (InvalidOperationException)
            {
                _logger.LogError("Provider not found for ukprn {model.Ukprn}", submitModel.Ukprn);
                ModelState.AddModelError("Ukprn", ProviderNotAvailable);
                return View("~/Views/ProviderSearch/Index.cshtml", submitModel);
            }
        }
    }
}