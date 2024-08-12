using Azure.Messaging;
using FluentValidation;
using FoundationaLLM.AIModel.Models;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Events;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
using FoundationaLLM.Common.Services.ResourceProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.AIModel.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.AIModel resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationService"/> providing authorization services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to provide loggers for logging.</param>
    public class AIModelResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IAuthorizationService authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_AIModel)] IStorageService storageService,
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
            loggerFactory.CreateLogger<AIModelResourceProviderService>(),
            [
                EventSetEventNamespaces.FoundationaLLM_ResourceProvider_AIModel
            ])
    {
        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            AIModelResourceProviderMetadata.AllowedResourceTypes;

        private ConcurrentDictionary<string, AIModelReference> _aiModelReferences;

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_AIModel;
        private const string AIMODEL_REFERENCES_FILE_NAME = "_ai-model-references.json";
        private const string AIMODEL_REFERENCES_FILE_PATH =
            $"/{ResourceProviderNames.FoundationaLLM_AIModel}/{AIMODEL_REFERENCES_FILE_NAME}";

        /// <inheritdoc/>
        protected override async Task InitializeInternal()
        {
            _logger.LogInformation("Starting to initialize the {ResourceProvider} resource provider...", _name);

            if (await _storageService.FileExistsAsync(_storageContainerName, AIMODEL_REFERENCES_FILE_PATH, default))
            {
                var fileContent = await _storageService.ReadFileAsync(
                    _storageContainerName,
                    AIMODEL_REFERENCES_FILE_PATH,
                    default);

                var resourceReferenceStore =
                    JsonSerializer.Deserialize<ResourceReferenceStore<AIModelReference>>(
                        Encoding.UTF8.GetString(fileContent.ToArray()));

                _aiModelReferences = new ConcurrentDictionary<string, AIModelReference>(
                        resourceReferenceStore!.ToDictionary());
            }
            else
            {
                await _storageService.WriteFileAsync(
                    _storageContainerName,
                    AIMODEL_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(new ResourceReferenceStore<AIModelReference>
                    {
                        ResourceReferences = []
                    }),
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
                AIModelResourceTypeNames.AIModels => await LoadAIModels(resourcePath.ResourceTypeInstances[0], userIdentity),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for GetResourcesAsyncInternal

        private async Task<List<ResourceProviderGetResult<AIModelBase>>> LoadAIModels(ResourceTypeInstance instance, UnifiedUserIdentity userIdentity)
        {
            var aiModels = new List<AIModelBase>();

            if (instance.ResourceId == null)
            {
                aiModels = (await Task.WhenAll(_aiModelReferences.Values
                                         .Where(ar => !ar.Deleted)
                                         .Select(ar => LoadAIModel(ar))))
                                             .Where(a => a != null)
                                             .Select(a => a!)
                                             .ToList();

            }
            else
            {
                AIModelBase? aiModel;
                if (!_aiModelReferences.TryGetValue(instance.ResourceId, out var aiModelReference))
                {
                    aiModel = await LoadAIModel(null, instance.ResourceId);
                    if (aiModel != null)
                        aiModels.Add(aiModel);
                }
                else
                {
                    if (aiModelReference.Deleted)
                        throw new ResourceProviderException(
                            $"Could not locate the {instance.ResourceId} aiModel resource.",
                            StatusCodes.Status404NotFound);

                    aiModel = await LoadAIModel(aiModelReference);
                    if (aiModel != null)
                        aiModels.Add(aiModel);
                }
            }
            return aiModels.Select(aiModel => new ResourceProviderGetResult<AIModelBase>() { Resource = aiModel, Actions = [], Roles = [] }).ToList();
        }

        /// <inheritdoc/>
        private async Task<AIModelBase?> LoadAIModel(AIModelReference? aiModelReference, string? resourceId = null)
        {
            if (aiModelReference != null || !string.IsNullOrWhiteSpace(resourceId))
            {
                aiModelReference ??= new AIModelReference
                {
                    Name = resourceId!,
                    Type = AIModelTypes.Basic,
                    Filename = $"/{_name}/{resourceId}.json",
                    Deleted = false
                };


                if (await _storageService.FileExistsAsync(_storageContainerName, aiModelReference.Filename, default))
                {
                    var fileContent = await _storageService.ReadFileAsync(_storageContainerName, aiModelReference.Filename, default);
                    var aiModel = JsonSerializer.Deserialize(
                               Encoding.UTF8.GetString(fileContent.ToArray()),
                               aiModelReference.AIModelType,
                               base._serializerSettings) as AIModelBase
                           ?? throw new ResourceProviderException($"Failed to load the AI Model {aiModelReference.Name}.",
                               StatusCodes.Status400BadRequest);

                    if (!string.IsNullOrWhiteSpace(resourceId))
                    {
                        aiModelReference.Type = aiModel.Type!;
                        _aiModelReferences.AddOrUpdate(aiModelReference.Name, aiModelReference, (k, v) => aiModelReference);
                    }

                    return aiModel;
                }

                if (string.IsNullOrWhiteSpace(resourceId))
                {
                    // Remove the reference from the dictionary since the file does not exist.
                    _aiModelReferences.TryRemove(aiModelReference.Name, out _);
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
                AIModelResourceTypeNames.AIModels => await UpdateAIModel(resourcePath, serializedResource, userIdentity),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for UpsertResourceAsync

        private async Task<ResourceProviderUpsertResult> UpdateAIModel(ResourcePath resourcePath, string serializedAIModel, UnifiedUserIdentity userIdentity)
        {

            var aiModel = JsonSerializer.Deserialize<AIModelBase>(serializedAIModel)
                ?? throw new ResourceProviderException("The object definition is invalid.",
                    StatusCodes.Status400BadRequest);

            if (_aiModelReferences.TryGetValue(aiModel.Name!, out var existingAIModelReference)
                && existingAIModelReference!.Deleted)
                throw new ResourceProviderException($"The AI model resource {existingAIModelReference.Name} cannot be added or updated.",
                        StatusCodes.Status400BadRequest);

            if (resourcePath.ResourceTypeInstances[0].ResourceId != aiModel.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            var aiModelReference = new AIModelReference
            {
                Name = aiModel.Name!,
                Type = aiModel.Type!,
                Filename = $"/{_name}/{aiModel.Name}.json",
                Deleted = false
            };

            aiModel.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);
            aiModel.Version = _instanceSettings.Version;

            var validator = _resourceValidatorFactory.GetValidator(aiModelReference.AIModelType);
            if (validator is IValidator aiModelValidator)
            {
                var context = new ValidationContext<object>(aiModel);
                var validationResult = await aiModelValidator.ValidateAsync(context);
                if (!validationResult.IsValid)
                {
                    throw new ResourceProviderException($"Validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}",
                        StatusCodes.Status400BadRequest);
                }
            }

            if (existingAIModelReference == null)
                aiModel.CreatedBy = userIdentity.UPN;
            else
                aiModel.UpdatedBy = userIdentity.UPN;

            await _storageService.WriteFileAsync(
                _storageContainerName,
                aiModelReference.Filename,
                JsonSerializer.Serialize<AIModelBase>(aiModel, _serializerSettings),
                default,
                default);

            _aiModelReferences.AddOrUpdate(aiModelReference.Name, aiModelReference, (k, v) => aiModelReference);

            await _storageService.WriteFileAsync(
                    _storageContainerName,
                    AIMODEL_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(ResourceReferenceStore<AIModelReference>.FromDictionary(_aiModelReferences.ToDictionary())),
                    default,
                    default);

            return new ResourceProviderUpsertResult
            {
                ObjectId = (aiModel as AIModelBase)!.ObjectId
            };
        }

        private string GetFileExtension(string fileName) =>
            Path.GetExtension(fileName);

        #endregion

        /// <inheritdoc/>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task<object> ExecuteActionAsync(ResourcePath resourcePath, string serializedAction, UnifiedUserIdentity userIdentity) => throw new NotImplementedException();
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        /// <inheritdoc/>
        protected override async Task DeleteResourceAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            switch (resourcePath.ResourceTypeInstances.Last().ResourceType)
            {
                case AIModelResourceTypeNames.AIModels:
                    await DeleteAIModel(resourcePath.ResourceTypeInstances);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances.Last().ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            };
        }

        #region Helpers for DeleteResourceAsync

        private async Task DeleteAIModel(List<ResourceTypeInstance> instances)
        {
            if (_aiModelReferences.TryGetValue(instances.Last().ResourceId!, out var aiModelReference))
            {
                if (!aiModelReference.Deleted)
                {
                    aiModelReference.Deleted = true;

                    await _storageService.WriteFileAsync(
                        _storageContainerName,
                        AIMODEL_REFERENCES_FILE_PATH,
                        JsonSerializer.Serialize(ResourceReferenceStore<AIModelReference>.FromDictionary(_aiModelReferences.ToDictionary())),
                        default,
                        default);
                }
            }
            else
            {
                throw new ResourceProviderException($"Could not locate the {instances.Last().ResourceId} aiModel resource.",
                    StatusCodes.Status404NotFound);
            }
        }

        #endregion

        #endregion

        /// <inheritdoc/>
        protected override async Task<T> GetResourceInternal<T>(ResourcePath resourcePath, UnifiedUserIdentity userIdentity, ResourceProviderOptions? options = null) where T : class
        {
            _aiModelReferences.TryGetValue(resourcePath.ResourceTypeInstances[0].ResourceId!, out var aiModelReference);
            if (aiModelReference == null || aiModelReference.Deleted)
                throw new ResourceProviderException($"The resource {resourcePath.ResourceTypeInstances[0].ResourceId!} of type {resourcePath.ResourceTypeInstances[0].ResourceType} was not found.");

            var aiModel = await LoadAIModel(aiModelReference);
            return aiModel as T
                ?? throw new ResourceProviderException($"The resource {resourcePath.ResourceTypeInstances[0].ResourceId!} of type {resourcePath.ResourceTypeInstances[0].ResourceType} was not found.");
        }


        #region Event handling

        /// <inheritdoc/>
        protected override async Task HandleEvents(EventSetEventArgs e)
        {
            _logger.LogInformation("{EventsCount} events received in the {EventsNamespace} events namespace.",
                e.Events.Count, e.Namespace);

            switch (e.Namespace)
            {
                case EventSetEventNamespaces.FoundationaLLM_ResourceProvider_AIModel:
                    foreach (var @event in e.Events)
                        await HandleAIModelResourceProviderEvent(@event);
                    break;
                default:
                    // Ignore sliently any event namespace that's of no interest.
                    break;
            }

            await Task.CompletedTask;
        }

        private async Task HandleAIModelResourceProviderEvent(CloudEvent e)
        {
            if (string.IsNullOrWhiteSpace(e.Subject))
                return;

            var fileName = e.Subject.Split("/").Last();

            _logger.LogInformation("The file [{FileName}] managed by the [{ResourceProvider}] resource provider has changed and will be reloaded.",
                fileName, _name);

            var aiModelReference = new AIModelReference
            {
                Name = Path.GetFileNameWithoutExtension(fileName),
                Filename = $"/{_name}/{fileName}",
                Type = nameof(AIModelBase),
                Deleted = false
            };

            var aiModel = await LoadAIModel(aiModelReference);
            aiModelReference.Name = aiModel.Name;
            aiModelReference.Type = aiModel.Type!;

            _aiModelReferences.AddOrUpdate(
                aiModelReference.Name,
                aiModelReference,
                (k, v) => v);

            _logger.LogInformation("The aiModel reference for the [{AIModelName}] agent or type [{AIModelType}] was loaded.",
                aiModelReference.Name, aiModelReference.Type);
        }

        #endregion

    }
}
