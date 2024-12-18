using Azure.Messaging;
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
using FoundationaLLM.Common.Models.Events;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.Agent.AgentAccessTokens;
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
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to provide loggers for logging.</param>
    public class AgentResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IAuthorizationServiceClient authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Agent)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
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
            eventNamespacesToSubscribe: [
                EventTypes.FoundationaLLM_ResourceProvider_Agent
            ],
            useInternalReferencesStore: true)
    {
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
                AgentResourceTypeNames.AgentAccessTokens => await LoadAgentAccessTokens(
                    resourcePath)!,
                AgentResourceTypeNames.Files => await LoadAgentFiles(
                    resourcePath.MainResourceId!)!,
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        /// <inheritdoc/>
        protected override async Task<object> UpsertResourceAsync(ResourcePath resourcePath, string? serializedResource, ResourceProviderFormFile? formFile, UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeName switch
            {
                AgentResourceTypeNames.Agents => await UpdateAgent(resourcePath, serializedResource!, userIdentity),
                AgentResourceTypeNames.Workflows => await UpdateWorkflow(resourcePath, serializedResource!, userIdentity),
                AgentResourceTypeNames.AgentAccessTokens => await UpdateAgentAccessToken(resourcePath, serializedResource!),
                AgentResourceTypeNames.Files => await UpdateAgentFile(resourcePath, formFile!, userIdentity),
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
                    _ => throw new ResourceProviderException($"The action {resourcePath.Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                AgentResourceTypeNames.Workflows => resourcePath.Action switch
                {
                    ResourceProviderActions.CheckName => await CheckResourceName<Workflow>(
                        JsonSerializer.Deserialize<ResourceName>(serializedAction)!),
                    ResourceProviderActions.Purge => await PurgeResource<Workflow>(resourcePath),
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
                case AgentResourceTypeNames.Workflows:
                    await DeleteResource<Workflow>(resourcePath);
                    break;
                case AgentResourceTypeNames.Files:
                    await DeleteAgentFile(resourcePath);
                    break;
                case AgentResourceTypeNames.AgentAccessTokens:
                    await DeleteAgentAccessToken(resourcePath);
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
        protected override async Task<T> GetResourceAsyncInternal<T>(ResourcePath resourcePath, ResourcePathAuthorizationResult authorizationResult, UnifiedUserIdentity userIdentity, ResourceProviderGetOptions? options = null) =>
            (await LoadResource<T>(resourcePath.ResourceId!))!;

        #endregion

        #region Event handling

        /// <inheritdoc/>
        protected override async Task HandleEvents(EventTypeEventArgs e)
        {
            _logger.LogInformation("{EventsCount} events received in the {EventsNamespace} events namespace.",
                e.Events.Count, e.EventType);

            switch (e.EventType)
            {
                case EventTypes.FoundationaLLM_ResourceProvider_Agent:
                    foreach (var @event in e.Events)
                        await HandleAgentResourceProviderEvent(@event);
                    break;
                default:
                    // Ignore sliently any event namespace that's of no interest.
                    break;
            }

            await Task.CompletedTask;
        }

        private async Task HandleAgentResourceProviderEvent(CloudEvent e)
        {
            await Task.CompletedTask;
            return;

            // Event handling is temporarily disabled until the updated event handling mechanism is implemented.

            //if (string.IsNullOrWhiteSpace(e.Subject))
            //    return;

            //var fileName = e.Subject.Split("/").Last();

            //_logger.LogInformation("The file [{FileName}] managed by the [{ResourceProvider}] resource provider has changed and will be reloaded.",
            //    fileName, _name);

            //var agentReference = new AgentReference
            //{
            //    Name = Path.GetFileNameWithoutExtension(fileName),
            //    Filename = $"/{_name}/{fileName}",
            //    Type = AgentTypes.Basic,
            //    Deleted = false
            //};

            //var getAgentResult = await LoadAgent(agentReference, null);
            //agentReference.Name = getAgentResult.Name;
            //agentReference.Type = getAgentResult.Type;

            //_agentReferences.AddOrUpdate(
            //    agentReference.Name,
            //    agentReference,
            //    (k, v) => v);

            //_logger.LogInformation("The agent reference for the [{AgentName}] agent or type [{AgentType}] was loaded.",
            //    agentReference.Name, agentReference.Type);
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

            if ((agent is KnowledgeManagementAgent { Vectorization.DedicatedPipeline: true, InlineContext: false } kmAgent))
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
                            IndexingProfileObjectId = kmAgent.Vectorization.IndexingProfileObjectIds[0]!,
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

            if (agent.HasCapability(AgentCapabilityCategoryNames.OpenAIAssistants))
            {
                agent.Properties ??= [];

                var openAIAssistantId = agent.Properties.GetValueOrDefault(
                    AgentPropertyNames.AzureOpenAIAssistantId);

                if (string.IsNullOrWhiteSpace(openAIAssistantId))
                {
                    // The agent uses the Azure OpenAI Assistants workflow
                    // but it does not have an associated assistant.
                    // Proceed to create the Azure OpenAI Assistants assistant.

                    _logger.LogInformation(
                        "Starting to create the Azure OpenAI assistant for agent {AgentName}",
                        agent.Name);

                    #region Resolve various agent properties

                    var agentAIModel = await GetResourceProviderServiceByName(ResourceProviderNames.FoundationaLLM_AIModel)
                        .GetResourceAsync<AIModelBase>(agent.AIModelObjectId!, userIdentity);
                    var agentAIModelAPIEndpoint = await GetResourceProviderServiceByName(ResourceProviderNames.FoundationaLLM_Configuration)
                        .GetResourceAsync<APIEndpointConfiguration>(agentAIModel.EndpointObjectId!, userIdentity);
                    var agentPrompt = await GetResourceProviderServiceByName(ResourceProviderNames.FoundationaLLM_Prompt)
                        .GetResourceAsync<PromptBase>(agent.PromptObjectId!, userIdentity);

                    #endregion

                    #region Create Azure OpenAI Assistants assistant

                    var gatewayClient = new GatewayServiceClient(
                       await _serviceProvider.GetRequiredService<IHttpClientFactoryService>()
                           .CreateClient(HttpClientNames.GatewayAPI, userIdentity),
                       _serviceProvider.GetRequiredService<ILogger<GatewayServiceClient>>());

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

                    if (string.IsNullOrWhiteSpace(newOpenAIAssistantId))
                        throw new ResourceProviderException($"Could not create an Azure OpenAI assistant for the agent {agent} which requires it.",
                            StatusCodes.Status500InternalServerError);

                    _logger.LogInformation(
                        "The Azure OpenAI assistant {AssistantId} for agent {AgentName} was created successfuly.",
                        newOpenAIAssistantId, agent.Name);
                    agent.Properties[AgentPropertyNames.AzureOpenAIAssistantId] = newOpenAIAssistantId;

                    #endregion
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

        private async Task<ResourceProviderUpsertResult> UpdateWorkflow(ResourcePath resourcePath, string serializedWorkflow, UnifiedUserIdentity userIdentity)
        {
            var workflow = JsonSerializer.Deserialize<Workflow>(serializedWorkflow)
                ?? throw new ResourceProviderException("The object definition is invalid.",
                    StatusCodes.Status400BadRequest);

            var existingReference = await _resourceReferenceStore!.GetResourceReference(workflow.Name);

            if (resourcePath.ResourceTypeInstances[0].ResourceId != workflow.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            var newReference = new AgentReference
            {
                Name = workflow.Name!,
                Type = workflow.Type!,
                Filename = $"/{_name}/{workflow.Name}.json",
                Deleted = false
            };

            workflow.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);

            var validator = _resourceValidatorFactory.GetValidator(newReference.ResourceType);
            if (validator is IValidator workflowValidator)
            {
                var context = new ValidationContext<object>(workflow);
                var validationResult = await workflowValidator.ValidateAsync(context);
                if (!validationResult.IsValid)
                {
                    throw new ResourceProviderException($"Validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}",
                        StatusCodes.Status400BadRequest);
                }
            }

            UpdateBaseProperties(workflow, userIdentity, isNew: existingReference == null);
            if (existingReference == null)
                await CreateResource<Workflow>(newReference, workflow);
            else
                await SaveResource<Workflow>(existingReference, workflow);

            return new ResourceProviderUpsertResult
            {
                ObjectId = workflow!.ObjectId,
                ResourceExists = existingReference != null
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

        private async Task<List<ResourceProviderGetResult<AgentFile>>> LoadAgentFiles(string agentName) =>
            (await _resourceReferenceStore!.GetAllResourceReferences<AgentFile>())
            .Where(r => r.Name.StartsWith(agentName))
            .Select(r => (r, r.Name.Split("|").Last()))
            .Select(x => new ResourceProviderGetResult<AgentFile>()
            {
                Actions = [],
                Roles = [],
                Resource = new AgentFile()
                {
                    Name = x.Item2,
                    DisplayName = x.Item2,
                    ObjectId = ResourcePath.GetObjectId(_instanceSettings.Id, _name, AgentResourceTypeNames.Agents, agentName, AgentResourceTypeNames.Files, x.Item2)
                }
            }).ToList();

        private async Task<ResourceProviderUpsertResult> UpdateAgentFile(ResourcePath resourcePath, ResourceProviderFormFile formFile, UnifiedUserIdentity userIdentity)
        {
            if (formFile.BinaryContent.Length == 0)
                throw new ResourceProviderException("The attached file is not valid.",
                    StatusCodes.Status400BadRequest);

            if (resourcePath.ResourceId != formFile.FileName)
                throw new ResourceProviderException("The resource path does not match the file name (name mismatch).",
                    StatusCodes.Status400BadRequest);

            var filePath = $"{_name}/{_instanceSettings.Id}/{resourcePath.MainResourceId!}/private-file-store/{resourcePath.ResourceId!}";
            var resourceName = $"{resourcePath.MainResourceId!}|{resourcePath.ResourceId}";

            var existingAgentReference = await _resourceReferenceStore!.GetResourceReference(resourceName);

            if (existingAgentReference == null)
            {
                var agentFileReference = new AgentReference
                {
                    Name = resourceName,
                    Type = AgentTypes.AgentFile,
                    Filename = $"/{filePath}",
                    Deleted = false
                };

                await _resourceReferenceStore.AddResourceReference(agentFileReference);
            }

            await _storageService.WriteFileAsync(_storageContainerName, filePath, new MemoryStream(formFile.BinaryContent.ToArray()), formFile.ContentType, CancellationToken.None);

            return new ResourceProviderUpsertResult
            {
                ResourceExists = existingAgentReference != null,
                ObjectId = ResourcePath.GetObjectId(_instanceSettings.Id, _name, AgentResourceTypeNames.Agents, resourcePath.MainResourceId!, AgentResourceTypeNames.Files, resourcePath.ResourceId!)
            };
        }

        private async Task DeleteAgentFile(ResourcePath resourcePath)
        {
            var resourceName = $"{resourcePath.MainResourceId!}|{resourcePath.ResourceId}";

            var result = await _resourceReferenceStore!.TryGetResourceReference(resourceName);

            if (result.Success && !result.Deleted)
            {
                await _resourceReferenceStore!.DeleteResourceReference(result.ResourceReference!);

                //var filePath = $"{_name}/{_instanceSettings.Id}/{resourcePath.MainResourceId!}/private-file-store/{resourcePath.ResourceId!}";

                //await _storageService.DeleteFileAsync(_storageContainerName, filePath);
            }
            else
            {
                throw new ResourceProviderException($"The resource {resourceName} cannot be deleted because it was either already deleted or does not exist.",
                    StatusCodes.Status404NotFound);
            }
        }

        #endregion
    }
}
