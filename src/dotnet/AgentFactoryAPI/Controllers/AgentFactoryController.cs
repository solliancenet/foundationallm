using Asp.Versioning;
using FoundationaLLM.AgentFactory.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.AgentFactory.API.Controllers
{
    //[Authorize]
    [ApiVersion(1.0)]
    [ApiController]
    [Route("api/agentfactory")]
    public class AgentFactoryController : ControllerBase
    {
        private readonly IAgentFactoryService _agentFactoryService;

        public AgentFactoryController(
            IAgentFactoryService agentFactoryService)
        {
            _agentFactoryService = agentFactoryService;
        }

    }
}
