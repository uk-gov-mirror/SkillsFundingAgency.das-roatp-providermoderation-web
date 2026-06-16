using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Roatp.ProviderModeration.Application.Providers.Queries.GetProvider;
using SFA.DAS.Roatp.ProviderModeration.Web.Configuration;
using SFA.DAS.Roatp.ProviderModeration.Web.Extensions;
using SFA.DAS.Roatp.ProviderModeration.Web.Infrastructure;
using SFA.DAS.Roatp.ProviderModeration.Web.Models;

namespace SFA.DAS.Roatp.ProviderModeration.Web.Controllers
{
    [Authorize(Roles = Roles.RoatpTribalTeam)]
    public class ProviderDescriptionUpdateController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProviderDescriptionUpdateController> _logger;
        private readonly IValidator<ProviderDescriptionSubmitModel> _validator;
        public const string ViewPath = "~/Views/ProviderSearch/ProviderDescriptionUpdate.cshtml";
        public ProviderDescriptionUpdateController(IMediator mediator, ILogger<ProviderDescriptionUpdateController> logger, IValidator<ProviderDescriptionSubmitModel> validator)
        {
            _mediator = mediator;
            _logger = logger;
            _validator = validator;
        }

        [HttpGet]
        [Route("providers/{ukprn}/update-provider-description", Name = RouteNames.GetUpdateProviderDescription)]
        public async Task<IActionResult> Index([FromRoute] int ukprn)
        {
            var providerSearchResult = await _mediator.Send(new GetProviderQuery(ukprn));
            var providerDescriptionUpdateViewModel = new ProviderDescriptionUpdateViewModel
            {
                Ukprn = ukprn,
                LegalName = providerSearchResult.Provider.LegalName,
                ExistingProviderDescription = providerSearchResult.Provider.MarketingInfo,
                ProviderDescription = TempData.ContainsKey("ProviderDescription") ? (string)TempData["ProviderDescription"] : string.Empty,
                CancelLink = Url.RouteUrl(RouteNames.GetProviderDetails, new { ukprn = ukprn })
            };
            return View(ViewPath, providerDescriptionUpdateViewModel);
        }

        [HttpPost]
        [Route("providers/{ukprn}/update-provider-description", Name = RouteNames.PostUpdateProviderDescription)]
        public IActionResult UpdateProviderDescription(ProviderDescriptionSubmitModel submitModel)
        {
            _logger.LogInformation("Provider description updating for {Ukprn}", submitModel.Ukprn);

            var validatedModel = _validator.Validate(submitModel);

            if (!validatedModel.IsValid)
            {
                ModelState.AddValidationErrors(validatedModel.Errors);
            }

            if (!ModelState.IsValid)
            {
                var model = new ProviderDescriptionUpdateViewModel()
                {
                    Ukprn = submitModel.Ukprn,
                    LegalName = submitModel.LegalName,
                    ProviderDescription = submitModel.ProviderDescription,
                    ExistingProviderDescription = submitModel.ExistingProviderDescription,
                    CancelLink = Url.RouteUrl(RouteNames.GetProviderDetails, new { ukprn = submitModel.Ukprn })
                };
                return View(ViewPath, model);
            }

            TempData["ProviderDescription"] = submitModel.ProviderDescription;
            return RedirectToRoute(RouteNames.GetReviewProviderDescription, new { submitModel.Ukprn });
        }
    }
}