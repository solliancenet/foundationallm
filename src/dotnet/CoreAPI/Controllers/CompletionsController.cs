using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Logging;
using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Orchestration.Response;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FoundationaLLM.Core.API.Controllers
{
    /// <summary>
    /// Provides methods for retrieving and managing completions.
    /// </summary>
    /// <remarks>
    /// Constructor for the Completions Controller.
    /// </remarks>
    [Authorize(Policy = "DefaultPolicy")]
    [ApiController]
    [Route("instances/{instanceId}")]
    public class CompletionsController : ControllerBase
    {
        private readonly ICoreService _coreService;
        private readonly IResourceProviderService _agentResourceProvider;
        private readonly ILogger<CompletionsController> _logger;
        private readonly ICallContext _callContext;

        /// <summary>
        /// Methods for orchestration services exposed by the Gatekeeper API service.
        /// </summary>
        /// <remarks>
        /// Constructor for the Orchestration Controller.
        /// </remarks>
        /// <param name="coreService">The Core service provides methods for getting
        /// completions from the orchestrator.</param>
        /// <param name="callContext">The call context for the request.</param>
        /// <param name="resourceProviderServices">The list of <see cref="IResourceProviderService"/> resource provider services.</param>
        /// <param name="logger">The logging interface used to log under the
        /// <see cref="CompletionsController"/> type name.</param>
        public CompletionsController(ICoreService coreService,
            ICallContext callContext,
            IEnumerable<IResourceProviderService> resourceProviderServices,
            ILogger<CompletionsController> logger)
        {
            _coreService = coreService;
            var resourceProviderServicesDictionary = resourceProviderServices.ToDictionary<IResourceProviderService, string>(
                rps => rps.Name);
            if (!resourceProviderServicesDictionary.TryGetValue(ResourceProviderNames.FoundationaLLM_Agent, out var agentResourceProvider))
                throw new ResourceProviderException($"The resource provider {ResourceProviderNames.FoundationaLLM_Agent} was not loaded.");
            _agentResourceProvider = agentResourceProvider;
            _logger = logger;
            _callContext = callContext;
        }

        /// <summary>
        /// Requests a completion from the downstream APIs.
        /// </summary>
        /// <param name="instanceId">The instance ID of the current request.</param>
        /// <param name="completionRequest">The user prompt for which to generate a completion.</param>
        [HttpPost("completions", Name = "GetCompletion")]
        public async Task<IActionResult> GetCompletion(string instanceId, [FromBody] CompletionRequest completionRequest)
        {
            using (var activity = ActivitySources.CoreAPIActivitySource.StartActivity("GetCompletion", ActivityKind.Consumer, parentContext: default, tags: new Dictionary<string, object> { { "UPN", _callContext.CurrentUserIdentity?.UPN }, { "RequestId", completionRequest.SessionId }, { "UserId", _callContext.CurrentUserIdentity?.UserId } }))
            {
                return !string.IsNullOrWhiteSpace(completionRequest.SessionId)
                ? Ok(await _coreService.GetChatCompletionAsync(instanceId, completionRequest))
                : Ok(await _coreService.GetCompletionAsync(instanceId, completionRequest));
            }
        }

        /// <summary>
        /// Begins a completion operation.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="completionRequest">The completion request containing the user prompt and message history.</param>
        /// <returns>Returns an <see cref="LongRunningOperation"/> object containing the OperationId and Status.</returns>
        [HttpPost("async-completions")]
        public async Task<ActionResult<LongRunningOperation>> StartCompletionOperation(string instanceId, CompletionRequest completionRequest)
        {
            using (var activity = ActivitySources.CoreAPIActivitySource.StartActivity("StartCompletionOperation", ActivityKind.Consumer, parentContext: default, tags: new Dictionary<string, object> { { "UPN", _callContext.CurrentUserIdentity?.UPN }, { "RequestId", completionRequest.SessionId }, { "UserId", _callContext.CurrentUserIdentity?.UserId } }))
            {
                var state = await _coreService.StartCompletionOperation(instanceId, completionRequest);
                return Accepted(state);
            }
        }

        /// <summary>
        /// Gets the status of a completion operation.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="operationId">The OperationId for which to retrieve the status.</param>
        /// <returns>Returns a <see cref="LongRunningOperation"/> object containing the OperationId, Status, and result.</returns>
        [HttpGet("async-completions/{operationId}/status")]
        public async Task<LongRunningOperation> GetCompletionOperationStatus(string instanceId, string operationId)
        {
            using (var activity = ActivitySources.CoreAPIActivitySource.StartActivity("GetCompletionOperationStatus", ActivityKind.Consumer, parentContext: default, tags: new Dictionary<string, object> { { "UPN", _callContext.CurrentUserIdentity?.UPN }, { "Operationid", operationId }, { "UserId", _callContext.CurrentUserIdentity?.UserId } }))
            {
                return await _coreService.GetCompletionOperationStatus(instanceId, operationId);
            }
        }

        /// <summary>
        /// Retrieves a list of global and private agents.
        /// </summary>
        /// <param name="instanceId">The instance ID of the current request.</param>
        /// <returns>A list of available agents.</returns>
        [HttpGet("completions/agents", Name = "GetAgents")]
        public async Task<IEnumerable<ResourceProviderGetResult<AgentBase>>> GetAgents(string instanceId) =>
            await _agentResourceProvider.GetResourcesAsync<AgentBase>(
                instanceId,
                _callContext.CurrentUserIdentity!,
                new ResourceProviderGetOptions
                {
                    IncludeRoles = true,
                    IncludeActions = true,
                    LoadContent = false
                });
    }
}
