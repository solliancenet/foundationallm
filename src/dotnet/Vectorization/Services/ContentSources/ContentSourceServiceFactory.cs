using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Vectorization.Exceptions;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models.Configuration;
using FoundationaLLM.Vectorization.Models.Resources;
using FoundationaLLM.Vectorization.ResourceProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Vectorization.Services.ContentSources
{
    /// <summary>
    /// Manages content sources registered for use in the vectorization pipelines.
    /// </summary>
    /// <remarks>
    /// Creates a new instance of the content source manager service.
    /// </remarks>
    /// <param name="vectorizationResourceProviderService">The vectorization resource provider service.</param>
    /// <param name="configuration">The global configuration provider.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers.</param>
    public class ContentSourceServiceFactory(
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_Vectorization_ResourceProviderService)] IResourceProviderService vectorizationResourceProviderService,
        IConfiguration configuration,
        ILoggerFactory loggerFactory) : IVectorizationServiceFactory<IContentSourceService>
    {
        private readonly IResourceProviderService _vectorizationResourceProviderService = vectorizationResourceProviderService;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;

        /// <inheritdoc/>
        public IContentSourceService GetService(string serviceName)
        {
            var contentSourceProfile = _vectorizationResourceProviderService.GetResource<ContentSourceProfile>(
                $"/{VectorizationResourceTypeNames.ContentSourceProfiles}/{serviceName}");

            return contentSourceProfile.Type switch
            {
                ContentSourceType.AzureDataLake => CreateAzureDataLakeContentSourceService(serviceName),
                ContentSourceType.SharePointOnline => CreateSharePointOnlineContentSourceService(serviceName),
                ContentSourceType.AzureSQLDatabase => CreateAzureSQLDatabaseContentSourceService(serviceName),
                _ => throw new VectorizationException($"The content source type {contentSourceProfile.Type} is not supported."),
            };
        }

        /// <inheritdoc/>
        public (IContentSourceService Service, VectorizationProfileBase VectorizationProfile) GetServiceWithProfile(string serviceName)
        {
            var contentSourceProfile = _vectorizationResourceProviderService.GetResource<ContentSourceProfile>(
                $"/{VectorizationResourceTypeNames.ContentSourceProfiles}/{serviceName}");

            return contentSourceProfile.Type switch
            {
                ContentSourceType.AzureDataLake => (CreateAzureDataLakeContentSourceService(serviceName), contentSourceProfile),
                ContentSourceType.SharePointOnline => (CreateSharePointOnlineContentSourceService(serviceName), contentSourceProfile),
                ContentSourceType.AzureSQLDatabase => (CreateAzureSQLDatabaseContentSourceService(serviceName), contentSourceProfile),
                _ => throw new VectorizationException($"The content source type {contentSourceProfile.Type} is not supported."),
            };
        }


        private DataLakeContentSourceService CreateAzureDataLakeContentSourceService(string serviceName)
        {
            var blobStorageServiceSettings = new BlobStorageServiceSettings { AuthenticationType = BlobStorageAuthenticationTypes.Unknown };
            _configuration.Bind(
                $"{AppConfigurationKeySections.FoundationaLLM_Vectorization_ContentSources}:{serviceName}",
                blobStorageServiceSettings);

            return new DataLakeContentSourceService(
                blobStorageServiceSettings,
                _loggerFactory);
        }

        private SharePointOnlineContentSourceService CreateSharePointOnlineContentSourceService(string serviceName)
        {
            var sharePointOnlineContentSourceServiceSettings = new SharePointOnlineContentSourceServiceSettings();
            _configuration.Bind(
                $"{AppConfigurationKeySections.FoundationaLLM_Vectorization_ContentSources}:{serviceName}",
                sharePointOnlineContentSourceServiceSettings);

            return new SharePointOnlineContentSourceService(
                sharePointOnlineContentSourceServiceSettings,
                _loggerFactory);
        }

        private AzureSQLDatabaseContentSourceService CreateAzureSQLDatabaseContentSourceService(string serviceName)
        {
            var azureSQLDatabaseContentSourceServiceSettings = new AzureSQLDatabaseContentSourceServiceSettings();
            _configuration.Bind(
                $"{AppConfigurationKeySections.FoundationaLLM_Vectorization_ContentSources}:{serviceName}",
                azureSQLDatabaseContentSourceServiceSettings);

            return new AzureSQLDatabaseContentSourceService(
                azureSQLDatabaseContentSourceServiceSettings,
                _loggerFactory);
        }
    }
}
