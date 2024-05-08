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
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
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
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Configuration)] IStorageService storageService,
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
        public override Dictionary<string, ResourceTypeDescriptor> GetResourceTypes() =>
            ConfigurationResourceProviderMetadata.AllowedResourceTypes;

        private ConcurrentDictionary<string, ExternalOrchestrationServiceReference> _externalOrchestrationServiceReferences = [];

        private const string KEY_VAULT_REFERENCE_CONTENT_TYPE = "application/vnd.microsoft.appconfig.keyvaultref+json;charset=utf-8";

        private const string EXTERNAL_ORCHESTRATION_SERVICE_REFERENCES_FILE_NAME = "_external-orchestration-service-references.json";
        private const string EXTERNAL_ORCHESTRATION_SERVICE_REFERENCES_FILE_PATH =
            $"/{ResourceProviderNames.FoundationaLLM_Configuration}/{EXTERNAL_ORCHESTRATION_SERVICE_REFERENCES_FILE_NAME}";

        private readonly IAzureAppConfigurationService _appConfigurationService = appConfigurationService;
        private readonly IAzureKeyVaultService _keyVaultService = keyVaultService;
        private readonly IConfigurationManager _configurationManager = configurationManager;

        /// <inheritdoc/>
        protected override string _name => ResourceProviderNames.FoundationaLLM_Configuration;

        /// <inheritdoc/>
        protected override async Task InitializeInternal()
        {
            _logger.LogInformation("Starting to initialize the {ResourceProvider} resource provider...", _name);

            if (await _storageService.FileExistsAsync(_storageContainerName, EXTERNAL_ORCHESTRATION_SERVICE_REFERENCES_FILE_PATH, default))
            {
                var fileContent = await _storageService.ReadFileAsync(
                    _storageContainerName,
                    EXTERNAL_ORCHESTRATION_SERVICE_REFERENCES_FILE_PATH,
                    default);

                var resourceReferenceStore =
                    JsonSerializer.Deserialize<ResourceReferenceStore<ExternalOrchestrationServiceReference>>(
                        Encoding.UTF8.GetString(fileContent.ToArray()));

                _externalOrchestrationServiceReferences = new ConcurrentDictionary<string, ExternalOrchestrationServiceReference>(
                        resourceReferenceStore!.ToDictionary());
            }
            else
            {
                await _storageService.WriteFileAsync(
                    _storageContainerName,
                    EXTERNAL_ORCHESTRATION_SERVICE_REFERENCES_FILE_PATH,
                    JsonSerializer.Serialize(new ResourceReferenceStore<ExternalOrchestrationServiceReference>
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
                ConfigurationResourceTypeNames.ExternalOrchestrationServices => await LoadExternalOrchestrationServices(resourcePath.ResourceTypeInstances[0]),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

        #region Helpers for GetResourcesAsyncInternal

        private async Task<List<AppConfigurationKeyBase>> LoadAppConfigurationKeys(ResourceTypeInstance instance)
        {
            var keyFilter = instance.ResourceId ?? "FoundationaLLM:*";
            var result = new List<AppConfigurationKeyBase>();

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
                    result.Add(appConfig);
                    continue;
                }

                if (!string.IsNullOrEmpty(setting.ContentType)
                    && setting.ContentType.StartsWith(KEY_VAULT_REFERENCE_CONTENT_TYPE))
                {
                    var kvAppConfig = await TryGetAsKeyVaultReference(setting.Key, setting.Value);
                    if (kvAppConfig != null)
                        appConfig = kvAppConfig;
                }

                result.Add(appConfig);
            }

            return result;
        }

        private async Task<List<ExternalOrchestrationService>> LoadExternalOrchestrationServices(ResourceTypeInstance instance)
        {
            if (instance.ResourceId == null)
            {
                return
                [
                    .. (await Task.WhenAll(
                        _externalOrchestrationServiceReferences.Values
                            .Where(eosr => !eosr.Deleted)
                            .Select(eosr => LoadExternalOrchestrationService(eosr))))
                ];
            }
            else
            {
                if (!_externalOrchestrationServiceReferences.TryGetValue(instance.ResourceId, out var resourceReference)
                    || resourceReference.Deleted)
                    throw new ResourceProviderException($"Could not locate the {instance.ResourceId} external orchestration service resource.",
                        StatusCodes.Status404NotFound);

                var externalOrchestrationService = await LoadExternalOrchestrationService(resourceReference);

                return [externalOrchestrationService];
            }
        }

        private async Task<ExternalOrchestrationService> LoadExternalOrchestrationService(
            ExternalOrchestrationServiceReference resourceReference)
        {
            if (await _storageService.FileExistsAsync(_storageContainerName, resourceReference.Filename, default))
            {
                var fileContent = await _storageService.ReadFileAsync(_storageContainerName, resourceReference.Filename, default);
                return JsonSerializer.Deserialize<ExternalOrchestrationService>(
                    Encoding.UTF8.GetString(fileContent.ToArray()),
                    _serializerSettings)
                    ?? throw new ResourceProviderException($"Failed to load the external orchestration service {resourceReference.Name}.",
                        StatusCodes.Status400BadRequest);
            }

            throw new ResourceProviderException($"Could not locate the {resourceReference.Name} external orchestration service resource.",
                StatusCodes.Status404NotFound);
        }

        #endregion

        /// <inheritdoc/>
        protected override async Task<object> UpsertResourceAsync(ResourcePath resourcePath, string serializedResource, UnifiedUserIdentity userIdentity) =>
            resourcePath.ResourceTypeInstances[0].ResourceType switch
            {
                ConfigurationResourceTypeNames.AppConfigurations => await UpdateAppConfigurationKey(resourcePath, serializedResource),
                _ => throw new ResourceProviderException($"The resource type {resourcePath.ResourceTypeInstances[0].ResourceType} is not supported by the {_name} resource provider.",
                    StatusCodes.Status400BadRequest)
            };

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
