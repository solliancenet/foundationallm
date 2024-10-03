using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Common.Models.ResourceProviders.AzureOpenAI;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Orchestration.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using Microsoft.Extensions.DependencyInjection;

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

            var threadId = result.ExplodedObjects?[CompletionRequestObjectsKeys.OpenAIAssistantThreadId] as string;

            if (result.Agent.AgentType == typeof(KnowledgeManagementAgent) || result.Agent.AgentType == typeof(AudioClassificationAgent))
            {
                var orchestrationName = string.IsNullOrWhiteSpace(result.Agent.OrchestrationSettings?.Orchestrator)
                    ? LLMOrchestrationServiceNames.LangChain
                    : result.Agent.OrchestrationSettings?.Orchestrator;

                var orchestrationService = llmOrchestrationServiceManager.GetService(instanceId, orchestrationName!, serviceProvider, callContext);

                var kmOrchestration = new KnowledgeManagementOrchestration(
                    instanceId,
                    (KnowledgeManagementAgent)result.Agent,
                    result.ExplodedObjects ?? [],
                    callContext,
                    orchestrationService,
                    loggerFactory.CreateLogger<OrchestrationBase>(),
                    serviceProvider.GetRequiredService<IHttpClientFactoryService>(),
                    resourceProviderServices,
                    result.DataSourceAccessDenied,
                    threadId,
                    vectorStoreId);

                return kmOrchestration;
            }

            return null;
        }

        private static async Task<(AgentBase? Agent, Dictionary<string, object>? ExplodedObjects, bool DataSourceAccessDenied)> LoadAgent(
            string instanceId,
            string agentName,
            string? sessionId,
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

            foreach (var endpointKey in agentBase.APIEndpointConfigurationObjectIds.Keys)
            {
                var apiEndpoint = await configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(
                    instanceId,
                    agentBase.APIEndpointConfigurationObjectIds[endpointKey],
                    currentUserIdentity);

                explodedObjects[endpointKey] = apiEndpoint;
            }

            #region Knowledge management processing

            if (agentBase.AgentType == typeof(KnowledgeManagementAgent) || agentBase.AgentType == typeof(AudioClassificationAgent))
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

                        var indexingProfile = await vectorizationResourceProvider.GetResourceAsync<IndexingProfile>(
                            indexingProfileName,
                            currentUserIdentity);
                       
                        if (indexingProfile == null)
                            throw new OrchestrationException($"The indexing profile {indexingProfileName} is not a valid indexing profile.");

                        explodedObjects[indexingProfileName] = indexingProfile;
                                               
                        // Provide the indexing profile API endpoint configuration.
                        if (indexingProfile.Settings == null)
                            throw new OrchestrationException($"The settings for the indexing profile {indexingProfileName} were not found. Must include \"{VectorizationSettingsNames.IndexingProfileApiEndpointConfigurationObjectId}\" setting.");

                        if(indexingProfile.Settings.TryGetValue(VectorizationSettingsNames.IndexingProfileApiEndpointConfigurationObjectId, out var apiEndpointConfigurationObjectId) == false)
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

            return (agentBase, explodedObjects, false);
        }

        private static async Task<string?> EnsureAgentCapabilities(
            string instanceId,
            AgentBase agent,
            string sessionId,
            Dictionary<string, object> explodedObjects,
            Dictionary<string, IResourceProviderService> resourceProviderServices,
            UnifiedUserIdentity currentUserIdentity,
            ILogger<OrchestrationBuilder> logger)
        {
            if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_AzureOpenAI, out var azureOpenAIResourceProvider))
                throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_AzureOpenAI} was not loaded.");

            string? vectorStoreId = null;
            var prompt = explodedObjects[agent.PromptObjectId!] as MultipartPrompt;
            var aiModel = explodedObjects[agent.AIModelObjectId!] as AIModelBase;
            var apiEndpointConfiguration = explodedObjects[aiModel!.EndpointObjectId!] as APIEndpointConfiguration;

            if (agent.HasCapability(AgentCapabilityCategoryNames.OpenAIAssistants))
            {
                var assistantUserContextName = $"{currentUserIdentity.UPN?.NormalizeUserPrincipalName() ?? currentUserIdentity.UserId}-assistant-{instanceId.ToLower()}";

                var nameCheckResult = await azureOpenAIResourceProvider.ResourceExists<AssistantUserContext>(
                    instanceId,
                    assistantUserContextName,
                    currentUserIdentity);

                if (!nameCheckResult.Exists)
                {
                    var result = await azureOpenAIResourceProvider.UpsertResourceAsync<AssistantUserContext, AssistantUserContextUpsertResult>(
                        instanceId,
                        new AssistantUserContext
                        {
                            Name = assistantUserContextName,
                            UserPrincipalName = currentUserIdentity.UPN ?? currentUserIdentity.UserId!,
                            Endpoint = apiEndpointConfiguration!.Url,
                            ModelDeploymentName = aiModel.DeploymentName!,
                            Prompt = prompt!.Prefix!,
                            Conversations = new()
                            {
                                {
                                    sessionId!,
                                    new ConversationMapping
                                    {
                                        FoundationaLLMSessionId = sessionId!
                                    }
                                }
                            }
                        },
                        currentUserIdentity);

                    if (!string.IsNullOrWhiteSpace(result.NewOpenAIAssistantId))
                        explodedObjects[CompletionRequestObjectsKeys.OpenAIAssistantId] = result.NewOpenAIAssistantId;

                    if (!string.IsNullOrWhiteSpace(result.NewOpenAIAssistantThreadId))
                        explodedObjects[CompletionRequestObjectsKeys.OpenAIAssistantThreadId] = result.NewOpenAIAssistantThreadId;

                    vectorStoreId = result.NewOpenAIAssistantVectorStoreId;
                }
                else
                {
                    var assistantUserContext = await azureOpenAIResourceProvider.GetResourceAsync<AssistantUserContext>(
                        instanceId,
                        assistantUserContextName,
                        currentUserIdentity);

                    explodedObjects[CompletionRequestObjectsKeys.OpenAIAssistantId] = assistantUserContext.OpenAIAssistantId!;

                    if (!assistantUserContext.Conversations.TryGetValue(sessionId!,
                            out ConversationMapping? assistantConversation))
                    {
                        assistantUserContext.Conversations.Add(
                            sessionId!,
                            new ConversationMapping
                            {
                                FoundationaLLMSessionId = sessionId!
                            });
                    }

                    if (assistantConversation == null ||
                        string.IsNullOrWhiteSpace(assistantConversation.OpenAIThreadId))
                    {
                        var result = await azureOpenAIResourceProvider
                            .UpsertResourceAsync<AssistantUserContext, AssistantUserContextUpsertResult>(
                                instanceId,
                                assistantUserContext,
                                currentUserIdentity);

                        if (!string.IsNullOrWhiteSpace(result.NewOpenAIAssistantThreadId))
                            explodedObjects[CompletionRequestObjectsKeys.OpenAIAssistantThreadId] =
                                result.NewOpenAIAssistantThreadId;

                        vectorStoreId = result.NewOpenAIAssistantVectorStoreId;
                    }
                    else
                    {
                        explodedObjects[CompletionRequestObjectsKeys.OpenAIAssistantThreadId] = assistantConversation.OpenAIThreadId;
                        vectorStoreId = assistantConversation.OpenAIVectorStoreId;
                    }
                }
            }

            return vectorStoreId;
        }
    }
}
