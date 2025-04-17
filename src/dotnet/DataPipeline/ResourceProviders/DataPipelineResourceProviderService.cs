using FluentValidation;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.Events;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.DataPipeline.Interfaces;
using FoundationaLLM.DataPipeline.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FoundationaLLM.DataPipeline.ResourceProviders
{
    /// <summary>
    /// Implements the FoundationaLLM.DataPipeline resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>    
    /// <param name="cacheOptions">The options providing the <see cref="ResourceProviderCacheSettings"/> with settings for the resource provider cache.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationServiceClient"/> providing authorization services.</param>
    /// <param name="dataPipelineServiceClient">The client for the Data Pipeline API.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="loggerFactory">The factory responsible for creating loggers.</param>    
    public class DataPipelineResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IOptions<ResourceProviderCacheSettings> cacheOptions,
        IAuthorizationServiceClient authorizationService,
        IDataPipelineServiceClient dataPipelineServiceClient,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_DataPipeline_Storage)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory)
        : ResourceProviderServiceBase<DataPipelineReference>(
            instanceOptions.Value,
            cacheOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            loggerFactory.CreateLogger<DataPipelineResourceProviderService>(),
            [
                EventTypes.FoundationaLLM_ResourceProvider_Cache_ResetCommand
            ],
            useInternalReferencesStore: true)
    {
        private readonly IDataPipelineServiceClient _dataPipelineServiceClient = dataPipelineServiceClient;

        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            DataPipelineResourceProviderMetadata.AllowedResourceTypes;

        protected override string _name => ResourceProviderNames.FoundationaLLM_DataPipeline;

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
                DataPipelineResourceTypeNames.DataPipelines => await LoadResources<DataPipelineDefinition>(
                    resourcePath.ResourceTypeInstances[0],
                    authorizationResult,
                    options ?? new ResourceProviderGetOptions
                    {
                        IncludeRoles = resourcePath.IsResourceTypePath
                    }),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                        StatusCodes.Status400BadRequest)
            };

        protected override async Task<object> UpsertResourceAsync(
            ResourcePath resourcePath,
            string? serializedResource,
            ResourceProviderFormFile? formFile,
            UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeName switch
            {
                DataPipelineResourceTypeNames.DataPipelines => await UpdateDataPipeline(
                    resourcePath,
                    serializedResource!,
                    userIdentity),
                _ => throw new ResourceProviderException(
                    $"The resource type {resourcePath.ResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for UpsertResourceAsync

        private async Task<ResourceProviderUpsertResult> UpdateDataPipeline(
            ResourcePath resourcePath,
            string serializedDataPipeline,
            UnifiedUserIdentity userIdentity)
        {
            var dataPipeline = JsonSerializer.Deserialize<DataPipelineDefinition>(serializedDataPipeline)
                ?? throw new ResourceProviderException("The object definition is invalid.",
                StatusCodes.Status400BadRequest);

            var existingDataPipelineReference = await _resourceReferenceStore!.GetResourceReference(dataPipeline.Name);

            if (resourcePath.ResourceTypeInstances[0].ResourceId != dataPipeline.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                        StatusCodes.Status400BadRequest);

            var dataPipelineReference = new DataPipelineReference
            {
                Name = dataPipeline.Name!,
                Type = dataPipeline.Type!,
                Filename = $"/{_name}/{dataPipeline.Name}.json",
                Deleted = false
            };

            dataPipeline.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);

            var validator = _resourceValidatorFactory.GetValidator(dataPipelineReference.ResourceType);
            if (validator is IValidator dataPipelineValidator)
            {
                var context = new ValidationContext<object>(dataPipeline);
                var validationResult = await dataPipelineValidator.ValidateAsync(context);
                if (!validationResult.IsValid)
                    throw new ResourceProviderException($"Validation failed: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}",
                        StatusCodes.Status400BadRequest);
            }

            UpdateBaseProperties(dataPipeline, userIdentity, isNew: existingDataPipelineReference == null);
            if (existingDataPipelineReference == null)
                await CreateResource<DataPipelineDefinition>(dataPipelineReference, dataPipeline);
            else
                await SaveResource<DataPipelineDefinition>(dataPipelineReference, dataPipeline);

            return new ResourceProviderUpsertResult
            {
                ObjectId = dataPipeline!.ObjectId,
                ResourceExists = existingDataPipelineReference != null
            };
        }

        #endregion

        #endregion

        #region Resource provider strongly typed operations

        protected override async Task<TResult> UpsertResourceAsyncInternal<T, TResult>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            T resource,
            UnifiedUserIdentity userIdentity,
            ResourceProviderUpsertOptions? options = null) =>
            typeof(TResult) switch
            {
                Type t when t == typeof(DataPipelineRun) =>
                    await _dataPipelineServiceClient.CreateDataPipelineRunAsync(
                        resourcePath.InstanceId!,
                        (resource as DataPipelineRun)!,
                        userIdentity) as TResult
                        ?? throw new ResourceProviderException("The object definition is invalid.",
                            StatusCodes.Status400BadRequest),
                _ => throw new ResourceProviderException(
                        $"The resource type {typeof(T).Name} is not supported by the {_name} resource provider.",
                            StatusCodes.Status400BadRequest)
            };

        protected override async Task<T> GetResourceAsyncInternal<T>(
            ResourcePath resourcePath,
            ResourcePathAuthorizationResult authorizationResult,
            UnifiedUserIdentity userIdentity,
            ResourceProviderGetOptions? options = null) =>
            typeof(T) switch
            {
                Type t when t == typeof(DataPipelineRun) =>
                    await _dataPipelineServiceClient.GetDataPipelineRunAsync(
                        resourcePath.InstanceId!,
                        resourcePath.MainResourceId!,
                        userIdentity) as T
                        ?? throw new ResourceProviderException("The object definition is invalid.",
                            StatusCodes.Status400BadRequest),
                _ => throw new ResourceProviderException(
                        $"The resource type {typeof(T).Name} is not supported by the {_name} resource provider.",
                            StatusCodes.Status400BadRequest)
            };

        #endregion
    }
}
