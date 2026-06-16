using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Roatp.ProviderModeration.Application.Providers.Queries.GetProvider;
using SFA.DAS.Roatp.ProviderModeration.Domain.ApiModels;
using SFA.DAS.Roatp.ProviderModeration.Web.Controllers;
using SFA.DAS.Roatp.ProviderModeration.Web.Infrastructure;
using SFA.DAS.Roatp.ProviderModeration.Web.Models;

namespace SFA.DAS.Roatp.ProviderModeration.Web.UnitTests.Controllers.ProviderSearchControllerTests
{
    [TestFixture]
    public class ProviderSearchPostTests
    {
        private ProviderSearchController _sut;
        private Mock<ILogger<ProviderSearchController>> _logger;
        private Mock<IMediator> _mediator;
        private Mock<IUrlHelper> _urlHelperMock;
        private Mock<IValidator<ProviderSearchSubmitModel>> _validatorMock;

        public const string Ukprn = "12345678";
        public const string MarketingInfo = "Marketing info";
        readonly string verifyAddProviderDescriptionUrl = "http://test-AddProviderDescriptionUrl";

        [SetUp]
        public void Before_each_test()
        {
            _logger = new Mock<ILogger<ProviderSearchController>>();
            var provider = new GetProviderResponse
            {
                MarketingInfo = MarketingInfo,
                ProviderType = ProviderType.Main,
                ProviderStatusType = ProviderStatusType.Active,
            };
            _mediator = new Mock<IMediator>();
            _mediator.Setup(x => x.Send(It.IsAny<GetProviderQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new GetProviderQueryResult
                {
                    Provider = provider
                });

            _urlHelperMock = new Mock<IUrlHelper>();

            _urlHelperMock
               .Setup(m => m.RouteUrl(It.Is<UrlRouteContext>(c => c.RouteName.Equals(RouteNames.GetAddProviderDescription))))
               .Returns(verifyAddProviderDescriptionUrl);

            _validatorMock = new Mock<IValidator<ProviderSearchSubmitModel>>();

            _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<ProviderSearchSubmitModel>()))
                .ReturnsAsync(new ValidationResult());

            _sut = new ProviderSearchController(_mediator.Object, _logger.Object, _validatorMock.Object);
            _sut.Url = _urlHelperMock.Object;

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _sut.TempData = tempData;
        }

        [TestCase(ProviderType.Main, ProviderStatusType.Active, true)]
        [TestCase(ProviderType.Main, ProviderStatusType.Onboarding, true)]
        [TestCase(ProviderType.Main, ProviderStatusType.ActiveButNotTakingOnApprentices, false)]
        [TestCase(ProviderType.Supporting, ProviderStatusType.Active, false)]
        [TestCase(ProviderType.Employer, ProviderStatusType.Active, false)]
        [TestCase(ProviderType.Main, ProviderStatusType.ActiveButNotTakingOnApprentices, false)]
        public async Task ProviderController_GetProviderDescription_ReturnsResponse(ProviderType providerType, ProviderStatusType providerStatusType, bool isValidToDisplayProviderResponse)
        {
            var provider = new GetProviderResponse
            {
                MarketingInfo = MarketingInfo,
                ProviderType = providerType,
                ProviderStatusType = providerStatusType,
            };
            _mediator.Setup(x => x.Send(It.IsAny<GetProviderQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new GetProviderQueryResult
                {
                    Provider = provider
                });

            var submitModel = new ProviderSearchSubmitModel
            {
                Ukprn = Ukprn
            };

            var result = await _sut.GetProviderDescription(submitModel);

            if (isValidToDisplayProviderResponse)
            {
                var redirectResult = result as RedirectToRouteResult;
                redirectResult.Should().NotBeNull();
                redirectResult.RouteName.Should().Be(RouteNames.GetProviderDetails);
            }
            else
            {
                var viewResult = result as ViewResult;
                viewResult.Should().NotBeNull();
                viewResult?.ViewName.Should().Contain("ProviderSearch/Index.cshtml");
                viewResult?.Model.Should().NotBeNull();
            }
        }

        [Test]
        public async Task ProviderController_GetProviderDescription_ModelStateErrorReturnSameView()
        {
            var failedValidationResult = new ValidationResult
            {
                Errors = [new ValidationFailure("ProviderNotMainProvider", "ErrorMessage")]
            };

            _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<ProviderSearchSubmitModel>()))
                .ReturnsAsync(failedValidationResult);

            var model = new ProviderSearchSubmitModel
            {
                Ukprn = Ukprn
            };
            var result = await _sut.GetProviderDescription(model);

            var viewResult = result as ViewResult;
            viewResult?.Should().NotBeNull();
            viewResult?.ViewName.Should().Contain("ProviderSearch/Index.cshtml");
            viewResult?.Model.Should().NotBeNull();

            _mediator.Verify(m => m.Send(It.IsAny<GetProviderQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task ProviderController_GetProviderDescription_InvalidOperationExceptionWhenNoProvider()
        {
            _mediator.Setup(x => x.Send(It.IsAny<GetProviderQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException());

            _sut = new ProviderSearchController(_mediator.Object, _logger.Object, _validatorMock.Object);

            var model = new ProviderSearchSubmitModel
            {
                Ukprn = Ukprn
            };
            var result = await _sut.GetProviderDescription(model);

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult?.ViewName.Should().Contain("ProviderSearch/Index.cshtml");
            viewResult?.Model.Should().NotBeNull();

            _mediator.Verify(m => m.Send(It.IsAny<GetProviderQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
