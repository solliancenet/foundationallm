using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.SemanticKernel.Core.Models.Configuration;
using FoundationaLLM.SemanticKernel.Core.Services.Indexing;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.ResourceProviders;
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
        public IIndexingService GetService(string serviceName)
        {
            _resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Vectorization, out var vectorizationResourceProviderService);
            if (vectorizationResourceProviderService == null)
                throw new VectorizationException($"The resource provider {ResourceProviderNames.FoundationaLLM_DataSource} was not loaded.");

            var indexingProfile = vectorizationResourceProviderService.GetResource<IndexingProfile>(
                $"/{VectorizationResourceTypeNames.IndexingProfiles}/{serviceName}");

            return indexingProfile.Indexer switch
            {
                IndexerType.AzureAISearchIndexer => CreateAzureAISearchIndexingService(indexingProfile),
                _ => throw new VectorizationException($"The text embedding type {indexingProfile.Indexer} is not supported."),
            };
        }

        /// <inheritdoc/>
        public (IIndexingService Service, ResourceBase Resource) GetServiceWithResource(string serviceName)
        {
            _resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Vectorization, out var vectorizationResourceProviderService);
            if (vectorizationResourceProviderService == null)
                throw new VectorizationException($"The resource provider {ResourceProviderNames.FoundationaLLM_Vectorization} was not loaded.");

            var indexingProfile = vectorizationResourceProviderService.GetResource<IndexingProfile>(
                $"/{VectorizationResourceTypeNames.IndexingProfiles}/{serviceName}");

            return indexingProfile.Indexer switch
            {
                IndexerType.AzureAISearchIndexer => (CreateAzureAISearchIndexingService(indexingProfile), indexingProfile),
                IndexerType.AzureCosmosDBNoSQLIndexer => (CreateAzureCosmosDBNoSQLIndexingService(), indexingProfile),
                IndexerType.PostgresIndexer => (CreatePostgresIndexingService(), indexingProfile),
                _ => throw new VectorizationException($"The text embedding type {indexingProfile.Indexer} is not supported."),
            };
        }

        private IIndexingService CreateAzureAISearchIndexingService(IndexingProfile profile)
        {
            // Get the API Endpoint configuration for the Azure AI Search service from the profile.
            _resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Configuration, out var configurationResourceProviderService);
            if (configurationResourceProviderService == null)
                throw new VectorizationException($"The resource provider {ResourceProviderNames.FoundationaLLM_Configuration} was not loaded.");

            var apiEndpoint = configurationResourceProviderService.GetResource<APIEndpointConfiguration>(profile.IndexingAPIEndpointConfigurationObjectId);
            if(apiEndpoint==null)
                throw new VectorizationException($"The API endpoint configuration {profile.IndexingAPIEndpointConfigurationObjectId} for the Azure AI Search service was not found.");

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
