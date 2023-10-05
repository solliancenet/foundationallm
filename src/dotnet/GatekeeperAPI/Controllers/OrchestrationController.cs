using Asp.Versioning;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FoundationaLLM.SemanticKernel.API.Controllers
{
    //[Authorize]
    [ApiVersion(1.0)]
    [ApiController]
    [Route("[controller]")]
    public class OrchestrationController : ControllerBase
    {
        private readonly IAgentFactoryAPIService _agentFactoryAPIService;

        public OrchestrationController(
            IAgentFactoryAPIService agentFactoryAPIService)
        {
            _agentFactoryAPIService = agentFactoryAPIService;
        }

        [HttpPost("completion")]
        public async Task<CompletionResponseBase> GetCompletion([FromBody] CompletionRequestBase completionRequest)
        {
            return await _agentFactoryAPIService.GetCompletion(completionRequest);
        }

        [HttpPost("summarize")]
        public async Task<string> GetSummary([FromBody] string content)
        {
            return await _agentFactoryAPIService.GetSummary(content);
        }
    }
}