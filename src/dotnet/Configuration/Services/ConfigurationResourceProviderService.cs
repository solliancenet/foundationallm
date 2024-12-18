using Azure.Messaging;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.Events;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Authorization;
using FoundationaLLM.Common.Models.Configuration.AppConfiguration;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Events;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Common.Services.ResourceProviders;
using FoundationaLLM.Configuration.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Configuration.Services
{
    /// <summary>
    /// Implements the FoundationaLLM.Configuration resource provider.
    /// </summary>
    /// <param name="instanceOptions">The options providing the <see cref="InstanceSettings"/> with instance settings.</param>
    /// <param name="authorizationService">The <see cref="IAuthorizationServiceClient"/> providing authorization services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="eventService">The <see cref="IEventService"/> providing event services.</param>
    /// <param name="resourceValidatorFactory">The <see cref="IResourceValidatorFactory"/> providing the factory to create resource validators.</param>
    /// <param name="appConfigurationService">The <see cref="IAzureAppConfigurationService"/> provding access to the app configuration service.</param>
    /// <param name="keyVaultService">The <see cref="IAzureKeyVaultService"/> providing access to the key vault service.</param>
    /// <param name="configurationManager">The <see cref="IConfigurationManager"/> providing configuration services.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> of the main dependency injection container.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class ConfigurationResourceProviderService(
        IOptions<InstanceSettings> instanceOptions,
        IAuthorizationServiceClient authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Configuration)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IAzureAppConfigurationService appConfigurationService,
        IAzureKeyVaultService keyVaultService,
        IConfigurationManager configurationManager,
        IServiceProvider serviceProvider,
        ILogger<ConfigurationResourceProviderService> logger)
        : ResourceProviderServiceBase<APIEndpointReference>(
            instanceOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            logger,
            [
                EventTypes.FoundationaLLM_ResourceProvider_Configuration
            ],
            useInternalReferencesStore: true)
    {
        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            ConfigurationResourceProviderMetadata.AllowedResourceTypes;

        private const string KEY_VAULT_REFERENCE_CONTENT_TYPE = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8";

        private readonly IAzureAppConfigurationService _appConfigurationService = appConfigurationService;
        private readonly IAzureKeyVaultService _keyVaultService = keyVaultService;
        private readonly IConfigurationManager _configurationManager = configurationManager;

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Configuration;

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
                ConfigurationResourceTypeNames.AppConfigurations => await LoadAppConfigurationKeys(resourcePath.ResourceTypeInstances[0]),
                ConfigurationResourceTypeNames.APIEndpointConfigurations => await LoadResources<APIEndpointConfiguration>(
                    resourcePath.ResourceTypeInstances[0],
                    authorizationResult,
                    options ?? new ResourceProviderGetOptions
                    {
                        IncludeRoles = resourcePath.IsResourceTypePath,
                    }),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.MainResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        /// <inheritdoc/>
        protected override async Task<object> UpsertResourceAsync(ResourcePath resourcePath, string? serializedResource, ResourceProviderFormFile? formFile, UnifiedUserIdentity userIdentity) =>
            resourcePath.MainResourceTypeName switch
            {
                ConfigurationResourceTypeNames.AppConfigurations => await UpdateAppConfigurationKey(resourcePath, serializedResource!),
                ConfigurationResourceTypeNames.APIEndpointConfigurations => await UpdateAPIEndpoints(resourcePath, serializedResource!, userIdentity),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.MainResourceTypeName} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        /// <inheritdoc/>
        protected override async Task DeleteResourceAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            switch (resourcePath.ResourceTypeName)
            {
                case ConfigurationResourceTypeNames.APIEndpointConfigurations:
                    await DeleteResource<APIEndpointConfiguration>(resourcePath);
                    break;
                case ConfigurationResourceTypeNames.AppConfigurations:
                    await DeleteAppConfigurationKey(resourcePath.ResourceTypeInstances);
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
        protected override async Task<T> GetResourceAsyncInternal<T>(ResourcePath resourcePath, ResourcePathAuthorizationResult authorizationResult, UnifiedUserIdentity userIdentity, ResourceProviderGetOptions? options = null)
        {
            switch (typeof(T))
            {
                case Type t when t == typeof(APIEndpointConfiguration):
                    var apiEndpoint = await LoadResource<T>(resourcePath.ResourceId!);
                    return apiEndpoint
                        ?? throw new ResourceProviderException(
                            $"The resource {resourcePath.ResourceTypeInstances[0].ResourceId!} of type {resourcePath.ResourceTypeInstances[0].ResourceType} could not be loaded.",
                            StatusCodes.Status500InternalServerError);
                case Type t when t == typeof(AppConfigurationKeyBase):
                    var appConfigKeys = await LoadAppConfigurationKeys(resourcePath.ResourceTypeInstances[0]);
                    return appConfigKeys.FirstOrDefault()?.Resource as T
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
                case EventTypes.FoundationaLLM_ResourceProvider_Configuration:
                    foreach (var @event in e.Events)
                        await HandleConfigurationResourceProviderEvent(@event);
                    break;
                default:
                    // Ignore sliently any event namespace that's of no interest.
                    break;
            }

            await Task.CompletedTask;
        }

        private async Task HandleConfigurationResourceProviderEvent(CloudEvent e)
        {
            if (string.IsNullOrWhiteSpace(e.Subject))
                return;

            try
            {
                var eventData = JsonSerializer.Deserialize<AppConfigurationEventData>(e.Data);
                if (eventData == null)
                    throw new ResourceProviderException("Invalid app configuration event data.");

                _logger.LogInformation("The value [{AppConfigurationKey}] managed by the [{ResourceProvider}] resource provider has changed and will be reloaded.",
                    eventData.Key, _name);

                var keyValue = await _appConfigurationService.GetConfigurationSettingAsync(eventData.Key);

                try
                {
                    var keyVaultSecret = JsonSerializer.Deserialize<AppConfigurationKeyVaultUri>(keyValue!);
                    if (keyVaultSecret != null
                        & !string.IsNullOrWhiteSpace(keyVaultSecret!.Uri))
                        keyValue = await _keyVaultService.GetSecretValueAsync(
                            keyVaultSecret.Uri!.Split('/').Last());
                }
                catch { }

                _configurationManager[eventData.Key] = keyValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while handling the app configuration event.");
            }
        }

        #endregion

        #region Resource management

        private async Task<List<ResourceProviderGetResult<AppConfigurationKeyBase>>> LoadAppConfigurationKeys(ResourceTypeInstance instance)
        {
            var keyFilter = instance.ResourceId ?? "FoundationaLLM:*";
            var result = new List<ResourceProviderGetResult<AppConfigurationKeyBase>>();

            var settings = await _appConfigurationService.GetConfigurationSettingsAsync(keyFilter);
            foreach (var setting in settings)
            {
                AppConfigurationKeyBase? appConfig = new AppConfigurationKeyValue
                {
                    ObjectId = $"/instances/{_instanceSettings.Id}/providers/{_name}/{ConfigurationResourceTypeNames.AppConfigurations}/{setting.Key}",
                    Name = setting.Key,
                    DisplayName = setting.Key,
                    Key = setting.Key,
                    Value = setting.Value,
                    ContentType = setting.ContentType,
                    Type = ConfigurationTypes.AppConfigurationKeyValue
                };

                if (string.IsNullOrEmpty(setting.Value))
                {
                    result.Add(new ResourceProviderGetResult<AppConfigurationKeyBase>() { Resource = appConfig, Roles = [], Actions = [] });
                    continue;
                }

                if (!string.IsNullOrEmpty(setting.ContentType)
                    && setting.ContentType.StartsWith(KEY_VAULT_REFERENCE_CONTENT_TYPE))
                {
                    var kvAppConfig = await TryGetAsKeyVaultReference(setting.Key, setting.Value);
                    if (kvAppConfig != null)
                        appConfig = kvAppConfig;
                }

                result.Add(new ResourceProviderGetResult<AppConfigurationKeyBase>() { Resource = appConfig, Roles = [], Actions = [] });
            }

            return result;
        }

        private async Task<ResourceProviderUpsertResult> UpdateAppConfigurationKey(ResourcePath resourcePath, string serializedAppConfig)
        {
            var appConfig = JsonSerializer.Deserialize<AppConfigurationKeyValue>(serializedAppConfig)
                ?? throw new ResourceProviderException("Invalid app configuration key value.", StatusCodes.Status400BadRequest);

            if (string.IsNullOrWhiteSpace(appConfig.Key))
                throw new ResourceProviderException("The key name is invalid.", StatusCodes.Status400BadRequest);

            if (appConfig.ContentType == null)
                throw new ResourceProviderException("The key content type is invalid.", StatusCodes.Status400BadRequest);

            if (appConfig.ContentType.StartsWith(KEY_VAULT_REFERENCE_CONTENT_TYPE))
            {
                var kvAppConfig = JsonSerializer.Deserialize<AppConfigurationKeyVaultReference>(serializedAppConfig)
                    ?? throw new ResourceProviderException("Invalid key vault reference value.", StatusCodes.Status400BadRequest);

                kvAppConfig.KeyVaultUri = _keyVaultService.KeyVaultUri;

                if (string.IsNullOrWhiteSpace(kvAppConfig.KeyVaultSecretName))
                    throw new ResourceProviderException("The key vault secret name is invalid.", StatusCodes.Status400BadRequest);

                await _keyVaultService.SetSecretValueAsync(kvAppConfig.KeyVaultSecretName.ToLower(), kvAppConfig.Value!);
                await _appConfigurationService.SetConfigurationSettingAsync(
                    appConfig.Key,
                    JsonSerializer.Serialize(new AppConfigurationKeyVaultUri
                    {
                        Uri = new Uri(new Uri(kvAppConfig.KeyVaultUri), $"/secrets/{kvAppConfig.KeyVaultSecretName}").AbsoluteUri
                    }),
                    appConfig.ContentType);

            }
            else
                await _appConfigurationService.SetConfigurationSettingAsync(appConfig.Key, appConfig.Value, appConfig.ContentType);

            return new ResourceProviderUpsertResult
            {
                ObjectId = $"/instances/{_instanceSettings.Id}/providers/{_name}/{ConfigurationResourceTypeNames.AppConfigurations}/{appConfig.Key}",
                ResourceExists = false
            };
        }

        private async Task<ResourceProviderUpsertResult> UpdateAPIEndpoints(ResourcePath resourcePath, string serializedAPIEndpoint, UnifiedUserIdentity userIdentity)
        {
            var apiEndpoint = JsonSerializer.Deserialize<APIEndpointConfiguration>(serializedAPIEndpoint)
               ?? throw new ResourceProviderException("The object definition is invalid.",
                StatusCodes.Status400BadRequest);

            var existingApiEndpointReference = await _resourceReferenceStore!.GetResourceReference(apiEndpoint.Name);

            if (resourcePath.ResourceTypeInstances[0].ResourceId != apiEndpoint.Name)
                throw new ResourceProviderException("The resource path does not match the object definition (name mismatch).",
                    StatusCodes.Status400BadRequest);

            var apiEndpointReference = new APIEndpointReference
            {
                Name = apiEndpoint.Name!,
                Type = apiEndpoint.Type!,
                Filename = $"/{_name}/{apiEndpoint.Name}.json",
                Deleted = false
            };

            apiEndpoint.ObjectId = resourcePath.GetObjectId(_instanceSettings.Id, _name);

            //TODO: Add validation for the API endpoint configuration

            UpdateBaseProperties(apiEndpoint, userIdentity, isNew: existingApiEndpointReference == null);
            if (existingApiEndpointReference == null)
                await CreateResource<APIEndpointConfiguration>(apiEndpointReference, apiEndpoint);
            else
                await SaveResource<APIEndpointConfiguration>(existingApiEndpointReference, apiEndpoint);

            return new ResourceProviderUpsertResult
            {
                ObjectId = apiEndpoint!.ObjectId,
                ResourceExists = existingApiEndpointReference != null
            };
        }

        private async Task DeleteAppConfigurationKey(List<ResourceTypeInstance> instances)
        {
            string key = instances.Last().ResourceId!.Split("/").Last();
            if (!await _appConfigurationService.CheckAppConfigurationSettingExistsAsync(key))
                throw new ResourceProviderException($"Could not locate the {key} App Configuration key.",
                                StatusCodes.Status404NotFound);
            await _appConfigurationService.DeleteAppConfigurationSettingAsync(key);
        }

        private async Task<AppConfigurationKeyVaultReference?> TryGetAsKeyVaultReference(string keyName, string keyValue)
        {
            try
            {
                var keyVaultSecretUri = JsonSerializer.Deserialize<AppConfigurationKeyVaultUri>(keyValue);
                if (keyVaultSecretUri != null
                    && !string.IsNullOrWhiteSpace(keyVaultSecretUri!.Uri))
                {
                    var uri = new Uri(keyVaultSecretUri.Uri!);
                    var keyVaultUri = $"https://{uri.Host}";
                    var secretName = uri.AbsolutePath.Split('/').Last();
                    var secretValue = await _keyVaultService.GetSecretValueAsync(secretName);

                    return new AppConfigurationKeyVaultReference
                    {
                        ObjectId = $"/instances/{_instanceSettings.Id}/providers/{_name}/{ConfigurationResourceTypeNames.AppConfigurations}/{keyName}",
                        Name = keyName,
                        DisplayName = keyName,
                        Key = keyName,
                        Value = secretValue,
                        KeyVaultUri = keyVaultUri,
                        KeyVaultSecretName = secretName,
                        Type = ConfigurationTypes.AppConfigurationKeyVaultReference
                    };
                }

                _logger.LogWarning("The key {KeyName} is not a valid key vault reference.", keyName);
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "The key {KeyName} is not a valid key vault reference.", keyName);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the key vault value for the key {KeyName}.", keyName);
                return null;
            }
        }

        #endregion
    }
}
