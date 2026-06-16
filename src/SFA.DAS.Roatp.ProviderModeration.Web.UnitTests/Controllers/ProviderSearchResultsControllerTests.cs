using AutoFixture.NUnit4;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Roatp.ProviderModeration.Application.Providers.Queries.GetProvider;
using SFA.DAS.Roatp.ProviderModeration.Web.Controllers;
using SFA.DAS.Roatp.ProviderModeration.Web.Infrastructure;
using SFA.DAS.Roatp.ProviderModeration.Web.Models;

namespace SFA.DAS.Roatp.ProviderModeration.Web.UnitTests.Controllers.ProviderSearchResultsControllerTests
{
    [TestFixture]
    public class ProviderSearchResultsControllerTests
    {
        private Mock<IMediator> _mediatorMock;
        private ProviderSearchResultsController _sut;
        private Mock<IUrlHelper> _urlHelperMock;
        string addProviderDescriptionUrl = "http://test/AddProviderDescriptionUrl";
        string updateProviderDescriptionUrl = "http://test/UpdateProviderDescriptionUrl";

        [SetUp]
        public void Before_Each_Test()
        {
            _mediatorMock = new Mock<IMediator>();

            _urlHelperMock = new Mock<IUrlHelper>();

            _urlHelperMock
               .Setup(m => m.RouteUrl(It.Is<UrlRouteContext>(c => c.RouteName.Equals(RouteNames.GetAddProviderDescription))))
               .Returns(addProviderDescriptionUrl);

            _urlHelperMock
               .Setup(m => m.RouteUrl(It.Is<UrlRouteContext>(c => c.RouteName.Equals(RouteNames.GetUpdateProviderDescription))))
               .Returns(updateProviderDescriptionUrl);

            _sut = new ProviderSearchResultsController(_mediatorMock.Object, Mock.Of<ILogger<ProviderSearchResultsController>>());
            _sut.Url = _urlHelperMock.Object;

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _sut.TempData = tempData;
        }

        [Test, AutoData]
        public async Task GetProvider_ValidRequest_ReturnsView(
            GetProviderQueryResult providerQueryResult,
            int ukprn)
        {
            _mediatorMock
                .Setup(m => m.Send(It.Is<GetProviderQuery>(q => q.Ukprn == ukprn), It.IsAny<CancellationToken>()))
                .ReturnsAsync(providerQueryResult);

            var result = await _sut.GetProvider(ukprn);

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult?.ViewName.Should().Contain("ProviderSearch/SearchResults.cshtml");
            var model = viewResult.Model as ProviderSearchResultViewModel;
            model.Should().NotBeNull();
            model.AddProviderDescriptionLink.Should().Be(addProviderDescriptionUrl);
            model.ChangeProviderDescriptionLink.Should().Be(updateProviderDescriptionUrl);
        }
    }
}
