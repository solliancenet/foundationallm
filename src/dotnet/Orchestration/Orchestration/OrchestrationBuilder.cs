using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows;
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
using System.Net;
using System.Text.Json;

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
        /// <param name="templatingService">The <see cref="ITemplatingService"/> used to render templates.</param>
        /// <param name="codeExecutionService">The <see cref="ICodeExecutionService"/> used to execute code.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> provding dependency injection services for the current scope.</param>
        /// <param name="loggerFactory">The logger factory used to create new loggers.</param>
        /// <param name="completionRequestObserver">An optional observer for completion requests.</param>
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
            ITemplatingService templatingService,
            ICodeExecutionService codeExecutionService,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory,
            Func<LLMCompletionRequest, Task>? completionRequestObserver = null)
        {
            var logger = loggerFactory.CreateLogger<OrchestrationBuilder>();

            var result = await LoadAgent(
                instanceId,
                agentName,
                originalRequest.SessionId,
                originalRequest.Settings?.ModelParameters,
                resourceProviderServices,
                templatingService,
                codeExecutionService,
                callContext.CurrentUserIdentity!,
                logger);

            if (result.Agent == null) return null;

            var vectorStoreId = await EnsureAgentCapabilities(
                instanceId,
                result.Agent,
                originalRequest.SessionId!,
                result.ExplodedObjectsManager,
                resourceProviderServices,
                callContext.CurrentUserIdentity!,
                logger);

            if (result.Agent.AgentType == typeof(KnowledgeManagementAgent))
            {
                var orchestrator = string.IsNullOrWhiteSpace(
                    result.Agent.Workflow?.WorkflowHost
                    ?? result.Agent.OrchestrationSettings?.Orchestrator) // TODO: Remove this fallback path after all agents are upgraded.
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
                    result.ExplodedObjectsManager.GetExplodedObjects() ?? [],
                    callContext,
                    orchestrationService,
                    loggerFactory.CreateLogger<OrchestrationBase>(),
                    serviceProvider.GetRequiredService<IHttpClientFactoryService>(),
                    resourceProviderServices,
                    result.DataSourceAccessDenied,
                    vectorStoreId,
                    completionRequestObserver);

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
                null,
                null);

            return kmOrchestration;
        }

        private static async Task<(AgentBase? Agent, AIModelBase? AIModel, APIEndpointConfiguration? APIEndpointConfiguration,  ExplodedObjectsManager ExplodedObjectsManager, bool DataSourceAccessDenied)> LoadAgent(
            string instanceId,
            string agentName,
            string? conversationId,
            Dictionary<string, object>? modelParameterOverrides,
            Dictionary<string, IResourceProviderService> resourceProviderServices,
            ITemplatingService templatingService,
            ICodeExecutionService codeExecutionService,
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

            var explodedObjectsManager = new ExplodedObjectsManager();

            var agentBase = await agentResourceProvider.GetResourceAsync<AgentBase>(
                $"/{AgentResourceTypeNames.Agents}/{agentName}",
                currentUserIdentity);

            var agentWorkflow = agentBase.Workflow;
            AIModelBase? mainAIModel = null;
            APIEndpointConfiguration? mainAIModelAPIEndpointConfiguration = null;
                      
            if (agentWorkflow is not null)
            {
                foreach (var resourceObjectId in agentWorkflow.ResourceObjectIds.Values)
                {
                    var resourcePath = ResourcePath.GetResourcePath(resourceObjectId.ObjectId);
                    switch (resourcePath.MainResourceTypeName)
                    {
                        case AIModelResourceTypeNames.AIModels:
                            var retrievedAIModel = await aiModelResourceProvider.GetResourceAsync<AIModelBase>(
                                            resourceObjectId.ObjectId,
                                            currentUserIdentity);
                            var retrievedAPIEndpointConfiguration = await configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(
                                                    retrievedAIModel.EndpointObjectId!,
                                                    currentUserIdentity);

                            mainAIModel = retrievedAIModel;
                            mainAIModelAPIEndpointConfiguration = retrievedAPIEndpointConfiguration;

                            // Check if the AI model is the main model, if so check for overrides.
                            if (resourceObjectId.Properties.TryGetValue(ResourceObjectIdPropertyNames.ObjectRole, out var aiModelObjectRole)
                                && ((JsonElement)aiModelObjectRole).GetString() == ResourceObjectIdPropertyValues.MainModel)
                            {
                                // Agent Workflow AI Model overrides.
                                if (resourceObjectId.Properties.TryGetValue(ResourceObjectIdPropertyNames.ModelParameters, out var modelParameters)
                                    && modelParameters != null)
                                {
                                    // Allowing the override only for the keys that are supported.
                                    var modelParamsDict = JsonSerializer.Deserialize<Dictionary<string, object>>(((JsonElement)modelParameters).GetRawText());
                                    foreach (var key in modelParamsDict!.Keys.Where(k => ModelParametersKeys.All.Contains(k)))
                                    {
                                        retrievedAIModel.ModelParameters[key] = modelParamsDict[key];
                                    }
                                }
                            }
                            explodedObjectsManager.TryAdd(
                                  retrievedAIModel.ObjectId!,
                                  retrievedAIModel);
                            explodedObjectsManager.TryAdd(
                                retrievedAIModel.EndpointObjectId!,
                                retrievedAPIEndpointConfiguration);
                            break;
                        case PromptResourceTypeNames.Prompts:                           
                            var retrievedPrompt = await promptResourceProvider.GetResourceAsync<PromptBase>(
                                        resourceObjectId.ObjectId,
                                        currentUserIdentity);

                            if (retrievedPrompt is MultipartPrompt multipartPrompt)
                            {
                                //check for token replacements, multipartPrompt variable has the same reference as retrievedPrompt therefore this edits the prefix/suffix in place
                                if (multipartPrompt is not null)
                                {
                                        
                                    multipartPrompt.Prefix = templatingService.Transform(multipartPrompt.Prefix!);
                                    multipartPrompt.Suffix = templatingService.Transform(multipartPrompt.Suffix!);
                                }
                            }
                            explodedObjectsManager.TryAdd(
                                retrievedPrompt.ObjectId!,
                                retrievedPrompt);                            
                            break;
                        case AgentResourceTypeNames.Workflows:

                            var retrievedWorkflow = await agentResourceProvider.GetResourceAsync<Workflow>(
                                resourceObjectId.ObjectId,
                                currentUserIdentity);

                            explodedObjectsManager.TryAdd(
                                retrievedWorkflow.ObjectId!,
                                retrievedWorkflow);

                            break;
                    }
                }

                if (agentWorkflow is AzureOpenAIAssistantsAgentWorkflow azureOpenAIAssistantsWorkflow)
                {
                    explodedObjectsManager.TryAdd(
                        CompletionRequestObjectsKeys.OpenAIAssistantsAssistantId,
                        azureOpenAIAssistantsWorkflow.AssistantId
                            ?? throw new OrchestrationException("The OpenAI Assistants assistant identifier was not found in the agent workflow."));
                }
            }
            else
            {
                #region Legacy agent loading code

                var prompt = await promptResourceProvider.GetResourceAsync<PromptBase>(
                    agentBase.PromptObjectId!,
                    currentUserIdentity);
                var aiModel = await aiModelResourceProvider.GetResourceAsync<AIModelBase>(
                    agentBase.AIModelObjectId!,
                    currentUserIdentity);
                var apiEndpointConfiguration = await configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(
                    aiModel.EndpointObjectId!,
                    currentUserIdentity);

                mainAIModel = aiModel;
                mainAIModelAPIEndpointConfiguration = apiEndpointConfiguration;
                explodedObjectsManager.TryAdd(agentBase.PromptObjectId!, prompt);
                explodedObjectsManager.TryAdd(agentBase.AIModelObjectId!, aiModel);
                explodedObjectsManager.TryAdd(aiModel.EndpointObjectId!, apiEndpointConfiguration);

                // Merge the model parameter overrides with the existing model parameter values from the AI model.
                if (modelParameterOverrides != null)
                {
                    // Allowing the override only for the keys that are supported.
                    foreach (var key in modelParameterOverrides.Keys.Where(k => ModelParametersKeys.All.Contains(k)))
                    {
                        aiModel.ModelParameters[key] = modelParameterOverrides[key];
                    }
                }

                if (agentBase.HasCapability(AgentCapabilityCategoryNames.OpenAIAssistants))
                {
                    explodedObjectsManager.TryAdd(
                        CompletionRequestObjectsKeys.OpenAIAssistantsAssistantId,
                        agentBase.Properties?.GetValueOrDefault(AgentPropertyNames.AzureOpenAIAssistantId)
                            ?? throw new OrchestrationException("The OpenAI Assistants assistant identifier was not found in the agent properties."));
                }

                #endregion
            }

            var gatewayAPIEndpointConfiguration = await configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(
                instanceId,
                "GatewayAPI",
                currentUserIdentity);

            explodedObjectsManager.TryAdd(
                CompletionRequestObjectsKeys.GatewayAPIEndpointConfiguration,
                gatewayAPIEndpointConfiguration);

            // TODO: New agent-to-agent conversations model is in development. Until then, no need to send the list of all agents and their descriptions..

            //var allAgents = await agentResourceProvider.GetResourcesAsync<AgentBase>(instanceId, currentUserIdentity);
            //var allAgentsDescriptions = allAgents
            //    .Where(a => !string.IsNullOrWhiteSpace(a.Resource.Description) && a.Resource.Name != agentBase.Name)
            //    .Select(a => new
            //    {
            //        a.Resource.Name,
            //        a.Resource.Description
            //    })
            //    .ToDictionary(x => x.Name, x => x.Description);
            //explodedObjects[CompletionRequestObjectsKeys.AllAgents] = allAgentsDescriptions;

            #region Tools

            List<string> toolNames = [];

            foreach (var tool in agentBase.Tools)
            {
                toolNames.Add(tool.Name);

                // Build the tool parameters dictionary.
                Dictionary<string, object> toolParameters = [];

                if (tool.TryGetPropertyValue<bool>(
                        AgentToolPropertyNames.FoundationaLLM_AzureContainerApps_CodeExecution_Enabled, out bool requiresCodeExecution)
                    && requiresCodeExecution)
                {
                    var codeExecutionSession = await codeExecutionService.CreateCodeExecutionSession(
                        instanceId,
                        tool.Name,
                        conversationId!,
                        currentUserIdentity);

                    toolParameters.Add(
                        AgentToolPropertyNames.FoundationaLLM_AzureContainerApps_CodeExecution_Endpoint,
                        codeExecutionSession.Endpoint);
                    toolParameters.Add(
                        AgentToolPropertyNames.FoundationaLLM_AzureContainerApps_CodeExecution_SessionId,
                        codeExecutionSession.SessionId);
                }

                explodedObjectsManager.TryAdd(
                    tool.Name,
                    toolParameters);

                // Ensure all resource object identifiers are exploded.
                foreach (var resourceObjectId in tool.ResourceObjectIds.Values)
                {
                    var resourcePath = ResourcePath.GetResourcePath(resourceObjectId.ObjectId);

                    // No need to explode objects that have already been exploded.
                    if (explodedObjectsManager.HasKey(resourceObjectId.ObjectId))
                        continue;

                    switch (resourcePath.MainResourceTypeName)
                    {
                        case AIModelResourceTypeNames.AIModels:

                            var aiModel = await aiModelResourceProvider.GetResourceAsync<AIModelBase>(
                                resourceObjectId.ObjectId,
                                currentUserIdentity);

                            explodedObjectsManager.TryAdd(
                                resourceObjectId.ObjectId,
                                aiModel);

                            // TODO: Improve handling to allow each tool to override model parameters separately
                            if (!string.IsNullOrEmpty(aiModel.EndpointObjectId) && !explodedObjectsManager.HasKey(aiModel.EndpointObjectId))
                            {
                                var aiModelEndpoint = await configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(
                                    aiModel.EndpointObjectId!,
                                    currentUserIdentity);

                                explodedObjectsManager.TryAdd(
                                    aiModel.EndpointObjectId!,
                                    aiModelEndpoint);
                            }

                            break;

                        case ConfigurationResourceTypeNames.APIEndpointConfigurations:
                            var apiEndpoint = await configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(
                            resourceObjectId.ObjectId,
                            currentUserIdentity);

                            explodedObjectsManager.TryAdd(
                                resourceObjectId.ObjectId,
                                apiEndpoint);

                            break;

                        case VectorizationResourceTypeNames.IndexingProfiles:
                            var indexingProfile = await vectorizationResourceProvider.GetResourceAsync<IndexingProfile>(
                                resourceObjectId.ObjectId,
                                currentUserIdentity);

                            explodedObjectsManager.TryAdd(
                                resourceObjectId.ObjectId,
                                indexingProfile);

                            if (indexingProfile.Settings == null)
                                throw new OrchestrationException($"Tool: {tool.Name}: Settings for indexing profile {indexingProfile.Name} not found.");

                            if (!indexingProfile.Settings.TryGetValue(VectorizationSettingsNames.IndexingProfileApiEndpointConfigurationObjectId, out var apiEndpointConfigurationObjectId))
                                throw new OrchestrationException($"Tool: {tool.Name}: API endpoint configuration ID not found in indexing profile settings.");

                            if (!explodedObjectsManager.HasKey(apiEndpointConfigurationObjectId))
                            {
                                // Explode the object only if it hasn't been exploded yet.

                                var indexingProfileApiEndpoint = await configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(
                                    apiEndpointConfigurationObjectId!,
                                    currentUserIdentity);

                                explodedObjectsManager.TryAdd(
                                    apiEndpointConfigurationObjectId,
                                    indexingProfileApiEndpoint);
                            }

                            break;
                        case PromptResourceTypeNames.Prompts:
                            var prompt = await promptResourceProvider.GetResourceAsync<PromptBase>(
                                resourceObjectId.ObjectId,
                                currentUserIdentity);
                            if (prompt is MultipartPrompt multipartPrompt)
                            {
                                // prompt template token replacement
                                if (multipartPrompt is not null)
                                {

                                    multipartPrompt.Prefix = templatingService.Transform(multipartPrompt.Prefix!);
                                    multipartPrompt.Suffix = templatingService.Transform(multipartPrompt.Suffix!);
                                }
                            }
                            explodedObjectsManager.TryAdd(
                                resourceObjectId.ObjectId,
                                prompt);
                            break;

                        default:
                            throw new OrchestrationException($"Unknown resource type '{resourcePath.MainResourceTypeName}'.");
                    }
                }
            }

            explodedObjectsManager.TryAdd(
                CompletionRequestObjectsKeys.ToolNames,
                toolNames);

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

                        explodedObjectsManager.TryAdd(
                            indexingProfileName,
                            indexingProfile);
                                               
                        // Provide the indexing profile API endpoint configuration.
                        if (indexingProfile.Settings == null)
                            throw new OrchestrationException($"The settings for the indexing profile {indexingProfileName} were not found. Must include \"{VectorizationSettingsNames.IndexingProfileApiEndpointConfigurationObjectId}\" setting.");

                        if(indexingProfile.Settings.TryGetValue(VectorizationSettingsNames.IndexingProfileApiEndpointConfigurationObjectId, out var apiEndpointConfigurationObjectId) == false)
                            throw new OrchestrationException($"The API endpoint configuration object ID was not found in the settings of the indexing profile.");

                        if (!explodedObjectsManager.HasKey(apiEndpointConfigurationObjectId))
                        {
                            // Explode the object only if it hasn't been exploded yet.

                            var indexingProfileAPIEndpointConfiguration = await configurationResourceProvider.GetResourceAsync<APIEndpointConfiguration>(
                            apiEndpointConfigurationObjectId,
                            currentUserIdentity);

                            explodedObjectsManager.TryAdd(
                                apiEndpointConfigurationObjectId,
                                indexingProfileAPIEndpointConfiguration);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(kmAgent.Vectorization.TextEmbeddingProfileObjectId)
                        && !explodedObjectsManager.HasKey(kmAgent.Vectorization.TextEmbeddingProfileObjectId))
                    {
                        var textEmbeddingProfile = await vectorizationResourceProvider.GetResourceAsync<TextEmbeddingProfile>(
                            kmAgent.Vectorization.TextEmbeddingProfileObjectId,
                            currentUserIdentity)
                            ?? throw new OrchestrationException($"The text embedding profile {kmAgent.Vectorization.TextEmbeddingProfileObjectId} is not a valid text embedding profile.");

                        explodedObjectsManager.TryAdd(
                            kmAgent.Vectorization.TextEmbeddingProfileObjectId!,
                            textEmbeddingProfile);
                    }
                }
            }

            #endregion

            return (agentBase, mainAIModel, mainAIModelAPIEndpointConfiguration, explodedObjectsManager, false);
        }

        private static async Task<string?> EnsureAgentCapabilities(
            string instanceId,
            AgentBase agent,
            string conversationId,
            ExplodedObjectsManager explodedObjectsManager,
            Dictionary<string, IResourceProviderService> resourceProviderServices,
            UnifiedUserIdentity currentUserIdentity,
            ILogger<OrchestrationBuilder> logger)
        {
            if ( agent.Workflow is not null)
            {
                if (agent.Workflow is AzureOpenAIAssistantsAgentWorkflow)
                {
                    if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_AzureOpenAI, out var azureOpenAIResourceProvider))
                        throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_AzureOpenAI} was not loaded.");

                    var mainAIModelObjectId = agent.Workflow.MainAIModelObjectId;
                    explodedObjectsManager.TryGet<AIModelBase>(mainAIModelObjectId!, out AIModelBase? aiModel);
                    explodedObjectsManager.TryGet<APIEndpointConfiguration>(aiModel!.EndpointObjectId!, out APIEndpointConfiguration? apiEndpointConfiguration);
                    explodedObjectsManager.TryGet<string>(CompletionRequestObjectsKeys.OpenAIAssistantsAssistantId, out string? openAIAssistantsAssistantId);

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
                            explodedObjectsManager.TryAdd(
                                CompletionRequestObjectsKeys.OpenAIAssistantsThreadId,
                                result.NewOpenAIAssistantThreadId);

                        vectorStoreId = result.NewOpenAIVectorStoreId;
                    }
                    else
                    {
                        explodedObjectsManager.TryAdd(
                            CompletionRequestObjectsKeys.OpenAIAssistantsAssistantId,
                            conversationMapping.OpenAIAssistantsAssistantId!);
                        explodedObjectsManager.TryAdd(
                            CompletionRequestObjectsKeys.OpenAIAssistantsThreadId,
                            conversationMapping.OpenAIAssistantsThreadId!);
                        vectorStoreId = conversationMapping.OpenAIVectorStoreId;
                    }

                    return vectorStoreId;
                }
            }
            else
            {
                #region Legacy agent code

                if (agent.HasCapability(AgentCapabilityCategoryNames.OpenAIAssistants))
                {
                    if (!resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_AzureOpenAI, out var azureOpenAIResourceProvider))
                        throw new OrchestrationException($"The resource provider {ResourceProviderNames.FoundationaLLM_AzureOpenAI} was not loaded.");

                    explodedObjectsManager.TryGet<MultipartPrompt>(agent.PromptObjectId!, out MultipartPrompt? prompt);
                    explodedObjectsManager.TryGet<AIModelBase>(agent.AIModelObjectId!, out AIModelBase? aiModel);
                    explodedObjectsManager.TryGet<APIEndpointConfiguration>(aiModel!.EndpointObjectId!, out APIEndpointConfiguration? apiEndpointConfiguration);
                    explodedObjectsManager.TryGet<string>(CompletionRequestObjectsKeys.OpenAIAssistantsAssistantId, out string? openAIAssistantsAssistantId);

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
                            explodedObjectsManager.TryAdd(
                                CompletionRequestObjectsKeys.OpenAIAssistantsThreadId,
                                result.NewOpenAIAssistantThreadId);

                        vectorStoreId = result.NewOpenAIVectorStoreId;
                    }
                    else
                    {
                        explodedObjectsManager.TryAdd(
                            CompletionRequestObjectsKeys.OpenAIAssistantsAssistantId,
                            conversationMapping.OpenAIAssistantsAssistantId!);
                        explodedObjectsManager.TryAdd(
                            CompletionRequestObjectsKeys.OpenAIAssistantsThreadId,
                            conversationMapping.OpenAIAssistantsThreadId!);
                        vectorStoreId = conversationMapping.OpenAIVectorStoreId;
                    }

                    return vectorStoreId;
                }

                #endregion
            }  

            return null;
        }
    }
}
