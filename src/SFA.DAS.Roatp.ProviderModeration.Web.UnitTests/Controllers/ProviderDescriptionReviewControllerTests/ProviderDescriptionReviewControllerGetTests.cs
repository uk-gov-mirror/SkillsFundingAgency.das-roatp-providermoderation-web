using AutoFixture.NUnit4;
using FluentAssertions;
using FluentValidation;
using MediatR;
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

namespace SFA.DAS.Roatp.ProviderModeration.Web.UnitTests.Controllers.ProviderDescriptionReviewControllerTests
{
    [TestFixture]
    public class ProviderDescriptionReviewControllerGetTests
    {
        private Mock<IMediator> _mediatorMock;
        private ProviderDescriptionReviewController _sut;
        private Mock<IUrlHelper> _urlHelperMock;
        string verifyUrl = "http://test";
        string verifyEditUrl = "http://test-edit";

        [SetUp]
        public void Before_Each_Test()
        {
            _mediatorMock = new Mock<IMediator>();

            _urlHelperMock = new Mock<IUrlHelper>();

            _urlHelperMock
               .Setup(m => m.RouteUrl(It.Is<UrlRouteContext>(c => c.RouteName.Equals(RouteNames.GetProviderDetails))))
               .Returns(verifyUrl);

            _urlHelperMock
               .Setup(m => m.RouteUrl(It.Is<UrlRouteContext>(c => c.RouteName.Equals(RouteNames.GetReviewProviderDescriptionEdit))))
               .Returns(verifyEditUrl);

            _sut = new ProviderDescriptionReviewController(_mediatorMock.Object, Mock.Of<ILogger<ProviderDescriptionReviewController>>(), Mock.Of<IValidator<ProviderDescriptionReviewViewModel>>());
            _sut.Url = _urlHelperMock.Object;

            var tempDataMock = new Mock<ITempDataDictionary>();
            _sut.TempData = tempDataMock.Object;
            object providerDescriptionTempData = "Test";
            tempDataMock.Setup(t => t.TryGetValue("ProviderDescription", out providerDescriptionTempData));
        }

        [Test, AutoData]
        public async Task Index_ValidRequest_ReturnsView(
            GetProviderQueryResult providerQueryResult,
            int ukprn)
        {
            _mediatorMock
                .Setup(m => m.Send(It.Is<GetProviderQuery>(q => q.Ukprn == ukprn), It.IsAny<CancellationToken>()))
                .ReturnsAsync(providerQueryResult);

            var result = await _sut.Index(ukprn);

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult?.ViewName.Should().Contain(ProviderDescriptionReviewController.ViewPath);
            var model = viewResult.Model as ProviderDescriptionReviewViewModel;
            model.Should().NotBeNull();
            model.CancelLink.Should().Be(verifyUrl);
            model.EditEntry.Should().Be(verifyEditUrl);
        }

        [Test, AutoData]
        public async Task Index_ValidRequestInvalidTempData_RedirectToGetProviderDescription(
           GetProviderQueryResult providerQueryResult,
           int ukprn)
        {
            var tempDataMock = new Mock<ITempDataDictionary>();
            _sut.TempData = tempDataMock.Object;
            object providerDescriptionTempData = null;
            tempDataMock.Setup(t => t.TryGetValue("ProviderDescription", out providerDescriptionTempData));

            _mediatorMock
                .Setup(m => m.Send(It.Is<GetProviderQuery>(q => q.Ukprn == ukprn), It.IsAny<CancellationToken>()))
                .ReturnsAsync(providerQueryResult);

            var result = await _sut.Index(ukprn);

            var redirectResult = result as RedirectToRouteResult;
            redirectResult.Should().NotBeNull();
            redirectResult.RouteName.Should().Be(RouteNames.GetProviderDescription);
        }

        [Test, AutoData]
        public async Task EditEntry_ValidRequestAddProviderDescription_ReturnsAddProviderDescriptionView(
           int ukprn)
        {
            _mediatorMock
                .Setup(m => m.Send(It.Is<GetProviderQuery>(q => q.Ukprn == ukprn), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetProviderQueryResult { Provider = new GetProviderResponse { MarketingInfo = "", ProviderType = ProviderType.Main, ProviderStatusType = ProviderStatusType.Onboarding } });

            var result = await _sut.EditEntry(ukprn);

            var redirectResult = result as RedirectToRouteResult;
            redirectResult.Should().NotBeNull();
            redirectResult.RouteName.Should().Be(RouteNames.GetAddProviderDescription);
        }

        [Test, AutoData]
        public async Task EditEntry_ValidRequestUpdateProviderDescription_ReturnsUpdateProviderDescriptionView(
           int ukprn)
        {
            _mediatorMock
               .Setup(m => m.Send(It.Is<GetProviderQuery>(q => q.Ukprn == ukprn), It.IsAny<CancellationToken>()))
               .ReturnsAsync(new GetProviderQueryResult { Provider = new GetProviderResponse { MarketingInfo = "Test", ProviderType = ProviderType.Main, ProviderStatusType = ProviderStatusType.Active } });

            var result = await _sut.EditEntry(ukprn);

            var redirectResult = result as RedirectToRouteResult;
            redirectResult.Should().NotBeNull();
            redirectResult.RouteName.Should().Be(RouteNames.GetUpdateProviderDescription);
        }

        [Test, AutoData]
        public async Task EditEntry_InValidTempDataRequest_ReturnsGetProviderDetailsView(
          int ukprn)
        {
            var tempDataMock = new Mock<ITempDataDictionary>();
            _sut.TempData = tempDataMock.Object;
            object providerDescriptionTempData = null;
            tempDataMock.Setup(t => t.TryGetValue("ProviderDescription", out providerDescriptionTempData));

            var result = await _sut.EditEntry(ukprn);

            var redirectResult = result as RedirectToRouteResult;
            redirectResult.Should().NotBeNull();
            redirectResult.RouteName.Should().Be(RouteNames.GetProviderDescription);
        }
    }
}
