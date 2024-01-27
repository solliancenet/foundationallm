using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
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
        private readonly GatekeeperService _testedService;

        private readonly IContentSafetyService _contentSafetyService = Substitute.For<IContentSafetyService>();
        private readonly IDownstreamAPIService _agentFactoryAPIService = Substitute.For<IDownstreamAPIService>();
        private readonly IRefinementService _refinementService = Substitute.For<IRefinementService>();
        private readonly IGatekeeperIntegrationAPIService _gatekeeperIntegrationAPIService = Substitute.For<IGatekeeperIntegrationAPIService>();
        private readonly IOptions<GatekeeperServiceSettings> _gatekeeperServiceSettings = Substitute.For<IOptions<GatekeeperServiceSettings>>();

        public GatekeeperServiceTests()
        {
            _testedService = new GatekeeperService(
                _agentFactoryAPIService,
                _contentSafetyService,
                _gatekeeperIntegrationAPIService,
                _gatekeeperServiceSettings);
        }

        [Fact]
        public async Task GetCompletion_CallsAgentFactoryAPIServiceWithCompletionRequest()
        {
            // Arrange
            var completionRequest = new CompletionRequest
            {
                UserPrompt = "Safe content."
            };

            var expectedResult = new CompletionResponse { Completion = "Completion from Agent Factory API Service." };

            var safeContentResult = new AnalyzeTextFilterResult { Safe = true, Reason = string.Empty };
            _contentSafetyService.AnalyzeText(completionRequest.UserPrompt).Returns(safeContentResult);
            _agentFactoryAPIService.GetCompletion(completionRequest).Returns(expectedResult);

            // Act
            var actualResult = await _testedService.GetCompletion(completionRequest);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public async Task GetSummary_CallsAgentFactoryAPIServiceWithSummaryRequest()
        {
            // Arrange
            var summaryRequest = new SummaryRequest
            {
                UserPrompt = "Safe content for summary."
            };

            var expectedResult = new SummaryResponse { Summary = "Summary from Agent Factory API Service." };

            var safeContentResult = new AnalyzeTextFilterResult { Safe = true, Reason = string.Empty };

            _contentSafetyService.AnalyzeText(summaryRequest.UserPrompt).Returns(safeContentResult);
            _agentFactoryAPIService.GetSummary(summaryRequest).Returns(expectedResult);

            // Act
            var actualResult = await _testedService.GetSummary(summaryRequest);

            // Assert
            Assert.Equal(expectedResult, actualResult);
        }
    }
}
