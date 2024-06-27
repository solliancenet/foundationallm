using Azure.Messaging;
using FluentValidation;
using FoundationaLLM.Common.Constants;
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
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.DataSource.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.DataSource.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.DataSource resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationService"/> providing authorization services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to provide loggers for logging.</param>
    public class DataSourceResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IAuthorizationService authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProvider_DataSource)] IStorageService storageService,
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
            loggerFactory.CreateLogger<DataSourceResourceProviderService>(),
            [
                EventSetEventNamespaces.FoundationaLLM_ResourceProvider_DataSource
            ])
    {
        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            DataSourceResourceProviderMetadata.AllowedResourceTypes;

        private ConcurrentDictionary<string, DataSourceReference> _dataSourceReferences = [];
        private string _defaultDataSourceName = string.Empty;

        private const string DATA_SOURCE_REFERENCES_FILE_NAME = "_data-source-references.json";
        private const string DATA_SOURCE_REFERENCES_FILE_PATH = $"/{ResourceProviderNames.FoundationaLLM_DataSource}/{DATA_SOURCE_REFERENCES_FILE_NAME}";

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_DataSource;

        /// <inheritdoc/>
        protected override async Task InitializeInternal()
        {
            _logger.LogInformation("Starting to initialize the {ResourceProvider} resource provider...", _name);

            if (await _storageService.FileExistsAsync(_storageContainerName, DATA_SOURCE_REFERENCES_FILE_PATH, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, DATA_SOURCE_REFERENCES_FILE_PATH, default);
                var dataSourceReferenceStore = JsonSerializer.Deserialize<DataSourceReferenceStore>(
                    Encoding.UTF8.GetString(fileContent.ToArray()));

                _dataSourceReferences = new ConcurrentDictionary<string, DataSourceReference>(
                    dataSourceReferenceStore!.ToDictionary());
                _defaultDataSourceName = dataSourceReferenceStore.DefaultDataSourceName ?? string.Empty;
            }
            else
            {
                await _storageService.WriteFileAsync(
                    _storageContainerName,
                    DATA_SOURCE_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(new DataSourceReferenceStore { DataSourceReferences = [] }),
                    default,
                    default);
            }

            _logger.LogInformation("The {ResourceProvider} resource provider was successfully initialized.", _name);
        }

        #region Support for Management API

        /// <inheritdoc/>
        protected override async Task<object> GetResourcesAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeInstances[0].ResourceType switch
            {
                DataSourceResourceTypeNames.DataSources => await LoadDataSources(resourcePath.ResourceTypeInstances[0], userIdentity),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for GetResourcesAsyncInternal

        private async Task<List<ResourceProviderGetResult<DataSourceBase>>> LoadDataSources(ResourceTypeInstance instance, UnifiedUserIdentity userIdentity)
        {
            var dataSources = new List<DataSourceBase>();

            if (instance.ResourceId == null)
            {
                dataSources = (await Task.WhenAll(_dataSourceReferences.Values
                                         .Where(dsr => !dsr.Deleted)
                                         .Select(dsr => LoadDataSource(dsr))))
                                             .Where(ds => ds != null)
                                             .Select(ds => ds!)
                                             .ToList();
            }
            else
            {
                DataSourceBase? dataSource;
                if (!_dataSourceReferences.TryGetValue(instance.ResourceId, out var dataSourceReference))
                {
                    dataSource = await LoadDataSource(null, instance.ResourceId);
                    if (dataSource != null)
                        dataSources.Add(dataSource);
                }
                else
                {
                    if (dataSourceReference.Deleted)
                        throw new ResourceProviderException(
                            $"Could not locate the {instance.ResourceId} data source resource.",
                            StatusCodes.Status404NotFound);

                    dataSource = await LoadDataSource(dataSourceReference);
                    if (dataSource != null)
                        dataSources.Add(dataSource);
                }
            }

            return await _authorizationService.FilterResourcesByAuthorizableAction(
                _instanceSettings.Id, userIdentity, dataSources,
                AuthorizableActionNames.FoundationaLLM_DataSource_DataSources_Read);
        }

        private async Task<DataSourceBase?> LoadDataSource(DataSourceReference? dataSourceReference, string? resourceId = null)
        {
            if (dataSourceReference != null || !string.IsNullOrWhiteSpace(resourceId))
            {
                dataSourceReference ??= new DataSourceReference
                {
                    Name = resourceId!,
                    Type = DataSourceTypes.Basic,
                    Filename = $"/{_name}/{resourceId}.json",
                    Deleted = false
                };
                if (await _storageService.FileExistsAsync(_storageContainerName, dataSourceReference.Filename, default))
                {
                    var fileContent = await _storageService.ReadFileAsync(_storageContainerName, dataSourceReference.Filename, default);
                    var dataSource = JsonSerializer.Deserialize(
                               Encoding.UTF8.GetString(fileContent.ToArray()),
                               dataSourceReference.DataSourceType,
                               _serializerSettings) as DataSourceBase
                           ?? throw new ResourceProviderException($"Failed to load the data source {dataSourceReference.Name}.",
                               StatusCodes.Status400BadRequest);

                    if (!string.IsNullOrWhiteSpace(resourceId))
                    {
                        dataSourceReference.Type = dataSource.Type!;
                        _dataSourceReferences.AddOrUpdate(dataSourceReference.Name, dataSourceReference, (k, v) => dataSourceReference);
                    }

                    return dataSource;
                }

                if (string.IsNullOrWhiteSpace(resourceId))
                {
                    // Remove the reference from the dictionary since the file does not exist.
                    _dataSourceReferences.TryRemove(dataSourceReference.Name, out _);
                    return null;
                }
            }
            throw new ResourceProviderException($"Could not locate the {dataSourceReference.Name} data source resource.",
                StatusCodes.Status404NotFound);
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task<object> UpsertResourceAsync(ResourcePath resourcePath, string serializedResource, UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeInstances[0].ResourceType switch
            {
                DataSourceResourceTypeNames.DataSources => await UpdateDataSource(resourcePath, serializedResource, userIdentity),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for UpsertResourceAsync

        private async Task<ResourceProviderUpsertResult> UpdateDataSource(ResourcePath resourcePath, string serializedDataSource, UnifiedUserIdentity userIdentity)
        {
            var dataSource = JsonSerializer.Deserialize<DataSourceBase>(serializedDataSource)
                ?? throw new ResourceProviderException("The object definition is invalid.",
                    StatusCodes.Status400BadRequest);

            if (_dataSourceReferences.TryGetValue(dataSource.Name!, out var existingDataSourceReference)
                && existingDataSourceReference!.Deleted)
                throw new ResourceProviderException($"The data source resource {existingDataSourceReference.Name} cannot be added or updated.",
                        StatusCodes.Status400BadRequest);

            if (resourcePath.ResourceTypeInstances[0].ResourceId != dataSource.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            var dataSourceReference = new DataSourceReference
            {
                Name = dataSource.Name!,
                Type = dataSource.Type!,
                Filename = $"/{_name}/{dataSource.Name}.json",
                Deleted = false
            };

            dataSource.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);

            var validator = _resourceValidatorFactory.GetValidator(dataSourceReference.DataSourceType);
            if (validator is IValidator dataSourceValidator)
            {
                var context = new ValidationContext<object>(dataSource);
                var validationResult = await dataSourceValidator.ValidateAsync(context);
                if (!validationResult.IsValid)
                {
                    throw new ResourceProviderException($"Validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}",
                        StatusCodes.Status400BadRequest);
                }
            }

            if (existingDataSourceReference == null)
                dataSource.CreatedBy = userIdentity.UPN;
            else
                dataSource.UpdatedBy = userIdentity.UPN;

            await _storageService.WriteFileAsync(
                _storageContainerName,
                dataSourceReference.Filename,
                JsonSerializer.Serialize<DataSourceBase>(dataSource, _serializerSettings),
                default,
                default);

            _dataSourceReferences.AddOrUpdate(dataSourceReference.Name, dataSourceReference, (k, v) => dataSourceReference);

            await _storageService.WriteFileAsync(
                    _storageContainerName,
                    DATA_SOURCE_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(DataSourceReferenceStore.FromDictionary(_dataSourceReferences.ToDictionary())),
                    default,
                    default);

            return new ResourceProviderUpsertResult
            {
                ObjectId = (dataSource as DataSourceBase)!.ObjectId
            };
        }

        #endregion

        /// <inheritdoc/>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task<object> ExecuteActionAsync(ResourcePath resourcePath, string serializedAction, UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeInstances.Last().ResourceType switch
            {
                DataSourceResourceTypeNames.DataSources => resourcePath.ResourceTypeInstances.Last().Action switch
                {
                    DataSourceResourceProviderActions.CheckName => CheckDataSourceName(serializedAction),
                    DataSourceResourceProviderActions.Filter => await Filter(serializedAction),
                    DataSourceResourceProviderActions.Purge => await PurgeResource(resourcePath),
                    _ => throw new ResourceProviderException($"The action {resourcePath.ResourceTypeInstances.Last().Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                _ => throw new ResourceProviderException()
            };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        #region Helpers for ExecuteActionAsync

        private ResourceNameCheckResult CheckDataSourceName(string serializedAction)
        {
            var resourceName = JsonSerializer.Deserialize<ResourceName>(serializedAction);
            return _dataSourceReferences.Values.Any(ar => ar.Name.Equals(resourceName!.Name, StringComparison.OrdinalIgnoreCase))
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

        private async Task<List<DataSourceBase>> Filter(string serializedAction)
        {
            var resourceFilter = JsonSerializer.Deserialize<ResourceFilter>(serializedAction) ??
                                 throw new ResourceProviderException("The object definition is invalid. Please provide a resource filter.",
                                       StatusCodes.Status400BadRequest);
            if (resourceFilter.Default.HasValue)
            {
                if (resourceFilter.Default.Value)
                {
                    if (string.IsNullOrWhiteSpace(_defaultDataSourceName))
                        throw new ResourceProviderException("The default data source is not set.",
                            StatusCodes.Status404NotFound);

                    if (!_dataSourceReferences.TryGetValue(_defaultDataSourceName, out var dataSourceReference)
                        || dataSourceReference.Deleted)
                        throw new ResourceProviderException(
                            $"Could not locate the {_defaultDataSourceName} data source resource.",
                            StatusCodes.Status404NotFound);

                    return [await LoadDataSource(dataSourceReference)];
                }
                else
                {
                    return
                    [
                        .. (await Task.WhenAll(
                                _dataSourceReferences.Values
                                          .Where(dsr => !dsr.Deleted && (
                                              string.IsNullOrWhiteSpace(_defaultDataSourceName) ||
                                              !dsr.Name.Equals(_defaultDataSourceName, StringComparison.OrdinalIgnoreCase)))
                                          .Select(dsr => LoadDataSource(dsr))))
                    ];
                }
            }
            else
            {
                // TODO: Apply other filters.
                return
                [
                    .. (await Task.WhenAll(
                        _dataSourceReferences.Values
                            .Where(dsr => !dsr.Deleted)
                            .Select(dsr => LoadDataSource(dsr))))
                ];
            }
        }

        private async Task<ResourceProviderActionResult> PurgeResource(ResourcePath resourcePath)
        {
            var resourceName = resourcePath.ResourceTypeInstances.Last().ResourceId!;
            if (_dataSourceReferences.TryGetValue(resourceName, out var agentReference))
            {
                if (agentReference.Deleted)
                {
                    // Delete the resource file from storage.
                    await _storageService.DeleteFileAsync(
                        _storageContainerName,
                        agentReference.Filename,
                        default);

                    // Remove this resource reference from the store.
                    _dataSourceReferences.TryRemove(resourceName, out _);

                    await _storageService.WriteFileAsync(
                        _storageContainerName,
                        DATA_SOURCE_REFERENCES_FILE_PATH,
                        JsonSerializer.Serialize(DataSourceReferenceStore.FromDictionary(_dataSourceReferences.ToDictionary())),
                        default,
                        default);

                    return new ResourceProviderActionResult(true);
                }
                else
                {
                    throw new ResourceProviderException(
                        $"The {resourceName} data source resource is not soft-deleted and cannot be purged.",
                        StatusCodes.Status400BadRequest);
                }
            }
            else
            {
                throw new ResourceProviderException($"Could not locate the {resourceName} data source resource.",
                    StatusCodes.Status404NotFound);
            }
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task DeleteResourceAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            switch (resourcePath.ResourceTypeInstances.Last().ResourceType)
            {
                case DataSourceResourceTypeNames.DataSources:
                    await DeleteDataSource(resourcePath.ResourceTypeInstances);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances.Last().ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            };
        }

        #region Helpers for DeleteResourceAsync

        private async Task DeleteDataSource(List<ResourceTypeInstance> instances)
        {
            if (_dataSourceReferences.TryGetValue(instances.Last().ResourceId!, out var dataSourceReference))
            {
                if (!dataSourceReference.Deleted)
                {
                    dataSourceReference.Deleted = true;

                    await _storageService.WriteFileAsync(
                        _storageContainerName,
                        DATA_SOURCE_REFERENCES_FILE_PATH,
                        JsonSerializer.Serialize(DataSourceReferenceStore.FromDictionary(_dataSourceReferences.ToDictionary())),
                        default,
                        default);
                }
            }
            else
            {
                throw new ResourceProviderException($"Could not locate the {instances.Last().ResourceId} data source resource.",
                    StatusCodes.Status404NotFound);
            }
        }

        #endregion

        #endregion

        /// <inheritdoc/>
        protected override T GetResourceInternal<T>(ResourcePath resourcePath) where T : class {
            if (resourcePath.ResourceTypeInstances.Count != 1)
                throw new ResourceProviderException($"Invalid resource path");

            if (typeof(T) != typeof(DataSourceBase))
                throw new ResourceProviderException($"The type of requested resource ({typeof(T)}) does not match the resource type specified in the path ({resourcePath.ResourceTypeInstances[0].ResourceType}).");
                     
            _dataSourceReferences.TryGetValue(resourcePath.ResourceTypeInstances[0].ResourceId!, out var dataSourceReference);
            if (dataSourceReference is not null && dataSourceReference.Deleted)
                throw new ResourceProviderException($"The resource {resourcePath.ResourceTypeInstances[0].ResourceId} of type {resourcePath.ResourceTypeInstances[0].ResourceType} has been soft deleted.");

            var dataSource = LoadDataSource(dataSourceReference, resourcePath.ResourceTypeInstances[0].ResourceId).Result;
            return dataSource as T
                ?? throw new ResourceProviderException($"The resource {resourcePath.ResourceTypeInstances[0].ResourceId} of type {resourcePath.ResourceTypeInstances[0].ResourceType} was not found.");
        }
        

        #region Event handling

        /// <inheritdoc/>
        protected override async Task HandleEvents(EventSetEventArgs e)
        {
            _logger.LogInformation("{EventsCount} events received in the {EventsNamespace} events namespace.",
                e.Events.Count, e.Namespace);

            switch (e.Namespace)
            {
                case EventSetEventNamespaces.FoundationaLLM_ResourceProvider_DataSource:
                    foreach (var @event in e.Events)
                        await HandleDataSourceResourceProviderEvent(@event);
                    break;
                default:
                    // Ignore sliently any event namespace that's of no interest.
                    break;
            }

            await Task.CompletedTask;
        }

        private async Task HandleDataSourceResourceProviderEvent(CloudEvent e)
        {
            if (string.IsNullOrWhiteSpace(e.Subject))
                return;

            var fileName = e.Subject.Split("/").Last();

            _logger.LogInformation("The file [{FileName}] managed by the [{ResourceProvider}] resource provider has changed and will be reloaded.",
                fileName, _name);

            var dataSourceReference = new DataSourceReference
            {
                Name = Path.GetFileNameWithoutExtension(fileName),
                Filename = $"/{_name}/{fileName}",
                Type = DataSourceTypes.Basic,
                Deleted = false
            };

            var dataSource = await LoadDataSource(dataSourceReference);
            dataSourceReference.Name = dataSource.Name;
            dataSourceReference.Type = dataSource.Type!;

            _dataSourceReferences.AddOrUpdate(
                dataSourceReference.Name,
                dataSourceReference,
                (k, v) => v);

            _logger.LogInformation("The data source reference for the [{DataSourceName}] agent or type [{DataSourceType}] was loaded.",
                dataSourceReference.Name, dataSourceReference.Type);
        }

        #endregion
    }
}
