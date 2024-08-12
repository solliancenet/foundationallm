using Azure.Messaging;
using FluentValidation;
using FoundationaLLM.Agent.Models.Resources;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
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
using System.Collections.Concurrent;
using System.Data;
using System.Text;
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
        : ResourceProviderServiceBase(
            instanceOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            loggerFactory.CreateLogger<AgentResourceProviderService>(),
            [
                EventSetEventNamespaces.FoundationaLLM_ResourceProvider_Agent
            ])
    {
        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            AgentResourceProviderMetadata.AllowedResourceTypes;

        private ConcurrentDictionary<string, AgentReference> _agentReferences = [];

        private const string AGENT_REFERENCES_FILE_NAME = "_agent-references.json";
        private const string AGENT_REFERENCES_FILE_PATH = $"/{ResourceProviderNames.FoundationaLLM_Agent}/{AGENT_REFERENCES_FILE_NAME}";

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Agent;

        /// <inheritdoc/>
        protected override async Task InitializeInternal()
        {
            _logger.LogInformation("Starting to initialize the {ResourceProvider} resource provider...", _name);

            if (await _storageService.FileExistsAsync(_storageContainerName, AGENT_REFERENCES_FILE_PATH, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, AGENT_REFERENCES_FILE_PATH, default);
                var agentReferenceStore = JsonSerializer.Deserialize<AgentReferenceStore>(
                    Encoding.UTF8.GetString(fileContent.ToArray()));

                _agentReferences = new ConcurrentDictionary<string, AgentReference>(
                    agentReferenceStore!.ToDictionary());
            }
            else
            {
                await _storageService.WriteFileAsync(
                    _storageContainerName,
                    AGENT_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(new AgentReferenceStore { AgentReferences = [] }),
                    default,
                    default);
            }

            _logger.LogInformation("The {ResourceProvider} resource provider was successfully initialized.", _name);
        }

        #region Resource provider support for Management API

        /// <inheritdoc/>
        protected override async Task<object> GetResourcesAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeInstances[0].ResourceType switch
            {
                AgentResourceTypeNames.Agents => await LoadAgents(resourcePath.ResourceTypeInstances[0], userIdentity),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for GetResourcesAsyncInternal

        private async Task<List<ResourceProviderGetResult<AgentBase>>> LoadAgents(ResourceTypeInstance instance, UnifiedUserIdentity userIdentity)
        {
            var agents = new List<AgentBase>();

            if (instance.ResourceId == null)
            {
                agents = (await Task.WhenAll(_agentReferences.Values
                                    .Where(ar => !ar.Deleted)
                                    .Select(ar => LoadAgent(ar))))
                                        .Where(agent => agent != null)
                                        .Select(agent => agent!)
                                        .ToList();
            }
            else
            {
                AgentBase? agent;
                if (!_agentReferences.TryGetValue(instance.ResourceId, out var agentReference))
                {
                    agent = await LoadAgent(null, instance.ResourceId);
                    if (agent != null)
                        agents.Add(agent);
                }
                else
                {
                    if (agentReference.Deleted)
                        throw new ResourceProviderException($"Could not locate the {instance.ResourceId} agent resource.",
                            StatusCodes.Status404NotFound);

                    agent = await LoadAgent(agentReference);
                    if (agent != null)
                        agents.Add(agent);
                }
            }

            return await _authorizationService.FilterResourcesByAuthorizableAction(
                _instanceSettings.Id, userIdentity, agents,
                AuthorizableActionNames.FoundationaLLM_Agent_Agents_Read);
        }

        private async Task<AgentBase?> LoadAgent(AgentReference? agentReference, string? resourceId = null)
        {
            // agentReference is null for legacy agents
            if (agentReference != null || !string.IsNullOrWhiteSpace(resourceId))
            {
                agentReference ??= new AgentReference
                {
                    Name = resourceId!,
                    Type = AgentTypes.Basic,
                    Filename = $"/{_name}/{resourceId}.json",
                    Deleted = false
                };
                if (await _storageService.FileExistsAsync(_storageContainerName, agentReference.Filename, default))
                {
                    var fileContent = await _storageService.ReadFileAsync(_storageContainerName, agentReference.Filename, default);

                    var agent = JsonSerializer.Deserialize(
                        Encoding.UTF8.GetString(fileContent.ToArray()),
                        agentReference.AgentType,
                        _serializerSettings) as AgentBase
                        ?? throw new ResourceProviderException($"Failed to load the agent {agentReference.Name}.",
                            StatusCodes.Status400BadRequest);

                    if (!string.IsNullOrWhiteSpace(resourceId))
                    {
                        // The agent file exists, but the agent reference is not in the dictionary. Update the dictionary with the missing reference.
                        agentReference.Type = agent.Type!;
                        _agentReferences.AddOrUpdate(agentReference.Name, agentReference, (k, v) => agentReference);
                    }

                    return agent;
                }

                if (string.IsNullOrWhiteSpace(resourceId))
                {
                    // Remove the reference from the dictionary since the file does not exist.
                    _agentReferences.TryRemove(agentReference.Name, out _);
                    return null;
                }
            }

            throw new ResourceProviderException($"The {_name} resource provider could not locate a resource because of invalid resource identification parameters.",
                StatusCodes.Status400BadRequest);
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task<object> UpsertResourceAsync(ResourcePath resourcePath, string serializedResource, UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeInstances[0].ResourceType switch
            {
                AgentResourceTypeNames.Agents => await UpdateAgent(resourcePath, serializedResource, userIdentity),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for UpsertResourceAsync

        private async Task<ResourceProviderUpsertResult> UpdateAgent(ResourcePath resourcePath, string serializedAgent, UnifiedUserIdentity userIdentity)
        {
            var agent = JsonSerializer.Deserialize<AgentBase>(serializedAgent)
                ?? throw new ResourceProviderException("The object definition is invalid.",
                    StatusCodes.Status400BadRequest);

            if (_agentReferences.TryGetValue(agent.Name!, out var existingAgentReference)
                && existingAgentReference!.Deleted)
                throw new ResourceProviderException($"The agent resource {existingAgentReference.Name} cannot be added or updated.",
                        StatusCodes.Status400BadRequest);

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
            agent.Version = Version.Parse(_instanceSettings.Version);

            if ((agent is KnowledgeManagementAgent {Vectorization.DedicatedPipeline: true, InlineContext: false} kmAgent))
            {
                var result = await GetResourceProviderServiceByName(ResourceProviderNames.FoundationaLLM_Vectorization)
                    .HandlePostAsync(
                        $"/{VectorizationResourceTypeNames.VectorizationPipelines}/{kmAgent.Name}",
                        JsonSerializer.Serialize<VectorizationPipeline>(new VectorizationPipeline
                        {
                            Name = kmAgent.Name,
                            Active = true,
                            Description = $"Vectorization data pipeline dedicated to the {kmAgent.Name} agent.",
                            DataSourceObjectId = kmAgent.Vectorization.DataSourceObjectId!,
                            TextPartitioningProfileObjectId = kmAgent.Vectorization.TextPartitioningProfileObjectId!,
                            TextEmbeddingProfileObjectId = kmAgent.Vectorization.TextEmbeddingProfileObjectId!,
                            IndexingProfileObjectId = kmAgent.Vectorization.IndexingProfileObjectIds[0]!,
                            TriggerType = (VectorizationPipelineTriggerType) kmAgent.Vectorization.TriggerType!,
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

            var validator = _resourceValidatorFactory.GetValidator(agentReference.AgentType);
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

            if (existingAgentReference == null)
                agent.CreatedBy = userIdentity.UPN;
            else
                agent.UpdatedBy = userIdentity.UPN;

            await _storageService.WriteFileAsync(
                _storageContainerName,
                agentReference.Filename,
                JsonSerializer.Serialize<AgentBase>(agent, _serializerSettings),
                default,
                default);

            _agentReferences.AddOrUpdate(agentReference.Name, agentReference, (k, v) => agentReference);

            await _storageService.WriteFileAsync(
                    _storageContainerName,
                    AGENT_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(AgentReferenceStore.FromDictionary(_agentReferences.ToDictionary())),
                    default,
                    default);

            return new ResourceProviderUpsertResult
            {
                ObjectId = agent!.ObjectId,
                ResourceExists = existingAgentReference != null
            };
        }

        #endregion

        /// <inheritdoc/>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task<object> ExecuteActionAsync(ResourcePath resourcePath, string serializedAction, UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeInstances.Last().ResourceType switch
            {
                AgentResourceTypeNames.Agents => resourcePath.ResourceTypeInstances.Last().Action switch
                {
                    ResourceProviderActions.CheckName => CheckAgentName(serializedAction),
                    ResourceProviderActions.Purge => await PurgeResource(resourcePath),
                    _ => throw new ResourceProviderException($"The action {resourcePath.ResourceTypeInstances.Last().Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                _ => throw new ResourceProviderException()
            };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        #region Helpers for ExecuteActionAsync

        private ResourceNameCheckResult CheckAgentName(string serializedAction)
        {
            var resourceName = JsonSerializer.Deserialize<ResourceName>(serializedAction);
            return _agentReferences.Values.Any(ar => ar.Name.Equals(resourceName!.Name, StringComparison.OrdinalIgnoreCase))
                ? new ResourceNameCheckResult
                {
                    Name = resourceName!.Name,
                    Type = resourceName.Type,
                    Status = NameCheckResultType.Denied,
                    Message = "A resource with the specified name already exists or was previously deleted and not purged."
                }
                : new ResourceNameCheckResult
                {
                    Name = resourceName!.Name,
                    Type = resourceName.Type,
                    Status = NameCheckResultType.Allowed
                };
        }

        private async Task<ResourceProviderActionResult> PurgeResource(ResourcePath resourcePath)
        {
            var resourceName = resourcePath.ResourceTypeInstances.Last().ResourceId!;
            if (_agentReferences.TryGetValue(resourceName, out var agentReference))
            {
                if (agentReference.Deleted)
                {
                    // Delete the resource file from storage.
                    await _storageService.DeleteFileAsync(
                        _storageContainerName,
                        agentReference.Filename,
                        default);

                    // Remove this resource reference from the store.
                    _agentReferences.TryRemove(resourceName, out _);

                    await _storageService.WriteFileAsync(
                        _storageContainerName,
                        AGENT_REFERENCES_FILE_PATH,
                        JsonSerializer.Serialize(AgentReferenceStore.FromDictionary(_agentReferences.ToDictionary())),
                        default,
                        default);

                    return new ResourceProviderActionResult(true);
                }
                else
                {
                    throw new ResourceProviderException($"The {resourceName} agent resource is not soft-deleted and cannot be purged.",
                                               StatusCodes.Status400BadRequest);
                }
            }
            else
            {
                throw new ResourceProviderException($"Could not locate the {resourceName} agent resource.",
                    StatusCodes.Status404NotFound);
            }
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task DeleteResourceAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            switch (resourcePath.ResourceTypeInstances.Last().ResourceType)
            {
                case AgentResourceTypeNames.Agents:
                    await DeleteAgent(resourcePath.ResourceTypeInstances);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances.Last().ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            };
        }

        #region Helpers for DeleteResourceAsync

        private async Task DeleteAgent(List<ResourceTypeInstance> instances)
        {
            if (_agentReferences.TryGetValue(instances.Last().ResourceId!, out var agentReference))
            {
                if (!agentReference.Deleted)
                {
                    agentReference.Deleted = true;

                    await _storageService.WriteFileAsync(
                        _storageContainerName,
                        AGENT_REFERENCES_FILE_PATH,
                        JsonSerializer.Serialize(AgentReferenceStore.FromDictionary(_agentReferences.ToDictionary())),
                        default,
                        default);
                }
            }
            else
            {
                throw new ResourceProviderException($"Could not locate the {instances.Last().ResourceId} agent resource.",
                    StatusCodes.Status404NotFound);
            }
        }

        #endregion

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
            if (string.IsNullOrWhiteSpace(e.Subject))
                return;

            var fileName = e.Subject.Split("/").Last();

            _logger.LogInformation("The file [{FileName}] managed by the [{ResourceProvider}] resource provider has changed and will be reloaded.",
                fileName, _name);

            var agentReference = new AgentReference
            {
                Name = Path.GetFileNameWithoutExtension(fileName),
                Filename = $"/{_name}/{fileName}",
                Type = AgentTypes.Basic,
                Deleted = false
            };

            var getAgentResult = await LoadAgent(agentReference, null);
            agentReference.Name = getAgentResult.Name;
            agentReference.Type = getAgentResult.Type;

            _agentReferences.AddOrUpdate(
                agentReference.Name,
                agentReference,
                (k, v) => v);

            _logger.LogInformation("The agent reference for the [{AgentName}] agent or type [{AgentType}] was loaded.",
                agentReference.Name, agentReference.Type);
        }

        #endregion
    }
}
