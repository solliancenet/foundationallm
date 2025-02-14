using FluentValidation;
using FoundationaLLM.Agent.Models.Resources;
using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.Events;
using FoundationaLLM.Common.Constants.OpenAI;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentAccessTokens;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentFiles;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentWorkflows;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Common.Services.ResourceProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Agent.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.Agent resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationServiceClient"/> providing authorization services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="cosmosDBService">The <see cref="IAzureCosmosDBService"/> providing Cosmos DB services.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to provide loggers for logging.</param>
    public class AgentResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IAuthorizationServiceClient authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Agent)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IAzureCosmosDBService cosmosDBService,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory)
        : ResourceProviderServiceBase<AgentReference>(
            instanceOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            loggerFactory.CreateLogger<AgentResourceProviderService>(),
            eventTypesToSubscribe: [
                EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand
            ],
            useInternalReferencesStore: true)
    {
        private readonly IAzureCosmosDBService _cosmosDBService = cosmosDBService;

        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            AgentResourceProviderMetadata.AllowedResourceTypes;

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Agent;

        /// <inheritdoc/>
        protected override async Task InitializeInternal() =>
            await Task.CompletedTask;

        #region Resource provider support for Management API

        /// <inheritdoc/>
        protected override async Task<object> GetResourcesAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions? options = null) =>
            resourcePath.ResourceTypeName switch
            {
                AgentResourceTypeNames.Agents => await LoadResources<AgentBase>(
                    resourcePath.ResourceTypeInstances[0],
                    authorizationResult,
                    options ?? new ResourceProviderGetOptions
                    {
                        IncludeRoles = resourcePath.IsResourceTypePath,
                    }),
                AgentResourceTypeNames.Workflows => await LoadResources<Workflow>(
                    resourcePath.ResourceTypeInstances[0],
                    authorizationResult,
                    options ?? new ResourceProviderGetOptions
                    {
                        IncludeRoles = resourcePath.IsResourceTypePath,
                    }),
                AgentResourceTypeNames.Tools => await LoadResources<Tool>(
                   resourcePath.ResourceTypeInstances[0],
                   authorizationResult,
                   options ?? new ResourceProviderGetOptions
                   {
                       IncludeRoles = resourcePath.IsResourceTypePath,
                   }),
                AgentResourceTypeNames.AgentAccessTokens => await LoadAgentAccessTokens(resourcePath)!,
                AgentResourceTypeNames.AgentFiles => await LoadAgentFiles(resourcePath, userIdentity, options)!,
                AgentResourceTypeNames.AgentFileToolAssociations => await LoadAgentFileToolAssociations(resourcePath, userIdentity)!,
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        /// <inheritdoc/>
        protected override async Task<object> UpsertResourceAsync(ResourcePath resourcePath, string? serializedResource, ResourceProviderFormFile? formFile, UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeName switch
            {
                AgentResourceTypeNames.Agents => await UpdateAgent(resourcePath, serializedResource!, userIdentity),
                AgentResourceTypeNames.AgentFiles => await UpdateAgentFile(resourcePath, formFile!, userIdentity),
                AgentResourceTypeNames.AgentAccessTokens => await UpdateAgentAccessToken(resourcePath, serializedResource!),
                AgentResourceTypeNames.AgentFileToolAssociations => await UpdateFileToolAssociations(resourcePath, serializedResource!, userIdentity),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        /// <inheritdoc/>
        protected override async Task<object> ExecuteActionAsync(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            string serializedAction,
            UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeName switch
            {
                AgentResourceTypeNames.Agents => resourcePath.Action switch
                {
                    ResourceProviderActions.CheckName => await CheckResourceName<AgentBase>(
                        JsonSerializer.Deserialize<ResourceName>(serializedAction)!),

                    ResourceProviderActions.Purge => await PurgeResource<AgentBase>(resourcePath),

                    ResourceProviderActions.SetDefault => await SetDefaultResource<AgentBase>(resourcePath),

                    _ => throw new ResourceProviderException($"The action {resourcePath.Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                AgentResourceTypeNames.AgentAccessTokens => resourcePath.Action switch
                {
                    ResourceProviderActions.Validate => await ValidateAgentAccessToken(
                        resourcePath,
                        JsonSerializer.Deserialize<AgentAccessTokenValidationRequest>(serializedAction)!,
                        userIdentity),

                    _ => throw new ResourceProviderException($"The action {resourcePath.Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                _ => throw new ResourceProviderException()
            };

        /// <inheritdoc/>
        protected override async Task DeleteResourceAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            switch (resourcePath.ResourceTypeName)
            {
                case AgentResourceTypeNames.Agents:
                    await DeleteResource<AgentBase>(resourcePath);
                    break;
                case AgentResourceTypeNames.AgentAccessTokens:
                    await DeleteAgentAccessToken(resourcePath);
                    break;
                case AgentResourceTypeNames.AgentFiles:
                    var attachment = await _cosmosDBService.GetAgentFile(_instanceSettings.Id, resourcePath.MainResourceId!, resourcePath.ResourceId!)
                        ?? throw new ResourceProviderException($"The resource {resourcePath.ResourceTypeInstances[0].ResourceId!} of type {resourcePath.MainResourceTypeName} was not found.");

                    await _cosmosDBService.DeleteAgentFile(attachment);
                    break;
                default:
                    throw new ResourceProviderException(
                        $"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest);
            };
        }

        #endregion

        #region Resource provider strongly typed operations

        /// <inheritdoc/>
        protected override async Task<T> GetResourceAsyncInternal<T>(
            ResourcePath resourcePath, ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity, ResourceProviderGetOptions? options = null)
        {
            switch (resourcePath.ResourceTypeName)
            {
                case AgentResourceTypeNames.Agents:
                case AgentResourceTypeNames.Workflows:
                case AgentResourceTypeNames.Tools:
                    return (await LoadResource<T>(resourcePath.ResourceId!))!;
                case AgentResourceTypeNames.AgentFiles:
                    var agentPrivateFile = await _cosmosDBService.GetAgentFile(_instanceSettings.Id, resourcePath.MainResourceId!, resourcePath.ResourceId!)
                        ?? throw new ResourceProviderException($"The resource {resourcePath.ResourceTypeInstances[0].ResourceId!} of type {resourcePath.MainResourceTypeName} was not found.");

                    return (await LoadAgentFile(agentPrivateFile, loadContent: options?.LoadContent ?? false)) as T
                        ?? throw new ResourceProviderException($"The resource {resourcePath.ResourceTypeInstances[0].ResourceId!} of type {resourcePath.MainResourceTypeName} could not be loaded.");
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.MainResourceTypeName} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest);
            }
        }

        #endregion

        #region Resource management

        private async Task<ResourceProviderUpsertResult> UpdateAgent(ResourcePath resourcePath, string serializedAgent, UnifiedUserIdentity userIdentity)
        {
            var agent = JsonSerializer.Deserialize<AgentBase>(serializedAgent)
                ?? throw new ResourceProviderException("The object definition is invalid.",
                    StatusCodes.Status400BadRequest);

            var existingAgentReference = await _resourceReferenceStore!.GetResourceReference(agent.Name);

            if (resourcePath.ResourceTypeInstances[0].ResourceId != agent.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            var agentReference = new AgentReference
            {
                Name = agent.Name!,
                Type = agent.Type!,
                Filename = $"/{_name}/{agent.Name}.json",
                Deleted = false
            };

            agent.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);

            if (agent is KnowledgeManagementAgent { Vectorization.DedicatedPipeline: true, InlineContext: false } kmAgent)
            {
                var result = await GetResourceProviderServiceByName(ResourceProviderNames.FoundationaLLM_Vectorization)
                    .HandlePostAsync(
                        $"/instances/{_instanceSettings.Id}/providers/{ResourceProviderNames.FoundationaLLM_Vectorization}/{VectorizationResourceTypeNames.VectorizationPipelines}/{kmAgent.Name}",
                        JsonSerializer.Serialize<VectorizationPipeline>(new VectorizationPipeline
                        {
                            Name = kmAgent.Name,
                            Active = true,
                            Description = $"Vectorization data pipeline dedicated to the {kmAgent.Name} agent.",
                            DataSourceObjectId = kmAgent.Vectorization.DataSourceObjectId!,
                            TextPartitioningProfileObjectId = kmAgent.Vectorization.TextPartitioningProfileObjectId!,
                            TextEmbeddingProfileObjectId = kmAgent.Vectorization.TextEmbeddingProfileObjectId!,
                            IndexingProfileObjectId = kmAgent.Vectorization.IndexingProfileObjectIds![0],
                            TriggerType = (VectorizationPipelineTriggerType)kmAgent.Vectorization.TriggerType!,
                            TriggerCronSchedule = kmAgent.Vectorization.TriggerCronSchedule
                        }),
                        null,
                        userIdentity);

                if ((result is ResourceProviderUpsertResult resourceProviderResult)
                    && !string.IsNullOrWhiteSpace(resourceProviderResult.ObjectId))
                    kmAgent.Vectorization.VectorizationDataPipelineObjectId = resourceProviderResult.ObjectId;
                else
                    throw new ResourceProviderException("There was an error attempting to create the associated vectorization pipeline for the agent.",
                        StatusCodes.Status500InternalServerError);
            }


            if (agent.Workflow is AzureOpenAIAssistantsAgentWorkflow)
            {
                agent.Properties ??= [];

                (var openAIAssistantId, var openAIAssistantVectorStoreId, var workflowBase, var agentAIModel, var agentPrompt, var agentAIModelAPIEndpoint)
                    = await ResolveAgentProperties(agent, userIdentity);

                var workflow = (workflowBase as AzureOpenAIAssistantsAgentWorkflow)!;
                var gatewayClient = await GetGatewayServiceClient(userIdentity);

                if (string.IsNullOrWhiteSpace(openAIAssistantId))
                {
                    // The agent uses the Azure OpenAI Assistants workflow
                    // but it does not have an associated assistant.
                    // Proceed to create the Azure OpenAI Assistants assistant.

                    _logger.LogInformation(
                        "Starting to create the Azure OpenAI assistant for agent {AgentName}",
                        agent.Name);


                    #region Create Azure OpenAI Assistants assistant                   

                    Dictionary<string, object> parameters = new()
                        {
                            { OpenAIAgentCapabilityParameterNames.CreateOpenAIAssistant, true },
                            { OpenAIAgentCapabilityParameterNames.OpenAIEndpoint, agentAIModelAPIEndpoint.Url },
                            { OpenAIAgentCapabilityParameterNames.OpenAIModelDeploymentName, agentAIModel.DeploymentName! },
                            { OpenAIAgentCapabilityParameterNames.OpenAIAssistantPrompt, (agentPrompt as MultipartPrompt)!.Prefix! }
                        };

                    var agentCapabilityResult = await gatewayClient!.CreateAgentCapability(
                        _instanceSettings.Id,
                        AgentCapabilityCategoryNames.OpenAIAssistants,
                        $"FoundationaLLM - {agent.Name}",
                        parameters);

                    var newOpenAIAssistantId = default(string);

                    if (agentCapabilityResult.TryGetValue(OpenAIAgentCapabilityParameterNames.OpenAIAssistantId, out var newOpenAIAssistantIdObject)
                        && newOpenAIAssistantIdObject != null)
                        newOpenAIAssistantId = ((JsonElement)newOpenAIAssistantIdObject!).Deserialize<string>();

                    var newOpenAIAssistantVectorStoreId = default(string);
                    if (agentCapabilityResult.TryGetValue(OpenAIAgentCapabilityParameterNames.OpenAIVectorStoreId, out var newOpenAIAssistantVectorStoreIdObject)
                        && newOpenAIAssistantVectorStoreIdObject != null)
                        newOpenAIAssistantVectorStoreId = ((JsonElement)newOpenAIAssistantVectorStoreIdObject!).Deserialize<string>();

                    if (string.IsNullOrWhiteSpace(newOpenAIAssistantId))
                        throw new ResourceProviderException($"Could not create an Azure OpenAI assistant for the agent {agent} which requires it.",
                            StatusCodes.Status500InternalServerError);
                    if (string.IsNullOrWhiteSpace(newOpenAIAssistantVectorStoreId))
                        throw new ResourceProviderException($"Could not create an Azure OpenAI assistant vector store id for the agent {agent} which requires it.",
                            StatusCodes.Status500InternalServerError);

                    _logger.LogInformation(
                        $"The Azure OpenAI assistant {newOpenAIAssistantId} for agent {agent.Name} was created successfully with Vector Store: {newOpenAIAssistantVectorStoreId}.",
                        newOpenAIAssistantId, agent.Name);

                    workflow.VectorStoreId = newOpenAIAssistantVectorStoreId;
                    workflow.AssistantId = newOpenAIAssistantId;

                    #endregion
                }
                else
                {
                    // Verify if the assistant has a vector store id.                   
                    if (string.IsNullOrEmpty(workflow.VectorStoreId))
                    {
                        // Add vector store to existing assistant
                        Dictionary<string, object> parameters = new()
                        {
                            { OpenAIAgentCapabilityParameterNames.CreateOpenAIAssistantVectorStore, true },
                            { OpenAIAgentCapabilityParameterNames.OpenAIEndpoint, agentAIModelAPIEndpoint.Url },
                            { OpenAIAgentCapabilityParameterNames.OpenAIModelDeploymentName, agentAIModel.DeploymentName! },
                            { OpenAIAgentCapabilityParameterNames.OpenAIAssistantId, openAIAssistantId }
                        };

                        // Pass the existing assistant id as the capability name
                        var agentCapabilityResult = await gatewayClient!.CreateAgentCapability(
                            _instanceSettings.Id,
                            AgentCapabilityCategoryNames.OpenAIAssistants,
                            string.Empty,
                            parameters);

                        var newOpenAIAssistantVectorStoreId = default(string);
                        if (agentCapabilityResult.TryGetValue(OpenAIAgentCapabilityParameterNames.OpenAIVectorStoreId, out var newOpenAIAssistantVectorStoreIdObject)
                            && newOpenAIAssistantVectorStoreIdObject != null)
                            newOpenAIAssistantVectorStoreId = ((JsonElement)newOpenAIAssistantVectorStoreIdObject!).Deserialize<string>();
                        if (string.IsNullOrWhiteSpace(newOpenAIAssistantVectorStoreId))
                            throw new ResourceProviderException($"Could not create an Azure OpenAI assistant vector store id for the agent {agent} which requires it.",
                                StatusCodes.Status500InternalServerError);

                        workflow.VectorStoreId = newOpenAIAssistantVectorStoreId;
                    }
                }
            }

            var validator = _resourceValidatorFactory.GetValidator(agentReference.ResourceType);
            if (validator is IValidator agentValidator)
            {
                var context = new ValidationContext<object>(agent);
                var validationResult = await agentValidator.ValidateAsync(context);
                if (!validationResult.IsValid)
                {
                    throw new ResourceProviderException($"Validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}",
                        StatusCodes.Status400BadRequest);
                }
            }

            // Ensure the agent has a valid virtual security group identifier.
            if (string.IsNullOrWhiteSpace(agent.VirtualSecurityGroupId))
                agent.VirtualSecurityGroupId = Guid.NewGuid().ToString().ToLower();

            UpdateBaseProperties(agent, userIdentity, isNew: existingAgentReference == null);
            if (existingAgentReference == null)
                await CreateResource<AgentBase>(agentReference, agent);
            else
                await SaveResource<AgentBase>(existingAgentReference, agent);

            return new ResourceProviderUpsertResult
            {
                ObjectId = agent!.ObjectId,
                ResourceExists = existingAgentReference != null
            };
        }

        private async Task<List<ResourceProviderGetResult<AgentAccessToken>>> LoadAgentAccessTokens(ResourcePath resourcePath)
        {
            var agentClientSecretKey = new AgentClientSecretKey
            {
                InstanceId = resourcePath.InstanceId!,
                ContextId = string.Empty,
                Id = string.Empty,
                ClientSecret = string.Empty
            };
            agentClientSecretKey.SetContextId(resourcePath.MainResourceId!);

            var secretKeys = await _authorizationServiceClient.GetSecretKeys(
                agentClientSecretKey.InstanceId,
                agentClientSecretKey.ContextId);

            return secretKeys.Select(k => new ResourceProviderGetResult<AgentAccessToken>()
            {
                Actions = [],
                Roles = [],
                Resource = new AgentAccessToken()
                {
                    Id = new Guid(k.Id),
                    Name = k.Id,
                    Description = k.Description,
                    Active = k.Active,
                    ExpirationDate = k.ExpirationDate
                }
            }).ToList();
        }

        private async Task<ResourceProviderUpsertResult> UpdateAgentAccessToken(ResourcePath resourcePath, string serializedAgentAccessToken)
        {
            var agentAccessToken = JsonSerializer.Deserialize<AgentAccessToken>(serializedAgentAccessToken)
                ?? throw new ResourceProviderException("The object definition is invalid.",
                    StatusCodes.Status400BadRequest);

            var agentClientSecretKey = new AgentClientSecretKey
            {
                InstanceId = resourcePath.InstanceId!,
                ContextId = string.Empty,
                Id = resourcePath.ResourceId!,
                ClientSecret = string.Empty
            };
            agentClientSecretKey.SetContextId(resourcePath.MainResourceId!);

            var secretKeyValue = await _authorizationServiceClient.UpsertSecretKey(_instanceSettings.Id, new SecretKey()
            {
                Id = agentClientSecretKey.Id,
                InstanceId = agentClientSecretKey.InstanceId,
                ContextId = agentClientSecretKey.ContextId,
                Description = agentAccessToken.Description!,
                Active = agentAccessToken.Active,
                ExpirationDate = agentAccessToken.ExpirationDate!.Value
            });

            agentAccessToken.ObjectId = resourcePath.RawResourcePath;

            return new ResourceProviderUpsertResult
            {
                ObjectId = agentAccessToken!.ObjectId,
                ResourceExists = string.IsNullOrWhiteSpace(secretKeyValue),
                Resource = secretKeyValue
            };
        }

        private async Task DeleteAgentAccessToken(ResourcePath resourcePath)
        {
            var agentClientSecretKey = new AgentClientSecretKey
            {
                InstanceId = resourcePath.InstanceId!,
                ContextId = string.Empty,
                Id = resourcePath.ResourceId!,
                ClientSecret = string.Empty
            };
            agentClientSecretKey.SetContextId(resourcePath.MainResourceId!);

            await _authorizationServiceClient.DeleteSecretKey(
                agentClientSecretKey.InstanceId,
                agentClientSecretKey.ContextId,
                agentClientSecretKey.Id);
        }

        private async Task<AgentAccessTokenValidationResult> ValidateAgentAccessToken(
            ResourcePath resourcePath,
            AgentAccessTokenValidationRequest agentAccessTokenValidationRequest,
            UnifiedUserIdentity userIdentity)
        {
            var fallbackResult = new AgentAccessTokenValidationResult()
            {
                Valid = false
            };

            if (!ClientSecretKey.TryParse(agentAccessTokenValidationRequest.AccessToken, out var clientSecretKey)
                || clientSecretKey == null)
                return fallbackResult;

            var agentClientSecretKey = AgentClientSecretKey.FromClientSecretKey(clientSecretKey);

            if (!StringComparer.OrdinalIgnoreCase.Equals(agentClientSecretKey.AgentName, resourcePath.MainResourceId))
                return fallbackResult;

            var result = await _authorizationServiceClient.ValidateSecretKey(
                resourcePath.InstanceId!,
                agentClientSecretKey.ContextId,
                agentAccessTokenValidationRequest.AccessToken);

            if (result.Valid)
            {
                // Set virtual identity
                var agent = await GetResourceAsync<AgentBase>($"/instances/{resourcePath.InstanceId}/providers/{_name}/{AgentResourceTypeNames.Agents}/{agentClientSecretKey.AgentName}", userIdentity);
                var upn =
                    $"aat_{agentClientSecretKey.AgentName}_{agentClientSecretKey.Id}@foundationallm.internal_";

                // Warn if the agent's virtual security group identifier is not set.
                if (string.IsNullOrEmpty(agent.VirtualSecurityGroupId))
                {
                    _logger.LogWarning("An agent access token was used for the agent {AgentName} in instance {InstanceId} but the agent's virtual security group identifier is not set.",
                        agentClientSecretKey.AgentName,
                        resourcePath.InstanceId);
                }

                return new AgentAccessTokenValidationResult()
                {
                    Valid = result.Valid,
                    VirtualIdentity = new UnifiedUserIdentity()
                    {
                        UserId = agentClientSecretKey.Id,
                        Name = agentClientSecretKey.Id,
                        Username = agentClientSecretKey.Id,
                        UPN = upn,
                        GroupIds = string.IsNullOrWhiteSpace(agent.VirtualSecurityGroupId)
                            ? []
                            : [agent.VirtualSecurityGroupId],
                    }
                };
            }

            return fallbackResult;
        }

        private async Task<List<ResourceProviderGetResult<AgentFile>>> LoadAgentFiles(
            ResourcePath resourcePath, UnifiedUserIdentity userIdentity, ResourceProviderGetOptions? options = null)
        {
            var agentFiles = new List<AgentFileReference>();

            if (resourcePath.ResourceId != null)
                agentFiles = [await _cosmosDBService.GetAgentFile(_instanceSettings.Id, resourcePath.MainResourceId!, resourcePath.ResourceId!)];
            else
                agentFiles = await _cosmosDBService.GetAgentFiles(_instanceSettings.Id, resourcePath.MainResourceId!);

            var results = new List<AgentFile>();
            foreach (var agentFile in agentFiles)
                results.Add(await LoadAgentFile(agentFile, options != null && options!.LoadContent));

            return results.Select(r => new ResourceProviderGetResult<AgentFile>
            {
                Resource = r,
                Roles = [],
                Actions = []
            }).ToList();
        }

        private async Task<AgentFile> LoadAgentFile(AgentFileReference agentFileReference, bool loadContent = false)
        {
            var agentFile = new AgentFile
            {
                ObjectId = agentFileReference.ObjectId,
                Name = agentFileReference.Name,
                DisplayName = agentFileReference.OriginalFilename,
                Type = agentFileReference.Type,
                ContentType = agentFileReference.ContentType,
                AgentObjectId = ResourcePath.GetObjectId(agentFileReference.InstanceId, _name, AgentResourceTypeNames.Agents, agentFileReference.AgentName)
            };

            if (loadContent)
            {
                var fileContent = await _storageService.ReadFileAsync(
                    _storageContainerName,
                    agentFileReference.Filename,
                    default);
                agentFile.Content = fileContent.ToArray();
            }

            return agentFile;
        }

        private async Task<ResourceProviderUpsertResult> UpdateAgentFile(ResourcePath resourcePath, ResourceProviderFormFile formFile, UnifiedUserIdentity userIdentity)
        {
            if (formFile.BinaryContent.Length == 0)
                throw new ResourceProviderException("The attached file is not valid.",
                    StatusCodes.Status400BadRequest);

            string? agentName = null;
            if (formFile.Payload != null && formFile.Payload.TryGetValue(ResourceProviderFormPayloadKeys.AgentName, out var value))
                agentName = value;

            if (string.IsNullOrWhiteSpace(agentName))
                throw new ResourceProviderException("The agent name is not valid.",
                    StatusCodes.Status400BadRequest);

            var extension = GetFileExtension(formFile.FileName);
            var fullName = $"{resourcePath.ResourceId!}{extension}";
            var filePath = $"/{_name}/{_instanceSettings.Id}/{agentName}/private-file-store/{fullName}";

            var agentPrivateFile = new AgentFileReference
            {
                Id = resourcePath.ResourceId!,
                Name = resourcePath.ResourceId!,
                ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name),
                OriginalFilename = formFile.FileName,
                ContentType = formFile.ContentType!,
                Type = AgentTypes.AgentFile,
                Filename = filePath,
                Size = formFile.BinaryContent.Length,
                UPN = userIdentity.UPN ?? string.Empty,
                InstanceId = _instanceSettings.Id,
                AgentName = agentName,
                Deleted = false,
            };

            await _storageService.WriteFileAsync(
                _storageContainerName,
                agentPrivateFile.Filename,
                new MemoryStream(formFile.BinaryContent.ToArray()),
                agentPrivateFile.ContentType ?? default,
                default);

            await _cosmosDBService.CreateAgentFile(agentPrivateFile);

            return new ResourceProviderUpsertResult<AgentFile>
            {
                ObjectId = agentPrivateFile!.ObjectId,
                ResourceExists = false,
                Resource = new AgentFile
                {
                    ObjectId = agentPrivateFile.ObjectId,
                    Name = agentPrivateFile.Name,
                    DisplayName = formFile.FileName,
                    Type = agentPrivateFile.Type,
                    ContentType = agentPrivateFile.ContentType,
                    AgentObjectId = ResourcePath.GetObjectId(agentPrivateFile.InstanceId, _name, AgentResourceTypeNames.Agents, agentPrivateFile.AgentName)
                }
            };
        }

        private async Task<List<ResourceProviderGetResult<AgentFileToolAssociation>>> LoadAgentFileToolAssociations(
            ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            if (!ResourcePath.TryParse(
                $"/instances/{_instanceSettings.Id}/providers/{_name}/{AgentResourceTypeNames.Agents}/{resourcePath.MainResourceId}/{AgentResourceTypeNames.AgentFiles}",
                [_name],
                AgentResourceProviderMetadata.AllowedResourceTypes,
                false,
                out var agentFilesResourcePath))
                throw new ResourceProviderException("The object definition is invalid.",
                    StatusCodes.Status400BadRequest);

            var agentFileResources = await LoadAgentFiles(agentFilesResourcePath!, userIdentity, new ResourceProviderGetOptions() { LoadContent = false });
            var agentFiles = agentFileResources.Select(x => x.Resource).ToList();

            var filePath = $"/{_name}/{_instanceSettings.Id}/{resourcePath.MainResourceId!}/Associations.json";
            await EnsureAgentFileToolAssociationsFileExists(filePath);

            var fileContent = await _storageService.ReadFileAsync(_storageContainerName, filePath, default);
            var existingAssociations = JsonSerializer.Deserialize<List<AgentFileToolAssociation>>(Encoding.UTF8.GetString(fileContent.ToArray()))!;

            foreach (var agentFile in agentFiles)
            {
                if (!existingAssociations.Any(x => x.FileObjectId == agentFile.ObjectId))
                {
                    existingAssociations.Add(new AgentFileToolAssociation()
                    {
                        FileObjectId = agentFile.ObjectId!,
                        AssociatedResourceObjectIds = [],
                        Name = Guid.NewGuid().ToString()
                    });
                }
            }

            return existingAssociations.Select(fileToolAssociation =>
            new ResourceProviderGetResult<AgentFileToolAssociation>()
            {
                Resource = fileToolAssociation,
                Actions = [],
                Roles = []
            }).ToList();
        }

        private async Task<ResourceProviderUpsertResult> UpdateFileToolAssociations(
            ResourcePath resourcePath,
            string serializedResource,
            UnifiedUserIdentity userIdentity)
        {
            var errors = new List<AgentFileToolAssociationError>();

            var agentFileToolAssociationRequest = JsonSerializer.Deserialize<AgentFileToolAssociationRequest>(serializedResource)
                ?? throw new ResourceProviderException("The object definition is invalid.",
                    StatusCodes.Status400BadRequest);

            var filePath = $"/{_name}/{_instanceSettings.Id}/{resourcePath.MainResourceId!}/Associations.json";
            await EnsureAgentFileToolAssociationsFileExists(filePath);

            var fileContent = await _storageService.ReadFileAsync(_storageContainerName, filePath, default);
            var existingAssociations = JsonSerializer.Deserialize<List<AgentFileToolAssociation>>(Encoding.UTF8.GetString(fileContent.ToArray()))!;

            foreach (var fileObjectId in agentFileToolAssociationRequest.AgentFileToolAssociations.Keys)
            {
                if (!agentFileToolAssociationRequest.AgentFileToolAssociations.TryGetValue(fileObjectId, out var toolAssociations))
                    continue;

                foreach (var toolObjectId in toolAssociations.Keys)
                {
                    if (!toolAssociations.TryGetValue(toolObjectId, out bool enabled))
                        continue;

                    var fileToolAssociation = existingAssociations.Where(x => x.FileObjectId == fileObjectId).SingleOrDefault();
                    if (fileToolAssociation == null)
                    {
                        fileToolAssociation = new AgentFileToolAssociation()
                        {
                            Name = Guid.NewGuid().ToString(),
                            FileObjectId = fileObjectId
                        };
                        existingAssociations.Add(fileToolAssociation);
                    }
                    fileToolAssociation.AssociatedResourceObjectIds ??= [];

                    var exists = fileToolAssociation != null && fileToolAssociation.AssociatedResourceObjectIds.ContainsKey(toolObjectId);

                    if (enabled && !exists)
                    {
                        try
                        {
                            await AddFileToolAssociation(fileObjectId, toolObjectId, fileToolAssociation!, resourcePath, userIdentity);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "Error when adding the association between the {File} file and the {Tool} tool.",
                                fileObjectId, toolObjectId);

                            errors.Add(new AgentFileToolAssociationError()
                            {
                                FileObjectId = fileObjectId,
                                ToolObjectId = toolObjectId,
                                ErrorMessage = $"Error when adding the association between the {fileObjectId} file and the {toolObjectId} tool."
                            });
                        }
                    }

                    if (!enabled && exists)
                    {
                        try
                        {
                            await RemoveFileToolAssociation(fileObjectId, toolObjectId, fileToolAssociation!, resourcePath, userIdentity);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "Error when removing the association between the {File} file and the {Tool} tool.",
                                fileObjectId, toolObjectId);

                            errors.Add(new AgentFileToolAssociationError()
                            {
                                FileObjectId = fileObjectId,
                                ToolObjectId = toolObjectId,
                                ErrorMessage = $"Error when removing the association between the {fileObjectId} file and the {toolObjectId} tool."
                            });
                        }
                    }
                }
            }

            await _storageService.WriteFileAsync(
                _storageContainerName,
                filePath,
                JsonSerializer.Serialize(existingAssociations),
                default,
                default);

            return new ResourceProviderUpsertResult()
            {
                ObjectId = resourcePath.RawResourcePath,
                ResourceExists = true,
                Resource = new AgentFileToolAssociationResult()
                {
                    Success = errors.Count == 0,
                    Errors = errors,
                    AgentFileToolAssociations = existingAssociations,
                }
            };
        }

        private async Task EnsureAgentFileToolAssociationsFileExists(string filePath)
        {
            var fileExists = await _storageService.FileExistsAsync(_storageContainerName, filePath, default);

            if (!fileExists)
                await _storageService.WriteFileAsync(_storageContainerName, filePath,
                    JsonSerializer.Serialize(new List<AgentFileToolAssociation>()), default, default);
        }

        private async Task AddFileToolAssociation(
            string fileObjectId,
            string toolObjectId,
            AgentFileToolAssociation fileToolAssociation,
            ResourcePath resourcePath,
            UnifiedUserIdentity userIdentity)
        {
            // check if the agent file reference contains the requested tool association
            if (fileToolAssociation.AssociatedResourceObjectIds?.Any(x => x.Value.HasObjectRole(ResourceObjectIdPropertyValues.ToolAssociation)
                && x.Value.ObjectId == toolObjectId) ?? false)
                throw new ResourceProviderException($"The agent file {fileObjectId} is already associated with the tool {toolObjectId}.",
                    StatusCodes.Status400BadRequest);

            var toolResource = await GetResourceAsync<Tool>(toolObjectId, userIdentity);

            switch (toolResource.Name)
            {
                case ToolNames.OpenAIAssistantsFileSearchTool:
                    var gatewayClient = await GetGatewayServiceClient(userIdentity);
                    var agentObjectId = ResourcePath.GetObjectId(_instanceSettings.Id, _name, AgentResourceTypeNames.Agents, resourcePath.MainResourceId!);
                    var agent = await GetResourceAsync<AgentBase>(agentObjectId, userIdentity);

                    (var openAIAssistantId, var openAIAssistantVectorStoreId, var workflow, var agentAIModel, var agentPrompt, var agentAIModelAPIEndpoint)
                        = await ResolveAgentProperties(agent, userIdentity);

                    // check reference for the Azure OpenAI File ID
                    if (string.IsNullOrWhiteSpace(fileToolAssociation.OpenAIFileId))
                    {
                        Dictionary<string, object> parameters = new()
                        {
                            { OpenAIAgentCapabilityParameterNames.OpenAIEndpoint, agentAIModelAPIEndpoint.Url },
                            { OpenAIAgentCapabilityParameterNames.CreateOpenAIFile, true },
                            { OpenAIAgentCapabilityParameterNames.AgentFileObjectId, fileObjectId! }
                        };

                        var agentCapabilityResult = await gatewayClient!.CreateAgentCapability(
                            _instanceSettings.Id,
                            AgentCapabilityCategoryNames.OpenAIAssistants,
                            string.Empty,
                            parameters);

                        agentCapabilityResult.TryGetValue(OpenAIAgentCapabilityParameterNames.OpenAIFileId, out var openAIFileIdObject);
                        fileToolAssociation.OpenAIFileId = ((JsonElement)openAIFileIdObject!).Deserialize<string>();
                    }

                    // Add the tool association to the agent file reference
                    _ = await AddFileToAssistantsVectorStore(
                        _instanceSettings.Id,
                        agentAIModelAPIEndpoint.Url,
                        agentAIModel.DeploymentName!,
                        openAIAssistantVectorStoreId,
                        fileToolAssociation.OpenAIFileId!,
                        userIdentity);

                    // Add the tool association to the agent file reference
                    fileToolAssociation.AssociatedResourceObjectIds!.Add(toolObjectId, new ResourceObjectIdProperties
                    {
                        ObjectId = toolObjectId,
                        Properties = new Dictionary<string, object>
                        {
                            { ResourceObjectIdPropertyNames.ObjectRole, ResourceObjectIdPropertyValues.ToolAssociation }
                        }
                    });

                    break;

                case ToolNames.OpenAIAssistantsCodeInterpreterTool:
                    var gatewayClientCI = await GetGatewayServiceClient(userIdentity);
                    var agentObjectIdCI = ResourcePath.GetObjectId(_instanceSettings.Id, _name, AgentResourceTypeNames.Agents, resourcePath.MainResourceId!);
                    var agentCI = await GetResourceAsync<AgentBase>(agentObjectIdCI, userIdentity);

                    (var openAIAssistantIdCI, var openAIAssistantVectorStoreIdCI, var workflowCI, var agentAIModelCI, var agentPromptCI, var agentAIModelAPIEndpointCI)
                        = await ResolveAgentProperties(agentCI, userIdentity);

                    // check reference for the Azure OpenAI File ID
                    if (string.IsNullOrWhiteSpace(fileToolAssociation.OpenAIFileId))
                    {
                        Dictionary<string, object> parameters = new()
                        {
                            { OpenAIAgentCapabilityParameterNames.OpenAIEndpoint, agentAIModelAPIEndpointCI.Url },
                            { OpenAIAgentCapabilityParameterNames.CreateOpenAIFile, true },
                            { OpenAIAgentCapabilityParameterNames.AgentFileObjectId, fileObjectId! }
                        };

                        var agentCapabilityResult = await gatewayClientCI!.CreateAgentCapability(
                            _instanceSettings.Id,
                            AgentCapabilityCategoryNames.OpenAIAssistants,
                            string.Empty,
                            parameters);

                        agentCapabilityResult.TryGetValue(OpenAIAgentCapabilityParameterNames.OpenAIFileId, out var openAIFileIdObject);
                        fileToolAssociation.OpenAIFileId = ((JsonElement)openAIFileIdObject!).Deserialize<string>();
                    }

                    // Add the tool association to the agent file reference
                    _ = await AddFileToAssistantsCodeInterpreter(
                        _instanceSettings.Id,
                        agentAIModelAPIEndpointCI.Url,
                        agentAIModelCI.DeploymentName!,
                        openAIAssistantIdCI!,
                        fileToolAssociation.OpenAIFileId!,
                        userIdentity);

                    // Add the tool association to the agent file reference
                    fileToolAssociation.AssociatedResourceObjectIds!.Add(toolObjectId, new ResourceObjectIdProperties
                    {
                        ObjectId = toolObjectId,
                        Properties = new Dictionary<string, object>
                        {
                            { ResourceObjectIdPropertyNames.ObjectRole, ResourceObjectIdPropertyValues.ToolAssociation }
                        }
                    });

                    break;

                default:
                    throw new ResourceProviderException($"The tool {toolResource.Name} is not supported for file association by the {_name} resource provider.",
                                                StatusCodes.Status400BadRequest);
            };
        }

        private async Task RemoveFileToolAssociation(
            string fileObjectId,
            string toolObjectId,
            AgentFileToolAssociation fileToolAssociation,
            ResourcePath resourcePath,
            UnifiedUserIdentity userIdentity)
        {
            // Ensure the agent file reference contains the requested tool association
            if (!fileToolAssociation.AssociatedResourceObjectIds?.Any(x => x.Value.HasObjectRole(ResourceObjectIdPropertyValues.ToolAssociation)
                && x.Value.ObjectId == toolObjectId) ?? false)
                throw new ResourceProviderException($"The agent file {fileObjectId} is not associated with the tool {toolObjectId}.",
                    StatusCodes.Status400BadRequest);

            var toolResource = await GetResourceAsync<Tool>(toolObjectId, userIdentity);
            switch (toolResource.Name)
            {
                case ToolNames.OpenAIAssistantsFileSearchTool:
                    var gatewayClient = await GetGatewayServiceClient(userIdentity);
                    var agentObjectId = ResourcePath.GetObjectId(_instanceSettings.Id, _name, AgentResourceTypeNames.Agents, resourcePath.MainResourceId!);
                    var agent = await GetResourceAsync<AgentBase>(agentObjectId, userIdentity);

                    (var openAIAssistantId, var openAIAssistantVectorStoreId, var workflow, var agentAIModel, var agentPrompt, var agentAIModelAPIEndpoint)
                        = await ResolveAgentProperties(agent, userIdentity);

                    _ = await RemoveFileFromAssistantsVectorStore(
                            _instanceSettings.Id,
                            agentAIModelAPIEndpoint.Url,
                            agentAIModel.DeploymentName!,
                            openAIAssistantVectorStoreId,
                            fileToolAssociation.OpenAIFileId!,
                            userIdentity);

                    // Remove the tool association from the agent file reference
                    fileToolAssociation.AssociatedResourceObjectIds!.Remove(toolObjectId);

                    break;

                case ToolNames.OpenAIAssistantsCodeInterpreterTool:
                    var gatewayClientCI = await GetGatewayServiceClient(userIdentity);
                    var agentObjectIdCI = ResourcePath.GetObjectId(_instanceSettings.Id, _name, AgentResourceTypeNames.Agents, resourcePath.MainResourceId!);
                    var agentCI = await GetResourceAsync<AgentBase>(agentObjectIdCI, userIdentity);

                    (var openAIAssistantIdCI, var openAIAssistantVectorStoreIdCI, var workflowCI, var agentAIModelCI, var agentPromptCI, var agentAIModelAPIEndpointCI)
                        = await ResolveAgentProperties(agentCI, userIdentity);

                    _ = await RemoveFileFromAssistantsCodeInterpreter(
                            _instanceSettings.Id,
                            agentAIModelAPIEndpointCI.Url,
                            agentAIModelCI.DeploymentName!,
                            openAIAssistantIdCI,
                            fileToolAssociation.OpenAIFileId!,
                            userIdentity);

                    // Remove the tool association from the agent file reference
                    fileToolAssociation.AssociatedResourceObjectIds!.Remove(toolObjectId);

                    break;

                default:
                    throw new ResourceProviderException($"The tool {toolResource.Name} is not supported by the {_name} resource provider.",
                                                                        StatusCodes.Status400BadRequest);
            }
        }

        /// <summary>
        /// Retrieves the GatewayServiceClient.
        /// </summary>
        /// <param name="userIdentity">Identity of the user.</param>
        /// <returns></returns>
        private async Task<GatewayServiceClient> GetGatewayServiceClient(UnifiedUserIdentity userIdentity)
        {
            var gatewayClient = new GatewayServiceClient(
                       await _serviceProvider.GetRequiredService<IHttpClientFactoryService>()
                           .CreateClient(HttpClientNames.GatewayAPI, userIdentity),
                       _serviceProvider.GetRequiredService<ILogger<GatewayServiceClient>>());
            return gatewayClient;
        }

        /// <summary>
        /// Resolves the agent properties.
        /// </summary>
        /// <param name="agent">Agent Resource</param>
        /// <param name="userIdentity">Identity of the caller</param>
        /// <returns>openAIAssistantId, openAIAssistantVectorStoreId, AIModel resource, Prompt resource, APIEndpointConfiguration resource.</returns>
        private async Task<(string, string, AgentWorkflowBase, AIModelBase, PromptBase, APIEndpointConfiguration)> ResolveAgentProperties(AgentBase agent, UnifiedUserIdentity userIdentity)
        {
            agent.Properties ??= [];

            var workflow = (agent.Workflow as AzureOpenAIAssistantsAgentWorkflow)!;
            var openAIAssistantId = workflow.AssistantId;
            var openAiAssistantVectorStoreId = workflow.VectorStoreId;


            var agentAIModel = await GetResourceProviderServiceByName(ResourceProviderNames.FoundationaLLM_AIModel)
                            .GetResourceAsync<AIModelBase>(workflow.MainAIModelObjectId!, userIdentity);
            var agentPrompt = await GetResourceProviderServiceByName(ResourceProviderNames.FoundationaLLM_Prompt)
                            .GetResourceAsync<PromptBase>(workflow.MainPromptObjectId!, userIdentity);

            APIEndpointConfiguration agentAIModelAPIEndpoint = await GetResourceProviderServiceByName(ResourceProviderNames.FoundationaLLM_Configuration)
                    .GetResourceAsync<APIEndpointConfiguration>(agentAIModel.EndpointObjectId!, userIdentity);

            return (openAIAssistantId!, openAiAssistantVectorStoreId!, workflow!, agentAIModel, agentPrompt, agentAIModelAPIEndpoint);
        }

        /// <summary>
        /// Adds file to the assistant-level vector store.
        /// Assumes the file is already uploaded and the OpenAI File ID is available.
        /// </summary>
        /// <param name="instanceId">Identifies the FoundationaLLM instance.</param>
        /// <param name="apiEndpointUrl">The API endpoint URL of the OpenAI service.</param>
        /// <param name="deploymentName">The deployment name of the model in the OpenAI service.</param>
        /// <param name="vectorStoreId">The assistant vector store ID.</param>
        /// <param name="fileId">The OpenAI FileId indicating the file to add to the vector store.</param>
        /// <param name="userIdentity">The identity of the user.</param>
        /// <returns>Returns true if successful, false otherwise.</returns>
        private async Task<bool> AddFileToAssistantsVectorStore(string instanceId, string apiEndpointUrl, string deploymentName, string vectorStoreId, string fileId, UnifiedUserIdentity userIdentity)
        {
            var gatewayClient = await GetGatewayServiceClient(userIdentity);

            Dictionary<string, object> parameters = new()
            {
                { OpenAIAgentCapabilityParameterNames.OpenAIEndpoint, apiEndpointUrl },
                { OpenAIAgentCapabilityParameterNames.OpenAIModelDeploymentName, deploymentName },
                { OpenAIAgentCapabilityParameterNames.AddOpenAIFileToVectorStore, true },
                { OpenAIAgentCapabilityParameterNames.OpenAIVectorStoreId, vectorStoreId },
                { OpenAIAgentCapabilityParameterNames.OpenAIFileId, fileId }
            };
            var vectorizationResult = await gatewayClient!.CreateAgentCapability(
                            instanceId,
                            AgentCapabilityCategoryNames.OpenAIAssistants,
                            string.Empty,
                            parameters);

            vectorizationResult.TryGetValue(OpenAIAgentCapabilityParameterNames.OpenAIFileActionOnVectorStoreSuccess, out var vectorizationSuccessObject);
            var vectorizationSuccess = ((JsonElement)vectorizationSuccessObject!).Deserialize<bool>();
            if (!vectorizationSuccess)
                throw new OrchestrationException($"The vectorization of file id {fileId} into the vector store with id {vectorStoreId} failed.");
            return vectorizationSuccess;
        }

        /// <summary>
        /// Removes a file from the assistant-level vector store.
        /// Assumes the file is already uploaded and the OpenAI File ID is available.
        /// </summary>
        /// <param name="instanceId">Identifies the FoundationaLLM instance.</param>
        /// <param name="apiEndpointUrl">The API endpoint URL of the OpenAI service.</param>
        /// <param name="deploymentName">The deployment name of the model in the OpenAI service.</param>
        /// <param name="vectorStoreId">The assistant vector store ID.</param>
        /// <param name="fileId">The OpenAI FileId indicating the file to remove from the vector store.</param>
        /// <param name="userIdentity">The identity of the user.</param>
        /// <returns>Returns true if successful, false otherwise.</returns>
        private async Task<bool> RemoveFileFromAssistantsVectorStore(string instanceId, string apiEndpointUrl, string deploymentName, string vectorStoreId, string fileId, UnifiedUserIdentity userIdentity)
        {
            var gatewayClient = await GetGatewayServiceClient(userIdentity);

            Dictionary<string, object> parameters = new()
            {
                { OpenAIAgentCapabilityParameterNames.OpenAIEndpoint, apiEndpointUrl },
                { OpenAIAgentCapabilityParameterNames.OpenAIModelDeploymentName, deploymentName },
                { OpenAIAgentCapabilityParameterNames.RemoveOpenAIFileFromVectorStore, true },
                { OpenAIAgentCapabilityParameterNames.OpenAIVectorStoreId, vectorStoreId },
                { OpenAIAgentCapabilityParameterNames.OpenAIFileId, fileId }
            };
            var removalResult = await gatewayClient!.CreateAgentCapability(
                            instanceId,
                            AgentCapabilityCategoryNames.OpenAIAssistants,
                            string.Empty,
                            parameters);

            removalResult.TryGetValue(OpenAIAgentCapabilityParameterNames.OpenAIFileActionOnVectorStoreSuccess, out var vectorStoreSuccessObject);
            var removalSuccess = ((JsonElement)vectorStoreSuccessObject!).Deserialize<bool>();
            if (!removalSuccess)
                throw new OrchestrationException($"The removal of file id {fileId} from the vector store with id {vectorStoreId} failed.");
            return removalSuccess;
        }

        /// <summary>
        /// Adds file to the assistant code interpreter resources.
        /// Assumes the file is already uploaded and the OpenAI File ID is available.
        /// </summary>
        /// <param name="instanceId">Identifies the FoundationaLLM instance.</param>
        /// <param name="apiEndpointUrl">The API endpoint URL of the OpenAI service.</param>
        /// <param name="deploymentName">The deployment name of the model in the OpenAI service.</param>
        /// <param name="assistantId">The unique identifier of the OpenAI Assistant.</param>
        /// <param name="fileId">The OpenAI FileId indicating the file to add to the code interpreter resources.</param>
        /// <param name="userIdentity">The identity of the user.</param>
        /// <returns>Returns true if successful, false otherwise.</returns>
        private async Task<bool> AddFileToAssistantsCodeInterpreter(string instanceId, string apiEndpointUrl, string deploymentName, string assistantId, string fileId, UnifiedUserIdentity userIdentity)
        {
            var gatewayClient = await GetGatewayServiceClient(userIdentity);

            Dictionary<string, object> parameters = new()
            {
                { OpenAIAgentCapabilityParameterNames.OpenAIEndpoint, apiEndpointUrl },
                { OpenAIAgentCapabilityParameterNames.OpenAIModelDeploymentName, deploymentName },
                { OpenAIAgentCapabilityParameterNames.AddOpenAIFileToCodeInterpreter, true },
                { OpenAIAgentCapabilityParameterNames.OpenAIAssistantId, assistantId },
                { OpenAIAgentCapabilityParameterNames.OpenAIFileId, fileId }
            };
            var additionResult = await gatewayClient!.CreateAgentCapability(
                            instanceId,
                            AgentCapabilityCategoryNames.OpenAIAssistants,
                            string.Empty,
                            parameters);

            additionResult.TryGetValue(OpenAIAgentCapabilityParameterNames.OpenAIFileActionOnCodeInterpreterSuccess, out var codeInterpreterSuccessObject);
            var additionSuccess = ((JsonElement)codeInterpreterSuccessObject!).Deserialize<bool>();
            if (!additionSuccess)
                throw new OrchestrationException($"The addition of file id {fileId} into the code interpreter resources of the assistant with id {assistantId} failed.");
            return additionSuccess;
        }

        /// <summary>
        /// Removes a file from the assistant code interpreter resources.
        /// Assumes the file is already uploaded and the OpenAI File ID is available.
        /// </summary>
        /// <param name="instanceId">Identifies the FoundationaLLM instance.</param>
        /// <param name="apiEndpointUrl">The API endpoint URL of the OpenAI service.</param>
        /// <param name="deploymentName">The deployment name of the model in the OpenAI service.</param>
        /// <param name="assistantId">The unique identifier of the OpenAI Assistant.</param>
        /// <param name="fileId">The OpenAI FileId indicating the file to remove from the code interpreter.</param>
        /// <param name="userIdentity">The identity of the user.</param>
        /// <returns>Returns true if successful, false otherwise.</returns>
        private async Task<bool> RemoveFileFromAssistantsCodeInterpreter(string instanceId, string apiEndpointUrl, string deploymentName, string assistantId, string fileId, UnifiedUserIdentity userIdentity)
        {
            var gatewayClient = await GetGatewayServiceClient(userIdentity);

            Dictionary<string, object> parameters = new()
            {
                { OpenAIAgentCapabilityParameterNames.OpenAIEndpoint, apiEndpointUrl },
                { OpenAIAgentCapabilityParameterNames.OpenAIModelDeploymentName, deploymentName },
                { OpenAIAgentCapabilityParameterNames.RemoveOpenAIFileFromCodeInterpreter, true },
                { OpenAIAgentCapabilityParameterNames.OpenAIAssistantId, assistantId },
                { OpenAIAgentCapabilityParameterNames.OpenAIFileId, fileId }
            };
            var removalResult = await gatewayClient!.CreateAgentCapability(
                            instanceId,
                            AgentCapabilityCategoryNames.OpenAIAssistants,
                            string.Empty,
                            parameters);

            removalResult.TryGetValue(OpenAIAgentCapabilityParameterNames.OpenAIFileActionOnCodeInterpreterSuccess, out var codeInterpreterSuccessObject);
            var removalSuccess = ((JsonElement)codeInterpreterSuccessObject!).Deserialize<bool>();
            if (!removalSuccess)
                throw new OrchestrationException($"The removal of file id {fileId} from the code interpreter resources for assistant with id {assistantId} failed.");
            return removalSuccess;
        }

        private static string GetFileExtension(string fileName) =>
            Path.GetExtension(fileName);

        #endregion
    }
}
