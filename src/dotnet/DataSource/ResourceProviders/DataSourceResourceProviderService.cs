using Azure.Messaging;
using FluentValidation;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.Events;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
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
using System.Text.Json;

namespace FoundationaLLM.DataSource.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.DataSource resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationServiceClient"/> providing authorization services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to provide loggers for logging.</param>
    public class DataSourceResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IAuthorizationServiceClient authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_DataSource)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory)
        : ResourceProviderServiceBase<DataSourceReference>(
            instanceOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            loggerFactory.CreateLogger<DataSourceResourceProviderService>(),
            [
                EventTypes.FoundationaLLM_ResourceProvider_DataSource
            ],
            useInternalReferencesStore: true)
    {
        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            DataSourceResourceProviderMetadata.AllowedResourceTypes;

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_DataSource;

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
            resourcePath.MainResourceTypeName switch
            {
                DataSourceResourceTypeNames.DataSources => await LoadResources<DataSourceBase>(
                    resourcePath.ResourceTypeInstances[0],
                    authorizationResult,
                    options ?? new ResourceProviderGetOptions
                    {
                        IncludeRoles = resourcePath.IsResourceTypePath
                    }),
                _ => throw new ResourceProviderException(
                        $"The resource type {resourcePath.MainResourceTypeName} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
            };

        /// <inheritdoc/>
        protected override async Task<object> UpsertResourceAsync(ResourcePath resourcePath, string? serializedResource, ResourceProviderFormFile? formFile, UnifiedUserIdentity userIdentity) =>
            resourcePath.MainResourceTypeName switch
            {
                DataSourceResourceTypeNames.DataSources => await UpdateDataSource(resourcePath, serializedResource!, userIdentity),
                _ => throw new ResourceProviderException(
                        $"The resource type {resourcePath.MainResourceTypeName} is not supported by the {_name} resource provider.",
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
                DataSourceResourceTypeNames.DataSources => resourcePath.Action switch
                {
                    ResourceProviderActions.CheckName => await CheckResourceName<DataSourceBase>(
                        JsonSerializer.Deserialize<ResourceName>(serializedAction)!),
                    ResourceProviderActions.Filter => await FilterResources<DataSourceBase>(
                        resourcePath,
                        JsonSerializer.Deserialize<ResourceFilter>(serializedAction)!,
                        authorizationResult,
                        new ResourceProviderGetOptions
                        {
                            LoadContent = false,
                            IncludeRoles = false
                        }),
                    ResourceProviderActions.Purge => await PurgeResource<DataSourceBase>(resourcePath),
                    _ => throw new ResourceProviderException(
                        $"The action {resourcePath.Action} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
                },
                _ => throw new ResourceProviderException()
            };

        /// <inheritdoc/>
        protected override async Task DeleteResourceAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            switch (resourcePath.ResourceTypeName)
            {
                case DataSourceResourceTypeNames.DataSources:
                    await DeleteResource<DataSourceBase>(resourcePath);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            };
        }

        #endregion

        #region Resource provider strongly typed operations

        /// <inheritdoc/>
        protected override async Task<T> GetResourceAsyncInternal<T>(ResourcePath resourcePath, ResourcePathAuthorizationResult authorizationResult, UnifiedUserIdentity userIdentity, ResourceProviderGetOptions? options = null)
        {
            switch (typeof(T))
            {
                case Type t when t == typeof(DataSourceBase):
                    var apiEndpoint = await LoadResource<T>(resourcePath.ResourceId!);
                    return apiEndpoint
                        ?? throw new ResourceProviderException(
                            $"The resource {resourcePath.ResourceTypeInstances[0].ResourceId!} of type {resourcePath.ResourceTypeInstances[0].ResourceType} could not be loaded.",
                            StatusCodes.Status500InternalServerError);
                default:
                    throw new ResourceProviderException(
                        $"The resource type {typeof(T).Name} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest);
            }
        }

        #endregion

        #region Event handling

        /// <inheritdoc/>
        protected override async Task HandleEvents(EventTypeEventArgs e)
        {
            _logger.LogInformation("{EventsCount} events received in the {EventsNamespace} events namespace.",
                e.Events.Count, e.EventType);

            switch (e.EventType)
            {
                case EventTypes.FoundationaLLM_ResourceProvider_DataSource:
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
            await Task.CompletedTask;
            return;

            // Event handling is temporarily disabled until the updated event handling mechanism is implemented.

            //if (string.IsNullOrWhiteSpace(e.Subject))
            //    return;

            //var fileName = e.Subject.Split("/").Last();

            //_logger.LogInformation("The file [{FileName}] managed by the [{ResourceProvider}] resource provider has changed and will be reloaded.",
            //    fileName, _name);

            //var dataSourceReference = new DataSourceReference
            //{
            //    Name = Path.GetFileNameWithoutExtension(fileName),
            //    Filename = $"/{_name}/{fileName}",
            //    Type = DataSourceTypes.Basic,
            //    Deleted = false
            //};

            //var dataSource = await LoadDataSource(dataSourceReference);
            //dataSourceReference.Name = dataSource.Name;
            //dataSourceReference.Type = dataSource.Type!;

            //_dataSourceReferences.AddOrUpdate(
            //    dataSourceReference.Name,
            //    dataSourceReference,
            //    (k, v) => v);

            //_logger.LogInformation("The data source reference for the [{DataSourceName}] agent or type [{DataSourceType}] was loaded.",
            //    dataSourceReference.Name, dataSourceReference.Type);
        }

        #endregion

        #region Resource management

        private async Task<ResourceProviderUpsertResult> UpdateDataSource(ResourcePath resourcePath, string serializedDataSource, UnifiedUserIdentity userIdentity)
        {
            var dataSource = JsonSerializer.Deserialize<DataSourceBase>(serializedDataSource)
                ?? throw new ResourceProviderException("The object definition is invalid.",
                    StatusCodes.Status400BadRequest);

            var existingDataSourceReference = await _resourceReferenceStore!.GetResourceReference(dataSource.Name);

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

            var validator = _resourceValidatorFactory.GetValidator(dataSourceReference.ResourceType);
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

            UpdateBaseProperties(dataSource, userIdentity, isNew: existingDataSourceReference == null);
            if (existingDataSourceReference == null)
                await CreateResource<DataSourceBase>(dataSourceReference, dataSource);
            else
                await SaveResource<DataSourceBase>(existingDataSourceReference, dataSource);

            return new ResourceProviderUpsertResult
            {
                ObjectId = dataSource!.ObjectId,
                ResourceExists = existingDataSourceReference != null
            };
        }

        #endregion
    }
}
