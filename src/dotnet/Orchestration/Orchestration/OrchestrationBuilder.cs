using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Logging;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Orchestration.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;

namespace FoundationaLLM.Orchestration.Core.Orchestration
{
    /// <summary>
    /// Builds an orchestration for a FoundationaLLM agent.
    /// </summary>
    public class OrchestrationBuilder
    {
        /// <summary>
        /// Builds the orchestration used to handle a synchronous completion operation or start an asynchronous completion operation.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance ID.</param>
        /// <param name="agentName">The unique name of the agent for which the orchestration is built.</param>
        /// <param name="originalRequest">The <see cref="CompletionRequest"/> request for which the orchestration is built.</param>
        /// <param name="callContext">The call context of the request being handled.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> used to retrieve app settings from configuration.</param>
        /// <param name="resourceProviderServices">A dictionary of <see cref="IResourceProviderService"/> resource providers hashed by resource provider name.</param>
        /// <param name="llmOrchestrationServiceManager">The <see cref="ILLMOrchestrationServiceManager"/> that manages internal and external orchestration services.</param>
        /// <param name="cosmosDBService">The <see cref="IAzureCosmosDBService"/> used to interact with the Cosmos DB database.</param>
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
            IAzureCosmosDBService cosmosDBService,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory)
        {
            var logger = loggerFactory.CreateLogger<OrchestrationBuilder>();

            var result = await LoadAgent(
                instanceId,
                agentName,
                originalRequest.SessionId,
                originalRequest.Settings?.ModelParameters,
                resourceProviderServices,
                callContext.CurrentUserIdentity!,
                logger);

            if (result.Agent == null) return null;

            var vectorStoreId = await EnsureAgentCapabilities(
                instanceId,
                result.Agent,
                originalRequest.SessionId!,
                result.ExplodedObjects!,
                resourceProviderServices,
                callContext.CurrentUserIdentity!,
                logger);

            if (result.Agent.AgentType == typeof(KnowledgeManagementAgent))
            {
                var orchestrator = string.IsNullOrWhiteSpace(result.Agent.OrchestrationSettings?.Orchestrator)
                    ? LLMOrchestrationServiceNames.LangChain
                    : result.Agent.OrchestrationSettings?.Orchestrator;

                if (originalRequest.LongRunningOperation)
                {
                    await cosmosDBService.PatchOperationsItemPropertiesAsync<LongRunningOperationContext>(
                        originalRequest.OperationId!,
                        originalRequest.OperationId!,
                        new Dictionary<string, object?>
                        {
                            { "/orchestrator", orchestrator! },
                            { "/agentWorkflowMainAIModelAPIEndpoint", result.APIEndpointConfiguration!.Url }
                        });
                }

                var orchestrationService = llmOrchestrationServiceManager.GetService(instanceId, orchestrator!, serviceProvider, callContext);

                var kmOrchestration = new KnowledgeManagementOrchestration(
                    instanceId,
                    result.Agent.ObjectId!,
                    (KnowledgeManagementAgent)result.Agent,
                    result.APIEndpointConfiguration!.Url,
                    result.ExplodedObjects ?? [],
                    callContext,
                    orchestrationService,
                    loggerFactory.CreateLogger<OrchestrationBase>(),
                    serviceProvider.GetRequiredService<IHttpClientFactoryService>(),
                    resourceProviderServices,
                    result.DataSourceAccessDenied,
                    vectorStoreId);

                return kmOrchestration;
            }

            return null;
        }

        /// <summary>
        /// Builds the orchestration used to retrieve the status of an asynchronous completion operation.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="operationId">The asynchronous completion operation identifier.</param>
        /// <param name="callContext">The call context of the request being handled.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> used to retrieve app settings from configuration.</param>
        /// <param name="resourceProviderServices">A dictionary of <see cref="IResourceProviderService"/> resource providers hashed by resource provider name.</param>
        /// <param name="llmOrchestrationServiceManager">The <see cref="ILLMOrchestrationServiceManager"/> that manages internal and external orchestration services.</param>
        /// <param name="cosmosDBService">The <see cref="IAzureCosmosDBService"/> used to interact with the Cosmos DB database.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> provding dependency injection services for the current scope.</param>
        /// <param name="loggerFactory">The logger factory used to create new loggers.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<OrchestrationBase?> BuildForStatus(
            string instanceId,
            string operationId,
            ICallContext callContext,
            IConfiguration configuration,
            Dictionary<string, IResourceProviderService> resourceProviderServices,
            ILLMOrchestrationServiceManager llmOrchestrationServiceManager,
            IAzureCosmosDBService cosmosDBService,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory)
        {
            var operationContext = await cosmosDBService.GetLongRunningOperationContextAsync(operationId);

            var orchestrationService = llmOrchestrationServiceManager.GetService(instanceId, operationContext.Orchestrator!, serviceProvider, callContext);

            var kmOrchestration = new KnowledgeManagementOrchestration(
                instanceId,
                ResourcePath.GetObjectId(
                    instanceId,
                    ResourceProviderNames.FoundationaLLM_Agent,
                    AgentResourceTypeNames.Agents,
                    operationContext.AgentName),
                null,
                operationContext.AgentWorkflowMainAIModelAPIEndpoint!,
                null,
                callContext,
                orchestrationService,
                loggerFactory.CreateLogger<OrchestrationBase>(),
                serviceProvider.GetRequiredService<IHttpClientFactoryService>(),
                resourceProviderServices,
                null,
                null);

            return kmOrchestration;
        }

        private static async Task<(AgentBase? Agent, AIModelBase? AIModel, APIEndpointConfiguration? APIEndpointConfiguration, Dictionary<string, object>? ExplodedObjects, bool DataSourceAccessDenied)> LoadAgent(
            string instanceId,
            string agentName,
            string? sessionId,
            Dictionary<string, object>? modelParameterOverrides,
            Dictionary<string, IResourceProviderService> resourceProviderServices,
            UnifiedUserIdentity currentUserIdentity,
            ILogger<OrchestrationBuilder> logger)
        {
            using (var activity = ActivitySources.OrchestrationAPIActivitySource.StartActivity("OrchestrationBuilder.LoadAgent", ActivityKind.Consumer, parentContext: default, tags: new Dictionary<string, object> { }))
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

                var agentBase = await agentResourceProvider.GetResourceAsync<AgentBase>(
                    $"/{AgentResourceTypeNames.Agents}/{agentName}",
                    currentUserIdentity);

                var prompt = await promptResourceProvider.GetResourceAsync<PromptBase>(
                    agentBase.PromptObjectId!,
                    currentUserIdentity);
                var aiModel = await aiModelResourceProvider.GetResourceAsync<AIModelBase>(
                    agentBase.AIModelObjectId!,
                    currentUserIdentity);
                var apiEndpointConfiguration = await configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(
                    aiModel.EndpointObjectId!,
                    currentUserIdentity);
                var gatewayAPIEndpointConfiguration = await configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(
                    instanceId,
                    "GatewayAPI",
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
                explodedObjects[CompletionRequestObjectsKeys.GatewayAPIEndpointConfiguration] = gatewayAPIEndpointConfiguration;

                if (agentBase.HasCapability(AgentCapabilityCategoryNames.OpenAIAssistants))
                {
                    explodedObjects[CompletionRequestObjectsKeys.OpenAIAssistantsAssistantId] =
                        agentBase.Properties?.GetValueOrDefault(AgentPropertyNames.AzureOpenAIAssistantId)
                        ?? throw new OrchestrationException("The OpenAI Assistants assistant identifier was not found in the agent properties.");
                }

                var allAgents = await agentResourceProvider.GetResourcesAsync<AgentBase>(instanceId, currentUserIdentity);
                var allAgentsDescriptions = allAgents
                    .Where(a => !string.IsNullOrWhiteSpace(a.Resource.Description) && a.Resource.Name != agentBase.Name)
                    .Select(a => new
                    {
                        a.Resource.Name,
                        a.Resource.Description
                    })
                    .ToDictionary(x => x.Name, x => x.Description);
                explodedObjects[CompletionRequestObjectsKeys.AllAgents] = allAgentsDescriptions;

                #region Tools

                List<string> toolNames = [];

                foreach (var toolName in agentBase.Tools.Keys)
                {
                    toolNames.Add(toolName);
                    explodedObjects[toolName] = agentBase.Tools[toolName];

                    foreach (var aiModelObjectId in agentBase.Tools[toolName].AIModelObjectIds.Values)
                    {
                        var toolAIModel = await aiModelResourceProvider.GetResourceAsync<AIModelBase>(
                            aiModelObjectId,
                            currentUserIdentity);

                        explodedObjects[aiModelObjectId] = toolAIModel;

                        var toolAPIEndpointConfiguration = await configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(
                            toolAIModel.EndpointObjectId!,
                            currentUserIdentity);

                        explodedObjects[toolAIModel.EndpointObjectId!] = toolAPIEndpointConfiguration;
                    }

                    foreach (var apiEndpointConfigurationObjectId in agentBase.Tools[toolName].APIEndpointConfigurationObjectIds.Values)
                    {
                        var toolAPIEndpointConfiguration = await configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(
                            apiEndpointConfigurationObjectId,
                            currentUserIdentity);

                        explodedObjects[apiEndpointConfigurationObjectId] = toolAPIEndpointConfiguration;
                    }
                }

                explodedObjects[CompletionRequestObjectsKeys.ToolNames] = toolNames;

                #endregion

                #region Knowledge management processing

                if (agentBase.AgentType == typeof(KnowledgeManagementAgent))
                {
                    KnowledgeManagementAgent kmAgent = (KnowledgeManagementAgent)agentBase;

                    // Check for inline-context agents, they are valid KM agents that do not have a vectorization section.
                    if (kmAgent is { Vectorization: not null, InlineContext: false })
                    {
                        if (!string.IsNullOrWhiteSpace(kmAgent.Vectorization.DataSourceObjectId))
                        {
                            try
                            {
                                var dataSource = await dataSourceResourceProvider.GetResourceAsync<DataSourceBase>(
                                    kmAgent.Vectorization.DataSourceObjectId,
                                    currentUserIdentity);

                                if (dataSource == null)
                                    return (null, null, null, null, false);
                            }
                            catch (ResourceProviderException ex) when (ex.StatusCode == (int)HttpStatusCode.Forbidden)
                            {
                                // Access is denied to the underlying data source.
                                return (agentBase, null, null, null, true);
                            }
                        }

                        foreach (var indexingProfileName in kmAgent.Vectorization.IndexingProfileObjectIds ?? [])
                        {
                            if (string.IsNullOrWhiteSpace(indexingProfileName))
                            {
                                continue;
                            }

                            var indexingProfile = await vectorizationResourceProvider.GetResourceAsync<IndexingProfile>(
                                indexingProfileName,
                                currentUserIdentity);

                            if (indexingProfile == null)
                                throw new OrchestrationException($"The indexing profile {indexingProfileName} is not a valid indexing profile.");

                            explodedObjects[indexingProfileName] = indexingProfile;

                            // Provide the indexing profile API endpoint configuration.
                            if (indexingProfile.Settings == null)
                                throw new OrchestrationException($"The settings for the indexing profile {indexingProfileName} were not found. Must include \"{VectorizationSettingsNames.IndexingProfileApiEndpointConfigurationObjectId}\" setting.");

                            if (indexingProfile.Settings.TryGetValue(VectorizationSettingsNames.IndexingProfileApiEndpointConfigurationObjectId, out var apiEndpointConfigurationObjectId) == false)
                                throw new OrchestrationException($"The API endpoint configuration object ID was not found in the settings of the indexing profile.");

                            var indexingProfileAPIEndpointConfiguration = await configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(
                                apiEndpointConfigurationObjectId,
                                currentUserIdentity);

                            explodedObjects[apiEndpointConfigurationObjectId] = indexingProfileAPIEndpointConfiguration;
                        }

                        if (!string.IsNullOrWhiteSpace(kmAgent.Vectorization.TextEmbeddingProfileObjectId))
                        {
                            var textEmbeddingProfile = await vectorizationResourceProvider.GetResourceAsync<TextEmbeddingProfile>(
                                kmAgent.Vectorization.TextEmbeddingProfileObjectId,
                                currentUserIdentity);

                            if (textEmbeddingProfile == null)
                                throw new OrchestrationException($"The text embedding profile {kmAgent.Vectorization.TextEmbeddingProfileObjectId} is not a valid text embedding profile.");

                            explodedObjects[kmAgent.Vectorization.TextEmbeddingProfileObjectId!] = textEmbeddingProfile;
                        }
                    }
                }

                #endregion

                return (agentBase, aiModel, apiEndpointConfiguration, explodedObjects, false);
            }
        }

        private static async Task<string?> EnsureAgentCapabilities(
            string instanceId,
            AgentBase agent,
            string conversationId,
            Dictionary<string, object> explodedObjects,
            Dictionary<string, IResourceProviderService> resourceProviderServices,
            UnifiedUserIdentity currentUserIdentity,
            ILogger<OrchestrationBuilder> logger)
        {
            if (agent.HasCapability(AgentCapabilityCategoryNames.OpenAIAssistants))
            {
                if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_AzureOpenAI, out var azureOpenAIResourceProvider))
                    throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_AzureOpenAI} was not loaded.");
                var prompt = explodedObjects[agent.PromptObjectId!] as MultipartPrompt;
                var aiModel = explodedObjects[agent.AIModelObjectId!] as AIModelBase;
                var apiEndpointConfiguration = explodedObjects[aiModel!.EndpointObjectId!] as APIEndpointConfiguration;
                var openAIAssistantsAssistantId = explodedObjects[CompletionRequestObjectsKeys.OpenAIAssistantsAssistantId] as string;

                var resourceProviderUpsertOptions = new ResourceProviderUpsertOptions
                {
                    Parameters = new()
                    {
                        { AzureOpenAIResourceProviderUpsertParameterNames.AgentObjectId, agent.ObjectId! },
                        { AzureOpenAIResourceProviderUpsertParameterNames.ConversationId, conversationId },
                        { AzureOpenAIResourceProviderUpsertParameterNames.OpenAIAssistantId, openAIAssistantsAssistantId! },
                        { AzureOpenAIResourceProviderUpsertParameterNames.MustCreateOpenAIAssistantThread, false }
                    }
                };

                var existsResult =
                    await azureOpenAIResourceProvider.ResourceExistsAsync<AzureOpenAIConversationMapping>(instanceId, conversationId, currentUserIdentity);

                if (existsResult.Exists && existsResult.Deleted)
                    throw new OrchestrationException($"The conversation mapping for conversation {conversationId} was deleted but not purged. It cannot be used for active conversations.");

                var conversationMapping = existsResult.Exists
                    ? await azureOpenAIResourceProvider.GetResourceAsync<AzureOpenAIConversationMapping>(instanceId, conversationId, currentUserIdentity)
                    : new AzureOpenAIConversationMapping
                    {
                        Name = conversationId,
                        Id = conversationId,
                        UPN = currentUserIdentity.UPN!,
                        InstanceId = instanceId,
                        ConversationId = conversationId,
                        OpenAIEndpoint = apiEndpointConfiguration!.Url,
                        OpenAIAssistantsAssistantId = openAIAssistantsAssistantId!,

                    };

                string? vectorStoreId;

                if (string.IsNullOrWhiteSpace(conversationMapping.OpenAIAssistantsThreadId))
                {
                    // We're either in the case of creating a new conversation mapping or the OpenAI thread identifier is missing.
                    // This can happen if previous attempts of creating the OpenAI thread failed.
                    // Either way we need to force an update to ensure we're attempting to create the OpenAI thread.

                    resourceProviderUpsertOptions.Parameters[AzureOpenAIResourceProviderUpsertParameterNames.MustCreateOpenAIAssistantThread] = true;

                    // We need to update the conversation mapping.
                    // We will rely on the upsert operation result to fill in the OpenAI assistant-related properties.
                    // We expect to get back valid values for the OpenAI Assistants thread identifier and OpenAI vector store identifier.

                    var result = await azureOpenAIResourceProvider.UpsertResourceAsync<AzureOpenAIConversationMapping, AzureOpenAIConversationMappingUpsertResult>(
                        instanceId,
                        conversationMapping,
                        currentUserIdentity,
                        resourceProviderUpsertOptions);

                    if (string.IsNullOrWhiteSpace(result.NewOpenAIAssistantThreadId))
                        throw new OrchestrationException("The OpenAI assistant thread ID was not returned.");
                    else
                        explodedObjects[CompletionRequestObjectsKeys.OpenAIAssistantsThreadId] = result.NewOpenAIAssistantThreadId;

                    vectorStoreId = result.NewOpenAIVectorStoreId;
                }
                else
                {
                    explodedObjects[CompletionRequestObjectsKeys.OpenAIAssistantsAssistantId] = conversationMapping.OpenAIAssistantsAssistantId!;
                    explodedObjects[CompletionRequestObjectsKeys.OpenAIAssistantsThreadId] = conversationMapping.OpenAIAssistantsThreadId!;
                    vectorStoreId = conversationMapping.OpenAIVectorStoreId;
                }

                return vectorStoreId;
            }

            return null;
        }
    }
}
