using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Orchestration.Response;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions;
using FoundationaLLM.Gatekeeper.Core.Models.ContentSafety;
using FoundationaLLM.Gatekeeper.Core.Services;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Gatekeeper.Tests.Services
{
    public class GatekeeperServiceTests
    {
        private readonly string _instanceId = "00000000-0000-0000-0000-000000000000";
        private readonly GatekeeperService _testedService;

        private readonly IContentSafetyService _contentSafetyService = Substitute.For<IContentSafetyService>();
        private readonly IAzureCosmosDBService _azureCosmosDBService = Substitute.For<IAzureCosmosDBService>();
        private readonly ILakeraGuardService _lakeraGuardService = Substitute.For<ILakeraGuardService>();
        private readonly IEnkryptGuardrailsService _enkryptGuardrailsService = Substitute.For<IEnkryptGuardrailsService>();
        private readonly IDownstreamAPIService _orchestrationAPIService = Substitute.For<IDownstreamAPIService>();
        private readonly IGatekeeperIntegrationAPIService _gatekeeperIntegrationAPIService = Substitute.For<IGatekeeperIntegrationAPIService>();
        private IOptions<GatekeeperServiceSettings> _gatekeeperServiceSettings;

        public GatekeeperServiceTests()
        {
            _gatekeeperServiceSettings = Options.Create(new GatekeeperServiceSettings
            {
                EnableAzureContentSafety = true,
                EnableMicrosoftPresidio = true,
                EnableAzureContentSafetyPromptShield = true,
                EnableLakeraGuard = true,
                EnableEnkryptGuardrails = true,
            });

            _testedService = new GatekeeperService(
                _orchestrationAPIService,
                _azureCosmosDBService,
                _contentSafetyService,
                _lakeraGuardService,
                _enkryptGuardrailsService,
                _gatekeeperIntegrationAPIService,
                _gatekeeperServiceSettings);
        }

        [Fact]
        public async Task GetCompletion_CallsOrchestrationAPIServiceWithCompletionRequest()
        {
            // Arrange
            var completionRequest = new CompletionRequest
            {
                OperationId = Guid.NewGuid().ToString(),
                UserPrompt = "Safe content."
            };

            var expectedResult = new CompletionResponse { OperationId=completionRequest.OperationId, Completion = "Completion from Orchestration API Service." };

            var safeContentResult = new AnalyzeTextFilterResult { Safe = true, Reason = string.Empty };
            _contentSafetyService.AnalyzeText(completionRequest.UserPrompt).Returns(safeContentResult);
            _orchestrationAPIService.GetCompletion(_instanceId, completionRequest).Returns(expectedResult);

            // Act
            var actualResult = await _testedService.GetCompletion(_instanceId, completionRequest);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }
    }
}
