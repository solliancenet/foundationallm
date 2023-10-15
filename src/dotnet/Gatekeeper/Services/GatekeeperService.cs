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
        private readonly IContentSafetyService _contentSafetyService;

        public GatekeeperService(
            IAgentFactoryAPIService agentFactoryAPIService,
            IRefinementService refinementService,
            IContentSafetyService contentSafetyService)
        {
            _agentFactoryAPIService = agentFactoryAPIService;
            _refinementService = refinementService;
            _contentSafetyService = contentSafetyService;
        }

        public async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
        {
            //TODO: Call RefinementService to refine userPrompt
            //await _refinementService.RefineUserPrompt(completionRequest.Prompt);

            var result = await _contentSafetyService.AnalyzeText(completionRequest.UserPrompt);
            
            if (result.Safe)
                return await _agentFactoryAPIService.GetCompletion(completionRequest);

            return new CompletionResponse() { Completion = result.Reason };
        }

        public async Task<SummaryResponse> GetSummary(SummaryRequest summaryRequest)
        {
            //TODO: Call RefinementService to refine userPrompt
            //await _refinementService.RefineUserPrompt(summaryRequest.Prompt);

            var result = await _contentSafetyService.AnalyzeText(summaryRequest.Prompt);

            if (result.Safe)
                return await _agentFactoryAPIService.GetSummary(summaryRequest);

            return new SummaryResponse() { Info = result.Reason };
        }

        public async Task<bool> SetLLMOrchestrationPreference(string orchestrationService)
        {
            return await _agentFactoryAPIService.SetLLMOrchestrationPreference(orchestrationService);
        }
    }
}
