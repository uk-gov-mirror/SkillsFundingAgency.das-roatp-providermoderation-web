using AutoFixture.NUnit4;
using FluentAssertions;
using FluentValidation;
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

namespace SFA.DAS.Roatp.ProviderModeration.Web.UnitTests.Controllers.ProviderDescriptionAddControllerTests
{
    [TestFixture]
    public class ProviderDescriptionAddControllerGetTests
    {
        private Mock<IMediator> _mediatorMock;
        private ProviderDescriptionAddController _sut;
        private Mock<IUrlHelper> _urlHelperMock;
        string verifyUrl = "http://test";

        [SetUp]
        public void Before_Each_Test()
        {
            _mediatorMock = new Mock<IMediator>();

            _urlHelperMock = new Mock<IUrlHelper>();

            _urlHelperMock
               .Setup(m => m.RouteUrl(It.Is<UrlRouteContext>(c => c.RouteName.Equals(RouteNames.GetProviderDetails))))
               .Returns(verifyUrl);

            _sut = new ProviderDescriptionAddController(_mediatorMock.Object, Mock.Of<ILogger<ProviderDescriptionAddController>>(), Mock.Of<IValidator<ProviderDescriptionSubmitModel>>());
            _sut.Url = _urlHelperMock.Object;
        }

        [Test, AutoData]
        public async Task Index_ValidRequest_ReturnsView(
            GetProviderQueryResult providerQueryResult,
            int ukprn)
        {
            _mediatorMock
                .Setup(m => m.Send(It.Is<GetProviderQuery>(q => q.Ukprn == ukprn), It.IsAny<CancellationToken>()))
                .ReturnsAsync(providerQueryResult);

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _sut.TempData = tempData;

            var result = await _sut.Index(ukprn);

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult?.ViewName.Should().Contain(ProviderDescriptionAddController.ViewPath);
            var model = viewResult.Model as ProviderDescriptionAddViewModel;
            model.Should().NotBeNull();
            model.CancelLink.Should().Be(verifyUrl);
        }
    }
}
