using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
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
        /// <para>
        /// Note that the agent name in <paramref name="agentName"/> can be different from the agent name in <paramref name="originalRequest"/>.
        /// This happens when the original completion request results in the need to bring in additional agents to the conversation.
        /// </para>
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance ID.</param>
        /// <param name="agentName">The unique name of the agent for which the orchestration is built.</param>
        /// <param name="originalRequest">The <see cref="CompletionRequest"/> request for which the orchestration is built.</param>
        /// <param name="callContext">The call context of the request being handled.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> used to retrieve app settings from configuration.</param>
        /// <param name="resourceProviderServices">A dictionary of <see cref="IResourceProviderService"/> resource providers hashed by resource provider name.</param>
        /// <param name="llmOrchestrationServiceManager">The <see cref="ILLMOrchestrationServiceManager"/> that manages internal and external orchestration services.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> provding dependency injection services for the current scope.</param>
        /// <param name="loggerFactory">The logger factory used to create new loggers.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<OrchestrationBase?> Build(
            string instanceId,
            string agentName,
            CompletionRequest originalRequest,
            ICallContext callContext,
            IConfiguration configuration,
            Dictionary<string, IResourceProviderService> resourceProviderServices,
            ILLMOrchestrationServiceManager llmOrchestrationServiceManager,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<OrchestrationBuilder>();

            var result = await LoadAgent(
                agentName,
                originalRequest.Settings?.ModelParameters,
                resourceProviderServices,
                callContext.CurrentUserIdentity!,
                logger);

            if (result.Agent == null) return null;
            
            if (result.Agent.AgentType == typeof(KnowledgeManagementAgent))
            {
                var orchestrationName = string.IsNullOrWhiteSpace(result.Agent.OrchestrationSettings?.Orchestrator)
                    ? LLMOrchestrationServiceNames.LangChain
                    : result.Agent.OrchestrationSettings?.Orchestrator;

                var orchestrationService = llmOrchestrationServiceManager.GetService(instanceId, orchestrationName!, serviceProvider, callContext);
                
                var kmOrchestration = new KnowledgeManagementOrchestration(
                    (KnowledgeManagementAgent)result.Agent,
                    result.ExplodedObjects ?? [],
                    callContext,
                    orchestrationService,
                    loggerFactory.CreateLogger<OrchestrationBase>(),
                    resourceProviderServices,
                    result.DataSourceAccessDenied);

                return kmOrchestration;
            }

            return null;
        }

        private static async Task<(AgentBase? Agent, Dictionary<string, object>? ExplodedObjects, bool DataSourceAccessDenied)> LoadAgent(
            string? agentName,
            Dictionary<string, object>? modelParameterOverrides,
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
            if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_AIModel, out var aiModelResourceProvider))
                throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_AIModel} was not loaded.");
            if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Configuration, out var configurationResourceProvider))
                throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_Configuration} was not loaded.");

            var explodedObjects = new Dictionary<string, object>();

            var agentBase = await agentResourceProvider.GetResource<AgentBase>(
                $"/{AgentResourceTypeNames.Agents}/{agentName}",
                currentUserIdentity);

            var prompt = await promptResourceProvider.GetResource<PromptBase>(
                agentBase.PromptObjectId!,
                currentUserIdentity);
            var aiModel = await aiModelResourceProvider.GetResource<AIModelBase>(
                agentBase.AIModelObjectId!,
                currentUserIdentity);
            var apiEndpointConfiguration = await configurationResourceProvider.GetResource<APIEndpointConfiguration>(
                aiModel.EndpointObjectId!,
                currentUserIdentity);

            // Merge the model parameter overrides with the existing model parameter values from the AI model.
            if (modelParameterOverrides != null)
            {
                // Allowing the override only for the keys that are supported.
                foreach (var key in modelParameterOverrides.Keys.Where(k => ModelParametersKeys.All.Contains(k)))
                {
                    aiModel.ModelParameters[key] = modelParameterOverrides[key];
                }
            }

            explodedObjects[agentBase.PromptObjectId!] = prompt;
            explodedObjects[agentBase.AIModelObjectId!] = aiModel;
            explodedObjects[aiModel.EndpointObjectId!] = apiEndpointConfiguration;

            var allAgents = await agentResourceProvider.GetResources<AgentBase>(currentUserIdentity);
            var allAgentsDescriptions = allAgents
                .Where(a => !string.IsNullOrWhiteSpace(a.Description) && a.Name != agentBase.Name)
                .Select(a => new
                {
                    a.Name,
                    a.Description
                })
                .ToDictionary(x => x.Name, x => x.Description);
            explodedObjects[CompletionRequestObjectsKeys.AllAgents] = allAgentsDescriptions;

            if (agentBase is KnowledgeManagementAgent kmAgent)
            {
                // Check for inline-context agents, they are valid KM agents that do not have a vectorization section.
                if (kmAgent is {Vectorization: not null, InlineContext: false})
                {
                    if (!string.IsNullOrWhiteSpace(kmAgent.Vectorization.DataSourceObjectId))
                    {
                        try
                        {
                            var dataSource = await dataSourceResourceProvider.GetResource<DataSourceBase>(
                                kmAgent.Vectorization.DataSourceObjectId,
                                currentUserIdentity);

                            if (dataSource == null)
                                return (null, null, false);
                        }
                        catch (ResourceProviderException ex) when (ex.StatusCode == (int)HttpStatusCode.Forbidden)
                        {
                            // Access is denied to the underlying data source.
                            return (agentBase, null, true);
                        }
                    }

                    foreach (var indexingProfileName in kmAgent.Vectorization.IndexingProfileObjectIds ?? [])
                    {
                        if (string.IsNullOrWhiteSpace(indexingProfileName))
                        {
                            continue;
                        }

                        var indexingProfile = await vectorizationResourceProvider.GetResource<VectorizationProfileBase>(
                            indexingProfileName,
                            currentUserIdentity);
                        var indexingProfileCasted = indexingProfile as IndexingProfile;
                        if (indexingProfileCasted == null)
                            throw new OrchestrationException($"The indexing profile {indexingProfileName} is not a valid indexing profile.");

                        explodedObjects[indexingProfileName] = indexingProfile;
                                               
                        // Provide the indexing profile API endpoint configuration.
                        var indexingProfileAPIEndpointConfiguration = await configurationResourceProvider.GetResource<APIEndpointConfiguration>(
                            indexingProfileCasted.IndexingAPIEndpointConfigurationObjectId,
                            currentUserIdentity);

                        explodedObjects[indexingProfileCasted.IndexingAPIEndpointConfigurationObjectId] = indexingProfileAPIEndpointConfiguration;
                    }

                    if (!string.IsNullOrWhiteSpace(kmAgent.Vectorization.TextEmbeddingProfileObjectId))
                    {
                        var textEmbeddingProfile = await vectorizationResourceProvider.GetResource<VectorizationProfileBase>(
                            kmAgent.Vectorization.TextEmbeddingProfileObjectId,
                            currentUserIdentity);

                        var textEmbeddingProfileCasted = textEmbeddingProfile as TextEmbeddingProfile;
                        if (textEmbeddingProfileCasted == null)
                            throw new OrchestrationException($"The text embedding profile {kmAgent.Vectorization.TextEmbeddingProfileObjectId} is not a valid text embedding profile.");

                        explodedObjects[kmAgent.Vectorization.TextEmbeddingProfileObjectId!] = textEmbeddingProfileCasted;

                        if(textEmbeddingProfileCasted.TextEmbedding != TextEmbeddingType.GatewayTextEmbedding)
                        {
                            // Provide the Embedding AI Model and associated API endpoint configuration.
                            var embeddingAIModelBase = await aiModelResourceProvider.GetResource<AIModelBase>(
                                textEmbeddingProfileCasted.EmbeddingAIModelObjectId!,
                                currentUserIdentity);

                            var embeddingAIModel = embeddingAIModelBase as EmbeddingAIModel;
                            if (embeddingAIModel == null)
                                throw new OrchestrationException($"The AI model {textEmbeddingProfileCasted.EmbeddingAIModelObjectId} is not a valid Embedding AI model.");

                            explodedObjects[textEmbeddingProfileCasted.EmbeddingAIModelObjectId!] = embeddingAIModel;

                            // Provide the embedding AI model API endpoint configuration.
                            var embeddingAIModelAPIEndpointConfiguration = await configurationResourceProvider.GetResource<APIEndpointConfiguration>(
                                embeddingAIModel.EndpointObjectId!,
                                currentUserIdentity);
                            explodedObjects[embeddingAIModel.EndpointObjectId!] = embeddingAIModelAPIEndpointConfiguration;
                        }
                       
                    }
                }
            }

            return (agentBase, explodedObjects, false);
        }
    }
}
