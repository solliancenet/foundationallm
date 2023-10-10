using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using Newtonsoft.Json;

namespace FoundationaLLM.Gatekeeper.Core.Services
{
    public class GatekeeperService : IGatekeeperService
    {
        private readonly IAgentFactoryAPIService _agentFactoryAPIService;
        private readonly IRefinementService _refinementService;

        public GatekeeperService(
            IAgentFactoryAPIService agentFactoryAPIService,
            IRefinementService refinementService)
        {
            _agentFactoryAPIService = agentFactoryAPIService;
            _refinementService = refinementService;
        }

        public async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
        {
            //TODO: Call RefinementService to refine userPrompt
            //await _refinementService.RefineUserPrompt(completionRequest.Prompt);

            return await _agentFactoryAPIService.GetCompletion(completionRequest);
        }

        public async Task<SummaryResponse> GetSummary(SummaryRequest summaryRequest)
        {
            //TODO: Call RefinementService to refine userPrompt
            //await _refinementService.RefineUserPrompt(summaryRequest.Prompt);

            return await _agentFactoryAPIService.GetSummary(summaryRequest);
        }

        public async Task<bool> SetLLMOrchestrationPreference(string orchestrationService)
        {
            return await _agentFactoryAPIService.SetLLMOrchestrationPreference(orchestrationService);
        }
    }
}
