using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Roatp.ProviderModeration.Application.Providers.Commands.UpdateProviderDescription;
using SFA.DAS.Roatp.ProviderModeration.Application.Providers.Queries.GetProvider;
using SFA.DAS.Roatp.ProviderModeration.Web.Configuration;
using SFA.DAS.Roatp.ProviderModeration.Web.Extensions;
using SFA.DAS.Roatp.ProviderModeration.Web.Infrastructure;
using SFA.DAS.Roatp.ProviderModeration.Web.Models;

namespace SFA.DAS.Roatp.ProviderModeration.Web.Controllers
{
    [Authorize(Roles = Roles.RoatpTribalTeam)]
    public class ProviderDescriptionReviewController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProviderDescriptionReviewController> _logger;
        private readonly IValidator<ProviderDescriptionReviewViewModel> _validator;
        public const string ViewPath = "~/Views/ProviderSearch/ProviderDescriptionReview.cshtml";
        public ProviderDescriptionReviewController(IMediator mediator, ILogger<ProviderDescriptionReviewController> logger, IValidator<ProviderDescriptionReviewViewModel> validator)
        {
            _mediator = mediator;
            _logger = logger;
            _validator = validator;
        }

        [HttpGet]
        [Route("providers/{ukprn}/review-provider-description", Name = RouteNames.GetReviewProviderDescription)]
        public async Task<IActionResult> Index([FromRoute] int ukprn)
        {
            TempData.TryGetValue("ProviderDescription", out var providerDescriptionTempData);
            if (providerDescriptionTempData == null)
            {
                return RedirectToRoute(RouteNames.GetProviderDescription);
            }
            TempData.Keep("ProviderDescription");

            var providerSearchResult = await _mediator.Send(new GetProviderQuery(ukprn));

            var providerDescriptionReviewViewModel = new ProviderDescriptionReviewViewModel
            {
                Ukprn = ukprn,
                LegalName = providerSearchResult.Provider.LegalName,
                ProviderDescription = (string)providerDescriptionTempData,
                CancelLink = Url.RouteUrl(RouteNames.GetProviderDetails, new { ukprn }),
                EditEntry = Url.RouteUrl(RouteNames.GetReviewProviderDescriptionEdit, new { ukprn }),
            };
            return View(ViewPath, providerDescriptionReviewViewModel);
        }

        [HttpGet]
        [Route("providers/{ukprn}/review-provider-description-edit", Name = RouteNames.GetReviewProviderDescriptionEdit)]
        public async Task<IActionResult> EditEntry([FromRoute] int ukprn)
        {
            TempData.TryGetValue("ProviderDescription", out var providerDescriptionTempData);
            if (providerDescriptionTempData == null)
            {
                return RedirectToRoute(RouteNames.GetProviderDescription);
            }
            TempData.Keep("ProviderDescription");

            var providerSearchResult = await _mediator.Send(new GetProviderQuery(ukprn));

            return string.IsNullOrEmpty(providerSearchResult?.Provider?.MarketingInfo)
                ? RedirectToRoute(RouteNames.GetAddProviderDescription, new { ukprn })
                : RedirectToRoute(RouteNames.GetUpdateProviderDescription, new { ukprn });
        }

        [HttpPost]
        [Route("providers/{ukprn}/review-provider-description", Name = RouteNames.PostReviewProviderDescription)]
        public async Task<IActionResult> ReviewProviderDescription(ProviderDescriptionReviewViewModel submitModel)
        {
            var validatedModel = await _validator.ValidateAsync(submitModel);

            if (!validatedModel.IsValid)
            {
                ModelState.AddValidationErrors(validatedModel.Errors);
            }

            if (!ModelState.IsValid)
            {
                return RedirectToRoute(RouteNames.GetProviderDescription);
            }

            _logger.LogInformation("Provider description updating for {Ukprn}", submitModel.Ukprn);
            TempData.Remove("ProviderDescription");

            var command = new UpdateProviderDescriptionCommand
            {
                Ukprn = submitModel.Ukprn,
                UserId = UserId,
                UserDisplayName = UserDisplayName,
                ProviderDescription = submitModel.ProviderDescription
            };

            await _mediator.Send(command);

            return RedirectToRoute(RouteNames.GetProviderDetails, new { submitModel.Ukprn });
        }
    }
}