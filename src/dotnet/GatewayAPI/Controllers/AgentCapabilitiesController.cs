﻿using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.OpenAI;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Gateway;
using FoundationaLLM.Gateway.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Gateway.API.Controllers
{
    /// <summary>
    /// Methods for managing agent capabilities.
    /// </summary>
    /// <param name="gatewayCore">The <see cref="IGatewayCore"/> that provides LLM gateway services.</param>
    /// <param name="callContext">The <see cref="IOrchestrationContext"/> call context of the request being handled.</param>
    [ApiController]
    [APIKeyAuthentication]
    [Route("instances/{instanceId}/[controller]")]
    public class AgentCapabilitiesController(
        IGatewayCore gatewayCore,
        IOrchestrationContext callContext)
    {
        readonly IGatewayCore _gatewayCore = gatewayCore;
        private readonly IOrchestrationContext _callContext = callContext;

        /// <summary>
        /// Creates an agent capability.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="agentCapabilityRequest">The <see cref="AgentCapabilityRequest"/> object with the deails of the requested capability.</param>
        /// <returns>A dictionary of output values.</returns>
        /// <remarks>
        /// The supported categories are:
        /// <list type="bullet">
        /// <item>
        /// OpenAI.Assistants (the names of the keys for the output dictionary are defined in <see cref="OpenAIAgentCapabilityParameterNames"/>)
        /// </item>
        /// </list>
        /// </remarks>
        [HttpPost]
        public async Task<IActionResult> CreateAgentCapability(
            string instanceId,
            [FromBody] AgentCapabilityRequest agentCapabilityRequest) =>
            new OkObjectResult(await _gatewayCore.CreateAgentCapability(
                instanceId,
                agentCapabilityRequest.CapabilityCategory,
                agentCapabilityRequest.CapabilityName,
                _callContext.CurrentUserIdentity!,
                agentCapabilityRequest.Parameters));
    }
}
