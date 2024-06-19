using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Orchestration.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;

namespace FoundationaLLM.Orchestration.Core.Orchestration
{
    /// <summary>
    /// Builds an orchestration for a FoundationaLLM agent.
    /// </summary>
    public class OrchestrationBuilder
    {
        /// <summary>
        /// Builds the orchestration based on the user prompt, the session id, and the call context.
        /// </summary>
        /// <param name="agentName">The unique name of the agent for which the orchestration is built.</param>
        /// <param name="callContext">The call context of the request being handled.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> used to retrieve app settings from configuration.</param>
        /// <param name="resourceProviderServices">A dictionary of <see cref="IResourceProviderService"/> resource providers hashed by resource provider name.</param>
        /// <param name="llmOrchestrationServiceManager">The <see cref="ILLMOrchestrationServiceManager"/> that manages internal and external orchestration services.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> provding dependency injection services for the current scope.</param>
        /// <param name="loggerFactory">The logger factory used to create new loggers.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<OrchestrationBase?> Build(
            string agentName,
            ICallContext callContext,
            IConfiguration configuration,
            Dictionary<string, IResourceProviderService> resourceProviderServices,
            ILLMOrchestrationServiceManager llmOrchestrationServiceManager,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<OrchestrationBuilder>();

            var result = await LoadAgent(agentName, resourceProviderServices, callContext.CurrentUserIdentity!, logger);
            if (result.Agent == null) return null;
            
            if (result.Agent.AgentType == typeof(KnowledgeManagementAgent))
            {
                var orchestrationName = string.IsNullOrWhiteSpace(result.Agent.OrchestrationSettings?.Orchestrator)
                    ? LLMOrchestrationServiceNames.LangChain
                    : result.Agent.OrchestrationSettings?.Orchestrator;

                var orchestrationService = llmOrchestrationServiceManager.GetService(orchestrationName!, serviceProvider, callContext);
                
                var kmOrchestration = new KnowledgeManagementOrchestration(
                    (KnowledgeManagementAgent)result.Agent,
                    callContext,
                    orchestrationService,
                    loggerFactory.CreateLogger<OrchestrationBase>(),
                    resourceProviderServices,
                    result.DataSourceAccessDenied);

                return kmOrchestration;
            }

            return null;
        }

        private static async Task<(AgentBase? Agent, bool DataSourceAccessDenied)> LoadAgent(
            string? agentName,
            Dictionary<string, IResourceProviderService> resourceProviderServices,
            UnifiedUserIdentity currentUserIdentity,
            ILogger<OrchestrationBuilder> logger)
        {
            if (string.IsNullOrWhiteSpace(agentName))
                throw new OrchestrationException("The agent name provided in the completion request is invalid.");

            if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Agent, out var agentResourceProvider))
                throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_Agent} was not loaded.");
            if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Prompt, out var promptResourceProvider))
                throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_Prompt} was not loaded.");
            if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Vectorization, out var vectorizationResourceProvider))
                throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_Vectorization} was not loaded.");
            if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_DataSource, out var dataSourceResourceProvider))
                throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_DataSource} was not loaded.");

            var agentBase = await agentResourceProvider.GetResource<AgentBase>(
                $"/{AgentResourceTypeNames.Agents}/{agentName}",
                currentUserIdentity);

            if (agentBase.OrchestrationSettings!.AgentParameters == null)
                agentBase.OrchestrationSettings.AgentParameters = [];

            var prompt = await promptResourceProvider.GetResource<PromptBase>(
                agentBase.PromptObjectId!,
                currentUserIdentity);

            agentBase.OrchestrationSettings.AgentParameters[agentBase.PromptObjectId!] = prompt;

            var allAgents = await agentResourceProvider.GetResources<AgentBase>(currentUserIdentity);
            var allAgentsDescriptions = allAgents
                .Where(a => !string.IsNullOrWhiteSpace(a.Description) && a.Name != agentBase.Name)
                .Select(a => new
                {
                    a.Name,
                    a.Description
                })
                .ToDictionary(x => x.Name, x => x.Description);
            agentBase.OrchestrationSettings.AgentParameters["AllAgents"] = allAgentsDescriptions;

            if (agentBase is KnowledgeManagementAgent kmAgent)
            {
                // check for inline-context agents, they are valid KM agents that do not have a vectorization section.
                if(kmAgent.Vectorization != null)
                {
                    if (!string.IsNullOrWhiteSpace(kmAgent.Vectorization.DataSourceObjectId))
                    {
                        try
                        {
                            var dataSource = await dataSourceResourceProvider.GetResource<DataSourceBase>(
                                kmAgent.Vectorization.DataSourceObjectId,
                                currentUserIdentity);

                            if (dataSource == null)
                                return (null, false);
                        }
                        catch (ResourceProviderException ex) when (ex.StatusCode == (int)HttpStatusCode.Forbidden)
                        {
                            // Access is denied to the underlying data source.
                            return (agentBase, true);
                        }
                    }

                    foreach (var indexingProfileName in kmAgent.Vectorization.IndexingProfileObjectIds ?? [])
                    {
                        var indexingProfile = await vectorizationResourceProvider.GetResource<VectorizationProfileBase>(
                            indexingProfileName,
                            currentUserIdentity);

                        kmAgent.OrchestrationSettings!.AgentParameters![indexingProfileName] = indexingProfile;
                    }

                    if (!string.IsNullOrWhiteSpace(kmAgent.Vectorization.TextEmbeddingProfileObjectId))
                    {
                        var textEmbeddingProfile = await vectorizationResourceProvider.GetResource<VectorizationProfileBase>(
                            kmAgent.Vectorization.TextEmbeddingProfileObjectId,
                            currentUserIdentity);

                        kmAgent.OrchestrationSettings!.AgentParameters![kmAgent.Vectorization.TextEmbeddingProfileObjectId!] = textEmbeddingProfile;
                    }
                }
            }

            return (agentBase, false);
        }
    }
}
