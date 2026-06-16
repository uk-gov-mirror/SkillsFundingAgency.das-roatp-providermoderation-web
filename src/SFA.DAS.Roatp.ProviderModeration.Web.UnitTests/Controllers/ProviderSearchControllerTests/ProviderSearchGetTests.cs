using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Roatp.ProviderModeration.Web.Controllers;
using SFA.DAS.Roatp.ProviderModeration.Web.Models;

namespace SFA.DAS.Roatp.ProviderModeration.Web.UnitTests.Controllers.ProviderSearchControllerTests
{
    [TestFixture]
    public class ProviderSearchGetTests
    {
        private ProviderSearchController _controller;
        private Mock<ILogger<ProviderSearchController>> _logger;
        private Mock<IMediator> _mediator;


        [SetUp]
        public void Before_each_test()
        {
            _logger = new Mock<ILogger<ProviderSearchController>>();

            _mediator = new Mock<IMediator>();

            _controller = new ProviderSearchController(_mediator.Object, _logger.Object, Mock.Of<IValidator<ProviderSearchSubmitModel>>());
        }

        [Test]
        public void ProviderController_Index_ReturnsView()
        {
            var result = _controller.Index();

            var viewResult = result as ViewResult;
            viewResult.Should().NotBeNull();
            viewResult?.ViewName.Should().Contain("ProviderSearch/Index.cshtml");
            viewResult?.Model.Should().BeNull();
        }
    }
}
