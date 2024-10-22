using Azure.Messaging;
using FluentValidation;
using FoundationaLLM.Agent.Models.Resources;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Events;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
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
    /// <param name="authorizationService">The <see cref="IAuthorizationService"/> providing authorization services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to provide loggers for logging.</param>
    public class AgentResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IAuthorizationService authorizationService,
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
                EventSetEventNamespaces.FoundationaLLM_ResourceProvider_Agent
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
            ResourceProviderLoadOptions? options = null) =>
            resourcePath.ResourceTypeName switch
            {
                AgentResourceTypeNames.Agents => await LoadResources<AgentBase>(
                    resourcePath.ResourceTypeInstances[0],
                    authorizationResult,
                    options ?? new ResourceProviderLoadOptions
                    {
                        IncludeRoles = resourcePath.IsResourceTypePath,
                    }),
                AgentResourceTypeNames.Files => ((await LoadAgentFiles(
                    resourcePath.MainResourceId!)))!,
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        /// <inheritdoc/>
        protected override async Task<object> UpsertResourceAsync(ResourcePath resourcePath, string serializedResource, UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeName switch
            {
                AgentResourceTypeNames.Agents => await UpdateAgent(resourcePath, serializedResource, userIdentity),
                AgentResourceTypeNames.Files => await UpdateAgentFile(resourcePath.MainResourceId!, resourcePath.ResourceId!, serializedResource),
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
                case AgentResourceTypeNames.Files:
                    await DeleteAgentFile(resourcePath.MainResourceId!, resourcePath.ResourceId!);
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
        protected override async Task<T> GetResourceAsyncInternal<T>(ResourcePath resourcePath, ResourcePathAuthorizationResult authorizationResult, UnifiedUserIdentity userIdentity, ResourceProviderLoadOptions? options = null) =>
            (await LoadResource<T>(resourcePath.ResourceId!))!;

        #endregion

        #region Event handling

        /// <inheritdoc/>
        protected override async Task HandleEvents(EventSetEventArgs e)
        {
            _logger.LogInformation("{EventsCount} events received in the {EventsNamespace} events namespace.",
                e.Events.Count, e.Namespace);

            switch (e.Namespace)
            {
                case EventSetEventNamespaces.FoundationaLLM_ResourceProvider_Agent:
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
                        userIdentity);

                if ((result is ResourceProviderUpsertResult resourceProviderResult)
                    && !string.IsNullOrWhiteSpace(resourceProviderResult.ObjectId))
                    kmAgent.Vectorization.VectorizationDataPipelineObjectId = resourceProviderResult.ObjectId;
                else
                    throw new ResourceProviderException("There was an error attempting to create the associated vectorization pipeline for the agent.",
                        StatusCodes.Status500InternalServerError);
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

        private async Task<List<ResourceBase>> LoadAgentFiles(string agentName)
        {
            var files = await _storageService.GetFilePathsAsync(_storageContainerName, $"{_name}/{agentName}/");

            var fileNames = files.Select(filePath => filePath.Split("/").Last()).ToList();

            return fileNames.Select(fileName => new ResourceBase()
            {
                Name = fileName,
                DisplayName = fileName,
                ObjectId = ResourcePath.GetObjectId(_instanceSettings.Id, _name, AgentResourceTypeNames.Agents, agentName, AgentResourceTypeNames.Files, fileName)
            }).ToList();
        }

        private async Task<ResourceProviderUpsertResult> UpdateAgentFile(string agentName, string fileName, string fileContent)
        {
            var resourceExists = await _storageService.FileExistsAsync(_storageContainerName, $"{_name}/{agentName}/{fileName}", CancellationToken.None);

            await _storageService.WriteFileAsync(_storageContainerName, $"{_name}/{agentName}/{fileName}", fileContent, null, CancellationToken.None);

            return new ResourceProviderUpsertResult
            {
                ResourceExists = resourceExists,
                ObjectId = ResourcePath.GetObjectId(_instanceSettings.Id, _name, AgentResourceTypeNames.Agents, agentName, AgentResourceTypeNames.Files, fileName)
            };
        }

        private async Task DeleteAgentFile(string agentName, string fileName) =>
            await _storageService.DeleteFileAsync(_storageContainerName, $"{_name}/{agentName}/{fileName}");

        #endregion
    }
}
