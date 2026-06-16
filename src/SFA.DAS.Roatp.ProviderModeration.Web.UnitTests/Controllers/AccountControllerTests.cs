using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Roatp.ProviderModeration.Web.Configuration;
using SFA.DAS.Roatp.ProviderModeration.Web.Controllers;
using SFA.DAS.Roatp.ProviderModeration.Web.UnitTests.MockedObjects;

namespace SFA.DAS.Roatp.ProviderModeration.Web.UnitTests.Controllers
{
    [TestFixture]
    public class AccountControllerTests
    {
        private AccountController _controller;
        private Mock<IOptions<ApplicationConfiguration>> _configurationMock;

        [SetUp]
        public void Setup()
        {
            _configurationMock = new Mock<IOptions<ApplicationConfiguration>>();
            _configurationMock.Setup(x => x.Value.UseDfeSignIn).Returns(true);
            _configurationMock.Setup(x => x.Value.DfESignInServiceHelpUrl).Returns("test");
            _controller = new AccountController(Mock.Of<ILogger<AccountController>>(), _configurationMock.Object)
            {
                ControllerContext = MockedControllerContext.Setup(),
                Url = Mock.Of<IUrlHelper>()
            };
        }

        [Test]
        public void SignIn_returns_expected_ChallengeResult()
        {
            _configurationMock.Setup(x => x.Value.UseDfeSignIn).Returns(false);

            var result = _controller.SignIn() as ChallengeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.AuthenticationSchemes, Is.Not.Empty);
            Assert.That(result.AuthenticationSchemes.Contains(WsFederationDefaults.AuthenticationScheme));
        }

        [Test]
        public void SignIn_returns_expected_ChallengeResult_DfeSignIn()
        {
            _configurationMock.Setup(x => x.Value.UseDfeSignIn).Returns(true);

            var result = _controller.SignIn() as ChallengeResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.AuthenticationSchemes, Is.Not.Empty);
            Assert.That(result.AuthenticationSchemes.Contains(OpenIdConnectDefaults.AuthenticationScheme));
        }

        [Test]
        public void PostSignIn_redirects_to_Home()
        {
            var result = _controller.PostSignIn() as RedirectToActionResult;

            Assert.AreEqual("Home", result?.ControllerName);
            Assert.AreEqual("Index", result?.ActionName);
        }

        [Test]
        public void SignOut_returns_expected_SignOutResult()
        {
            _configurationMock.Setup(x => x.Value.UseDfeSignIn).Returns(false);

            var result = _controller.SignOut() as SignOutResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.AuthenticationSchemes, Is.Not.Empty);
            Assert.That(result.AuthenticationSchemes.Contains(WsFederationDefaults.AuthenticationScheme));
            Assert.That(result.AuthenticationSchemes.Contains(CookieAuthenticationDefaults.AuthenticationScheme));
        }

        [Test]
        public void SignOut_returns_expected_SignOutResult_DfeSignIn()
        {
            _configurationMock.Setup(x => x.Value.UseDfeSignIn).Returns(true);

            var result = _controller.SignOut() as SignOutResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.AuthenticationSchemes, Is.Not.Empty);
            Assert.That(result.AuthenticationSchemes.Contains(OpenIdConnectDefaults.AuthenticationScheme));
            Assert.That(result.AuthenticationSchemes.Contains(CookieAuthenticationDefaults.AuthenticationScheme));
        }

        [Test]
        public void SignedOut_shows_correct_view()
        {
            var result = _controller.SignedOut() as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.AreEqual("SignedOut", result?.ViewName);
        }

        [Test]
        public void AccessDenied_shows_correct_view()
        {
            var result = _controller.AccessDenied() as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.AreEqual("AccessDenied", result?.ViewName);
        }
    }
}
