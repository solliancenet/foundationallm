using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Gatekeeper.Core.Services
{
    /// <summary>
    /// Implements the <see cref="IGatekeeperService"/> interface.
    /// </summary>
    /// <remarks>
    /// Constructor for the Gatekeeper service.
    /// </remarks>
    /// <param name="agentFactoryAPIService">The Agent Factory API client.</param>
    /// <param name="contentSafetyService">The user prompt Content Safety service.</param>
    /// <param name="gatekeeperIntegrationAPIService">The Gatekeeper Integration API client.</param>
    /// <param name="gatekeeperServiceSettings">The configuration options for the Gatekeeper service.</param>
    public class GatekeeperService(
        IDownstreamAPIService agentFactoryAPIService,
        IContentSafetyService contentSafetyService,
        IGatekeeperIntegrationAPIService gatekeeperIntegrationAPIService,
        IOptions<GatekeeperServiceSettings> gatekeeperServiceSettings) : IGatekeeperService
    {
        private readonly IDownstreamAPIService _agentFactoryAPIService = agentFactoryAPIService;
        private readonly IContentSafetyService _contentSafetyService = contentSafetyService;
        private readonly IGatekeeperIntegrationAPIService _gatekeeperIntegrationAPIService = gatekeeperIntegrationAPIService;
        private readonly GatekeeperServiceSettings _gatekeeperServiceSettings = gatekeeperServiceSettings.Value;

        /// <summary>
        /// Gets a completion from the Gatekeeper service.
        /// </summary>
        /// <param name="completionRequest">The completion request containing the user prompt and message history.</param>
        /// <returns>The completion response.</returns>
        public async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
        {
            //TODO: Call the Refinement Service with the userPrompt
            //await _refinementService.RefineUserPrompt(completionRequest.Prompt);

            if (_gatekeeperServiceSettings.EnableAzureContentSafety)
            {
                var contentSafetyResult = await _contentSafetyService.AnalyzeText(completionRequest.UserPrompt!);

                if (!contentSafetyResult.Safe)
                    return new CompletionResponse() { Completion = contentSafetyResult.Reason };
            }

            var completionResponse = await _agentFactoryAPIService.GetCompletion(completionRequest);

            if (_gatekeeperServiceSettings.EnableMicrosoftPresidio)
                completionResponse.Completion = await _gatekeeperIntegrationAPIService.AnonymizeText(completionResponse.Completion);

            return completionResponse;
        }

        /// <summary>
        /// Gets a summary from the Gatekeeper service.
        /// </summary>
        /// <param name="summaryRequest">The summarize request containing the user prompt.</param>
        /// <returns>The summary response.</returns>
        public async Task<SummaryResponse> GetSummary(SummaryRequest summaryRequest)
        {
            //TODO: Call the Refinement Service with the userPrompt
            //await _refinementService.RefineUserPrompt(summaryRequest.Prompt);

            if (_gatekeeperServiceSettings.EnableAzureContentSafety)
            {
                var contentSafetyResult = await _contentSafetyService.AnalyzeText(summaryRequest.UserPrompt!);

                if (!contentSafetyResult.Safe)
                    return new SummaryResponse() { Summary = contentSafetyResult.Reason };
            }

            var summaryResponse = await _agentFactoryAPIService.GetSummary(summaryRequest);

            if (_gatekeeperServiceSettings.EnableMicrosoftPresidio)
                summaryResponse.Summary = await _gatekeeperIntegrationAPIService.AnonymizeText(summaryResponse.Summary!);

            return summaryResponse;
        }
    }
}
