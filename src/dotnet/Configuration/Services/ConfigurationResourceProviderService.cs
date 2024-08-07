using Azure.Messaging;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Configuration.AppConfiguration;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Events;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.AIModel;
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
    /// <param name="authorizationService">The <see cref="IAuthorizationService"/> providing authorization services.</param>
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
        IAuthorizationService authorizationService,
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Configuration)] IStorageService storageService,
        IEventService eventService,
        IResourceValidatorFactory resourceValidatorFactory,
        IAzureAppConfigurationService appConfigurationService,
        IAzureKeyVaultService keyVaultService,
        IConfigurationManager configurationManager,
        IServiceProvider serviceProvider,
        ILogger<ConfigurationResourceProviderService> logger)
        : ResourceProviderServiceBase(
            instanceOptions.Value,
            authorizationService,
            storageService,
            eventService,
            resourceValidatorFactory,
            serviceProvider,
            logger,
            [
                EventSetEventNamespaces.FoundationaLLM_ResourceProvider_Configuration
            ])
    {
        /// <inheritdoc/>
        protected override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            ConfigurationResourceProviderMetadata.AllowedResourceTypes;

        private ConcurrentDictionary<string, APIEndpointReference> _apiEndpointReferences = [];

        private const string KEY_VAULT_REFERENCE_CONTENT_TYPE = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8";

        private const string API_ENDPOINT_REFERENCES_FILE_NAME = "_api-endpoint-references.json";
        private const string API_ENDPOINT_REFERENCES_FILE_PATH =
            $"/{ResourceProviderNames.FoundationaLLM_Configuration}/{API_ENDPOINT_REFERENCES_FILE_NAME}";

        private readonly IAzureAppConfigurationService _appConfigurationService = appConfigurationService;
        private readonly IAzureKeyVaultService _keyVaultService = keyVaultService;
        private readonly IConfigurationManager _configurationManager = configurationManager;

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Configuration;

        /// <inheritdoc/>
        protected override async Task InitializeInternal()
        {
            _logger.LogInformation("Starting to initialize the {ResourceProvider} resource provider...", _name);

            if (await _storageService.FileExistsAsync(_storageContainerName, API_ENDPOINT_REFERENCES_FILE_PATH, default))
            {
                var fileContent = await _storageService.ReadFileAsync(
                    _storageContainerName,
                    API_ENDPOINT_REFERENCES_FILE_PATH,
                    default);

                var resourceReferenceStore =
                    JsonSerializer.Deserialize<ResourceReferenceStore<APIEndpointReference>>(
                        Encoding.UTF8.GetString(fileContent.ToArray()));

                _apiEndpointReferences = new ConcurrentDictionary<string, APIEndpointReference>(
                        resourceReferenceStore!.ToDictionary());
            }
            else
            {
                await _storageService.WriteFileAsync(
                    _storageContainerName,
                    API_ENDPOINT_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(new ResourceReferenceStore<APIEndpointReference>
                    {
                        ResourceReferences = []
                    }),
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
                ConfigurationResourceTypeNames.AppConfigurations => await LoadAppConfigurationKeys(resourcePath.ResourceTypeInstances[0]),
                ConfigurationResourceTypeNames.APIEndpointConfigurations => await LoadAPIEndpoints(resourcePath.ResourceTypeInstances[0]),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for GetResourcesAsyncInternal

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
                    result.Add(new ResourceProviderGetResult<AppConfigurationKeyBase>() { Resource = appConfig, Actions = [], Roles = [] });
                    continue;
                }

                if (!string.IsNullOrEmpty(setting.ContentType)
                    && setting.ContentType.StartsWith(KEY_VAULT_REFERENCE_CONTENT_TYPE))
                {
                    var kvAppConfig = await TryGetAsKeyVaultReference(setting.Key, setting.Value);
                    if (kvAppConfig != null)
                        appConfig = kvAppConfig;
                }

                result.Add(new ResourceProviderGetResult<AppConfigurationKeyBase>() { Resource = appConfig, Actions = [], Roles = [] });
            }

            return result;
        }

        private async Task<List<ResourceProviderGetResult<APIEndpointConfiguration>>> LoadAPIEndpoints(ResourceTypeInstance instance)
        {
            if (instance.ResourceId == null)
            {
                var apiEndpoints = (await Task.WhenAll(
                        _apiEndpointReferences.Values
                            .Where(apie => !apie.Deleted)
                            .Select(apie => LoadAPIEndpoint(apie)))).ToList();

                return apiEndpoints.Select(service => new ResourceProviderGetResult<APIEndpointConfiguration>() { Resource = service, Actions = [], Roles = [] }).ToList();
            }
            else
            {
                if (!_apiEndpointReferences.TryGetValue(instance.ResourceId, out var resourceReference)
                    || resourceReference.Deleted)
                    throw new ResourceProviderException($"Could not locate the {instance.ResourceId} api endpoint resource.",
                        StatusCodes.Status404NotFound);

                var apiEndpoint = await LoadAPIEndpoint(resourceReference);

                return [new ResourceProviderGetResult<APIEndpointConfiguration>() { Resource = apiEndpoint, Actions = [], Roles = [] }];
            }
        }

        private async Task<APIEndpointConfiguration> LoadAPIEndpoint(
            APIEndpointReference apiEndpointReference)
        {
            if (await _storageService.FileExistsAsync(_storageContainerName, apiEndpointReference.Filename, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, apiEndpointReference.Filename, default);

                return JsonSerializer.Deserialize<APIEndpointConfiguration>(
                    Encoding.UTF8.GetString(fileContent.ToArray()),
                    _serializerSettings)
                    ?? throw new ResourceProviderException($"Failed to load the api endpoint {apiEndpointReference.Name}.",
                        StatusCodes.Status400BadRequest);
            }

            throw new ResourceProviderException($"Could not locate the {apiEndpointReference.Name} api endpoint resource.",
                StatusCodes.Status404NotFound);
        }

        /// <inheritdoc/>
        protected override T GetResourceInternal<T>(ResourcePath resourcePath) where T : class
        {
            if (resourcePath.ResourceTypeInstances.Count != 1)
                throw new ResourceProviderException($"Invalid resource path");

            if (typeof(T) != typeof(APIEndpointConfiguration))
                throw new ResourceProviderException($"The type of requested resource ({typeof(T)}) does not match the resource type specified in the path ({resourcePath.ResourceTypeInstances[0].ResourceType}).");

            _apiEndpointReferences.TryGetValue(resourcePath.ResourceTypeInstances[0].ResourceId!, out var apiEndpointReference);
            if (apiEndpointReference == null || apiEndpointReference.Deleted)
                throw new ResourceProviderException($"The resource {resourcePath.ResourceTypeInstances[0].ResourceId!} of type {resourcePath.ResourceTypeInstances[0].ResourceType} was not found.");

            var apiEndpoint = LoadAPIEndpoint(apiEndpointReference).Result;
            return apiEndpoint as T
                ?? throw new ResourceProviderException($"The resource {resourcePath.ResourceTypeInstances[0].ResourceId!} of type {resourcePath.ResourceTypeInstances[0].ResourceType} was not found.");
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task<object> UpsertResourceAsync(ResourcePath resourcePath, string serializedResource, UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeInstances[0].ResourceType switch
            {
                ConfigurationResourceTypeNames.AppConfigurations => await UpdateAppConfigurationKey(resourcePath, serializedResource),
                ConfigurationResourceTypeNames.APIEndpointConfigurations => await UpdateAPIEndpoints(resourcePath, serializedResource, userIdentity),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        /// <inheritdoc/>
        protected override async Task DeleteResourceAsync(ResourcePath resourcePath, UnifiedUserIdentity userIdentity)
        {
            switch (resourcePath.ResourceTypeInstances.Last().ResourceType)
            {
                case ConfigurationResourceTypeNames.APIEndpointConfigurations:
                    await DeleteAPIEndpoint(resourcePath.ResourceTypeInstances);
                    break;
                case ConfigurationResourceTypeNames.AppConfigurations:
                    await DeleteAppConfigurationKey(resourcePath.ResourceTypeInstances);
                    break;
                default:
                    throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances.Last().ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest);
            };
        }

        #endregion

        #region Helpers for UpsertResourceAsync

        private async Task<ResourceProviderUpsertResult> UpdateAppConfigurationKey(ResourcePath resourcePath, string serializedAppConfig)
        {
            var appConfig = JsonSerializer.Deserialize<AppConfigurationKeyValue>(serializedAppConfig)
                ?? throw new ResourceProviderException("Invalid app configuration key value.", StatusCodes.Status400BadRequest);

            if (string.IsNullOrWhiteSpace(appConfig.Key))
                throw new ResourceProviderException("The key name is invalid.", StatusCodes.Status400BadRequest);

            if (string.IsNullOrWhiteSpace(appConfig.Value))
                throw new ResourceProviderException("The key value is invalid.", StatusCodes.Status400BadRequest);

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
                ObjectId = $"/instances/{_instanceSettings.Id}/providers/{_name}/{ConfigurationResourceTypeNames.AppConfigurations}/{appConfig.Key}"
            };
        }

        private async Task<ResourceProviderUpsertResult> UpdateAPIEndpoints(ResourcePath resourcePath, string serializedAPIEndpoint, UnifiedUserIdentity userIdentity)
        {
            var apiEndpoint = JsonSerializer.Deserialize<APIEndpointConfiguration>(serializedAPIEndpoint)
               ?? throw new ResourceProviderException("The object definition is invalid.");

            if (_apiEndpointReferences.TryGetValue(apiEndpoint.Name!, out var existingApiEndpointReference)
                && existingApiEndpointReference!.Deleted)
                throw new ResourceProviderException($"The api endpoint resource {existingApiEndpointReference.Name} cannot be added or updated.",
                        StatusCodes.Status400BadRequest);

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

            if (existingApiEndpointReference == null)
                apiEndpoint.CreatedBy = userIdentity.UPN;
            else
                apiEndpoint.UpdatedBy = userIdentity.UPN;

            await _storageService.WriteFileAsync(
                _storageContainerName,
                apiEndpointReference.Filename,
                JsonSerializer.Serialize(apiEndpoint, _serializerSettings),
                default,
                default);

            _apiEndpointReferences.AddOrUpdate(apiEndpointReference.Name, apiEndpointReference, (k, v) => v);

            await _storageService.WriteFileAsync(
                    _storageContainerName,
                    API_ENDPOINT_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(new ResourceReferenceStore<APIEndpointReference>() { ResourceReferences = _apiEndpointReferences.Values.ToList() }),
                    default,
                    default);

            return new ResourceProviderUpsertResult
            {
                ObjectId = (apiEndpoint as APIEndpointConfiguration)!.ObjectId
            };
        }

        #endregion

        #region Helpers for DeleteResourceAsync

        private async Task DeleteAPIEndpoint(List<ResourceTypeInstance> instances)
        {
            if (_apiEndpointReferences.TryGetValue(instances.Last().ResourceId!, out var apiEndpointReference)
                || apiEndpointReference!.Deleted)
            {
                apiEndpointReference.Deleted = true;

                await _storageService.WriteFileAsync(
                    _storageContainerName,
                    API_ENDPOINT_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(new ResourceReferenceStore<APIEndpointReference>() { ResourceReferences = _apiEndpointReferences.Values.ToList() }),
                    default,
                    default);
            }
            else
                throw new ResourceProviderException($"Could not locate the {instances.Last().ResourceId} api endpoint resource.",
                            StatusCodes.Status404NotFound);
        }

        private async Task DeleteAppConfigurationKey(List<ResourceTypeInstance> instances)
        {
            string key = instances.Last().ResourceId!.Split("/").Last();
            if (!await _appConfigurationService.CheckAppConfigurationSettingExistsAsync(key))
                throw new ResourceProviderException($"Could not locate the {key} App Configuration key.",
                                StatusCodes.Status404NotFound);
            await _appConfigurationService.DeleteAppConfigurationSettingAsync(key);
        }
        #endregion

        #region Event handling

        /// <inheritdoc/>
        protected override async Task HandleEvents(EventSetEventArgs e)
        {
            _logger.LogInformation("{EventsCount} events received in the {EventsNamespace} events namespace.",
                e.Events.Count, e.Namespace);

            switch (e.Namespace)
            {
                case EventSetEventNamespaces.FoundationaLLM_ResourceProvider_Configuration:
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
    }
}
