using AutoFixture.NUnit4;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Roatp.ProviderModeration.Application.Providers.Queries.GetProvider;

namespace SFA.DAS.Roatp.ProviderModeration.Application.UnitTests.Providers.Queries.GetProvider
{
    [TestFixture]
    public class GetProviderQueryTests
    {
        [Test, AutoData]
        public void Constructor_SetsEntityProperty(int ukprn)
        {
            var sut = new GetProviderQuery(ukprn);

            sut.Should().NotBeNull();
            sut.Ukprn.Should().Be(ukprn);
        }
    }
}
