using System.Net;
using AutoFixture.NUnit4;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Roatp.ProviderModeration.Application.Providers.Commands.UpdateProviderDescription;
using SFA.DAS.Roatp.ProviderModeration.Domain.Interfaces;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Roatp.ProviderModeration.Application.UnitTests.Providers.Commands
{
    [TestFixture]
    public class UpdateProviderDescriptionCommandHandlerTests
    {
        [Test, MoqAutoData]
        public async Task Handle_OuterApiCallSuccess_ReturnsUnit(
            [Frozen] Mock<IApiClient> apiClientMock,
            UpdateProviderDescriptionCommandHandler sut,
            UpdateProviderDescriptionCommand command,
            CancellationToken cancellationToken)
        {
            var expectedUri = $"providers/{command.Ukprn}/update-provider-description";
            apiClientMock.Setup(c => c.Post(expectedUri, command)).ReturnsAsync(HttpStatusCode.NoContent);

            var result = await sut.Handle(command, cancellationToken);

            apiClientMock.Verify(c => c.Post(expectedUri, command));
            result.Should().BeEquivalentTo(Unit.Value);
        }

        [Test, MoqAutoData]
        public async Task Handle_OuterApiCallFails_ThrowsInvalidOperationException(
            [Frozen] Mock<IApiClient> apiClientMock,
            UpdateProviderDescriptionCommandHandler sut,
            UpdateProviderDescriptionCommand command,
            CancellationToken cancellationToken)
        {
            var expectedUri = $"providers/{command.Ukprn}/update-provider-description";
            apiClientMock.Setup(c => c.Post(expectedUri, command)).ReturnsAsync(HttpStatusCode.BadRequest);

            Func<Task> act = () => sut.Handle(command, cancellationToken);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
