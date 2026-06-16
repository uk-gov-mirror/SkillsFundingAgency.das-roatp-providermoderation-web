
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
using SFA.DAS.Roatp.ProviderModeration.Web.Controllers;
using SFA.DAS.Roatp.ProviderModeration.Web.Infrastructure;
using SFA.DAS.Roatp.ProviderModeration.Web.Models;
using SFA.DAS.Roatp.ProviderModeration.Web.UnitTests.TestHelpers;

namespace SFA.DAS.Roatp.ProviderModeration.Web.UnitTests.Controllers.ProviderDescriptionReviewControllerTests
{
    [TestFixture]
    public class ProviderDescriptionReviewControllerPostTests
    {
        private Mock<IMediator> _mediatorMock;
        private ProviderDescriptionReviewController _sut;
        private Mock<IUrlHelper> _urlHelperMock;
        private Mock<IValidator<ProviderDescriptionReviewViewModel>> _validatorMock;
        readonly string verifyUrl = "http://test";
        readonly string verifyEditUrl = "http://test-edit";
        public const int Ukprn = 12345678;
        public const string LegalName = "TestLegalName";
        public const string ProviderDescription = "TestProviderDescription";

        [SetUp]
        public void Before_Each_Test()
        {
            _mediatorMock = new Mock<IMediator>();

            _validatorMock = new Mock<IValidator<ProviderDescriptionReviewViewModel>>();

            _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<ProviderDescriptionReviewViewModel>()))
                .ReturnsAsync(new ValidationResult());

            _sut = new ProviderDescriptionReviewController(_mediatorMock.Object, Mock.Of<ILogger<ProviderDescriptionReviewController>>(), _validatorMock.Object);
            _sut.AddDefaultContextWithUser();
            _urlHelperMock = new Mock<IUrlHelper>();
            _sut.Url = _urlHelperMock.Object;

            _urlHelperMock
                .Setup(m => m.RouteUrl(It.Is<UrlRouteContext>(c => c.RouteName.Equals(RouteNames.GetProviderDetails))))
                .Returns(verifyUrl);

            _urlHelperMock
               .Setup(m => m.RouteUrl(It.Is<UrlRouteContext>(c => c.RouteName.Equals(RouteNames.GetReviewProviderDescriptionEdit))))
               .Returns(verifyEditUrl);

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _sut.TempData = tempData;
        }

        [Test]
        public async Task ReviewProviderDescription_ValidResponseReturnsSameView()
        {
            var submitModel = new ProviderDescriptionReviewViewModel
            {
                Ukprn = Ukprn,
                LegalName = LegalName,
                ProviderDescription = ProviderDescription
            };

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            tempData["ProviderDescription"] = submitModel.ProviderDescription;
            _sut.TempData = tempData;

            var result = await _sut.ReviewProviderDescription(submitModel);

            var redirectResult = result as RedirectToRouteResult;
            redirectResult.Should().NotBeNull();
            redirectResult.RouteName.Should().Be(RouteNames.GetProviderDetails);
        }

        [Test]
        public async Task ReviewProviderDescription_InValidModelStateResponseRedirectToGetProviderDescription()
        {
            var submitModel = new ProviderDescriptionReviewViewModel
            {
                Ukprn = Ukprn,
                LegalName = LegalName,
                ProviderDescription = ProviderDescription
            };

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _sut.TempData = tempData;

            var failedValidationResult = new ValidationResult
            {
                Errors = [new ValidationFailure("key", "message")]
            };
            _validatorMock.Setup(x => x.ValidateAsync(It.IsAny<ProviderDescriptionReviewViewModel>()))
                .ReturnsAsync(failedValidationResult);

            var result = await _sut.ReviewProviderDescription(submitModel);

            var redirectResult = result as RedirectToRouteResult;
            redirectResult.Should().NotBeNull();
            redirectResult.RouteName.Should().Be(RouteNames.GetProviderDescription);
        }
    }
}
