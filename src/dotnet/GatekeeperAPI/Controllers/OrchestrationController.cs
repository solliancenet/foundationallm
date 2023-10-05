using Asp.Versioning;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FoundationaLLM.Gatekeeper.API.Controllers
{
    //[Authorize]
    [ApiVersion(1.0)]
    [ApiController]
    [Route("[controller]")]
    public class OrchestrationController : ControllerBase
    {
        private readonly IGatekeeperService _gatekeeperService;

        public OrchestrationController(
            IGatekeeperService gatekeeperService)
        {
            _gatekeeperService = gatekeeperService;
        }

        [HttpPost("completion")]
        public async Task<CompletionResponseBase> GetCompletion([FromBody] CompletionRequestBase completionRequest)
        {
            return await _gatekeeperService.GetCompletion(completionRequest);
        }

        [HttpPost("summarize")]
        public async Task<string> GetSummary([FromBody] string content)
        {
            return await _gatekeeperService.GetSummary(content);
        }
    }
}