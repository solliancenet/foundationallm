using Asp.Versioning;
using FoundationaLLM.Common.Authorization;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Gatekeeper.API.Controllers
{
    [ApiVersion(1.0)]
    [ApiController]
    [ApiKeyAuthorization]
    [Route("[controller]")]
    public class OrchestrationController : ControllerBase
    {
        private readonly IAgentFactoryAPIService _agentFactoryApiService;

        public OrchestrationController(
            IAgentFactoryAPIService agentFactoryApiService)
        {
            _agentFactoryApiService = agentFactoryApiService;
        }

        [HttpPost("completion")]
        public async Task<CompletionResponse> GetCompletion([FromBody] CompletionRequest completionRequest)
        {
            return await _agentFactoryApiService.GetCompletion(completionRequest);
        }

        [HttpPost("summarize")]
        public async Task<SummaryResponse> GetSummary([FromBody] SummaryRequest content)
        {
            return await _agentFactoryApiService.GetSummary(content);
        }

        [HttpPost("preference")]
        public async Task<bool> SetLLMOrchestrationPreference([FromBody] string orchestrationService)
        {
            return await _agentFactoryApiService.SetLLMOrchestrationPreference(orchestrationService);
        }
    }
}