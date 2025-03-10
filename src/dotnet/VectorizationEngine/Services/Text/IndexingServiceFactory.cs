using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.SemanticKernel.Core.Models.Configuration;
using FoundationaLLM.SemanticKernel.Core.Services.Indexing;
using FoundationaLLM.Vectorization.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Vectorization.Services.Text
{
    /// <summary>
    /// Creates text splitter service instances.
    /// </summary>
    /// <param name="resourceProviderServices">The collection of registered resource providers.</param>
    /// <param name="configuration">The global configuration provider.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> providing dependency injection services.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers.</param>
    public class IndexingServiceFactory(
        IEnumerable<IResourceProviderService> resourceProviderServices,
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory) : IVectorizationServiceFactory<IIndexingService>
    {        
        private readonly IConfiguration _configuration = configuration;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;

        private readonly Dictionary<string, IResourceProviderService> _resourceProviderServices =
            resourceProviderServices.ToDictionary<IResourceProviderService, string>(
                rps => rps.Name);

        /// <inheritdoc/>
        public async Task<IIndexingService> GetService(string serviceName, UnifiedUserIdentity userIdentity)
        {
            _resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Vectorization, out var vectorizationResourceProviderService);
            if (vectorizationResourceProviderService == null)
                throw new VectorizationException($"The resource provider {ResourceProviderNames.FoundationaLLM_DataSource} was not loaded.");

            var indexingProfile = await vectorizationResourceProviderService.GetResourceAsync<IndexingProfile>(
                $"/{VectorizationResourceTypeNames.IndexingProfiles}/{serviceName}", userIdentity);

            return indexingProfile.Indexer switch
            {
                IndexerType.AzureAISearchIndexer => await CreateAzureAISearchIndexingService(indexingProfile),
                _ => throw new VectorizationException($"The text embedding type {indexingProfile.Indexer} is not supported."),
            };
        }

        /// <inheritdoc/>
        public async Task<(IIndexingService Service, ResourceBase Resource)> GetServiceWithResource(string serviceName, UnifiedUserIdentity userIdentity)
        {
            _resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Vectorization, out var vectorizationResourceProviderService);
            if (vectorizationResourceProviderService == null)
                throw new VectorizationException($"The resource provider {ResourceProviderNames.FoundationaLLM_Vectorization} was not loaded.");

            var indexingProfile = await vectorizationResourceProviderService.GetResourceAsync<IndexingProfile>(
                $"/{VectorizationResourceTypeNames.IndexingProfiles}/{serviceName}", userIdentity);

            return indexingProfile.Indexer switch
            {
                IndexerType.AzureAISearchIndexer => (await CreateAzureAISearchIndexingService(indexingProfile), indexingProfile),
                IndexerType.AzureCosmosDBNoSQLIndexer => (CreateAzureCosmosDBNoSQLIndexingService(), indexingProfile),
                IndexerType.PostgresIndexer => (CreatePostgresIndexingService(), indexingProfile),
                _ => throw new VectorizationException($"The text embedding type {indexingProfile.Indexer} is not supported."),
            };
        }

        private async Task<IIndexingService> CreateAzureAISearchIndexingService(IndexingProfile profile)
        {
            // Get the API Endpoint configuration for the Azure AI Search service from the profile.
            _resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Configuration, out var configurationResourceProviderService);
            if (configurationResourceProviderService == null)
                throw new VectorizationException($"The resource provider {ResourceProviderNames.FoundationaLLM_Configuration} was not loaded.");

            if (profile.Settings==null)
                throw new VectorizationException($"The settings for the Azure AI Search service were not found.");

            if (profile.Settings.TryGetValue("api_endpoint_configuration_object_id", out var apiEndpointObjectId) == false)
                throw new VectorizationException($"The API endpoint configuration object ID was not found in the settings.");
            
            var apiEndpoint = await configurationResourceProviderService.GetResourceAsync<APIEndpointConfiguration>(apiEndpointObjectId, ServiceContext.ServiceIdentity!);
            if(apiEndpoint==null)
                throw new VectorizationException($"The API endpoint configuration {apiEndpointObjectId} for the Azure AI Search service was not found.");

            var settings = new AzureAISearchIndexingServiceSettings()
                {
                    AuthenticationType = apiEndpoint.AuthenticationType,
                    Endpoint = apiEndpoint.Url
            };
            return new AzureAISearchIndexingService(settings, _loggerFactory.CreateLogger<AzureAISearchIndexingService>());
        }

        private IIndexingService CreateAzureCosmosDBNoSQLIndexingService()
        {
            var indexingService = _serviceProvider.GetKeyedService<IIndexingService>(
                DependencyInjectionKeys.FoundationaLLM_APIEndpoints_AzureCosmosDBNoSQLVectorStore_Configuration)
                ?? throw new VectorizationException($"Could not retrieve the Azure Cosmos DB NoSQL indexing service instance.");

            return indexingService!;
        }

        private IIndexingService CreatePostgresIndexingService()
        {
            var indexingService = _serviceProvider.GetKeyedService<IIndexingService>(
                DependencyInjectionKeys.FoundationaLLM_APIEndpoints_AzurePostgreSQLVectorStore_Configuration)
                ?? throw new VectorizationException($"Could not retrieve the PostgreSQL indexing service instance.");

            return indexingService!;
        }
    }
}
