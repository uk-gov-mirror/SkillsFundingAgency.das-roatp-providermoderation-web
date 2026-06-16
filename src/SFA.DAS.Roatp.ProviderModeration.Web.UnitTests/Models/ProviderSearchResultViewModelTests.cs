using AutoFixture.NUnit4;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Roatp.ProviderModeration.Application.Providers.Queries.GetProvider;
using SFA.DAS.Roatp.ProviderModeration.Domain.ApiModels;
using SFA.DAS.Roatp.ProviderModeration.Web.Models;

namespace SFA.DAS.Roatp.ProviderModeration.Web.UnitTests.Models
{
    [TestFixture]
    public class ProviderSearchResultViewModelTests
    {
        [Test, AutoData]
        public void ImplicitOperatorForApiModel_ReturnsViewModel(GetProviderQueryResult queryResult)
        {
            queryResult.Provider.ProviderStatusType = ProviderStatusType.Active;

            var vm = (ProviderSearchResultViewModel)queryResult;

            vm.Ukprn.Should().Be(queryResult.Provider.Ukprn);
            vm.LegalName.Should().Be(queryResult.Provider.LegalName);
            vm.ProviderDescription.Should().Be(queryResult.Provider.MarketingInfo);
            vm.ProviderType.Should().Be(queryResult.Provider.ProviderType);
            vm.ProviderStatusType.Should().Be(queryResult.Provider.ProviderStatusType);
            vm.ProviderStatusUpdatedDate.Should().Be(queryResult.Provider.ProviderStatusUpdatedDate);
            vm.IsProviderHasStandard.Should().Be(queryResult.Provider.IsProviderHasStandard);
        }

        [TestCase(ProviderStatusType.Active, "TestDescription", false, "Submitted")]
        [TestCase(ProviderStatusType.ActiveButNotTakingOnApprentices, "TestDescription", false, "Submitted")]
        [TestCase(ProviderStatusType.Onboarding, "TestDescription", false, "Submitted")]
        [TestCase(ProviderStatusType.Onboarding, "", true, "Not submitted")]
        public void ImplicitOperatorForApiModel_ReturnsViewModelLinks(ProviderStatusType providerStatusType, string providerDescription, bool showAddLink, string providerDescriptionStatus)
        {
            var queryResult = new GetProviderQueryResult { Provider = new GetProviderResponse() };
            queryResult.Provider.ProviderStatusType = providerStatusType;
            queryResult.Provider.MarketingInfo = providerDescription;

            var vm = (ProviderSearchResultViewModel)queryResult;

            vm.ShowAddLink.Should().Be(showAddLink);
            vm.ProviderDescriptionStatus.Should().Be(providerDescriptionStatus);
        }
    }
}
