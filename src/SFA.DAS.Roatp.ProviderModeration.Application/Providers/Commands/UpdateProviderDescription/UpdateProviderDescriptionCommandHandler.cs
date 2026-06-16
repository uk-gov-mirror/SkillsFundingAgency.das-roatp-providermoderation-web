using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.Roatp.ProviderModeration.Domain.Interfaces;

namespace SFA.DAS.Roatp.ProviderModeration.Application.Providers.Commands.UpdateProviderDescription
{
    public class UpdateProviderDescriptionCommandHandler : IRequestHandler<UpdateProviderDescriptionCommand, Unit>
    {
        private readonly ILogger<UpdateProviderDescriptionCommandHandler> _logger;
        private readonly IApiClient _apiClient;

        public UpdateProviderDescriptionCommandHandler(ILogger<UpdateProviderDescriptionCommandHandler> logger, IApiClient apiClient)
        {
            _logger = logger;
            _apiClient = apiClient;
        }


        public async Task<Unit> Handle(UpdateProviderDescriptionCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Command triggered to update provider description for ukprn:{Ukprn} from user:{Userid}", command.Ukprn, command.UserId);

            var statusCode = await _apiClient.Post($"providers/{command.Ukprn}/update-provider-description", command);

            if (statusCode != System.Net.HttpStatusCode.NoContent)
            {
                _logger.LogError("Failed to update provider description for ukprn:{Ukprn} from user:{Userid}", command.Ukprn, command.UserId);
                throw new InvalidOperationException("Update provider description response did not come back with success code");
            }

            return Unit.Value;
        }
    }
}
