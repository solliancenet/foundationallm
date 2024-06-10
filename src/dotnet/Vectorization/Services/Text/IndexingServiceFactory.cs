using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Vectorization.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Vectorization.Services.Text
{
    /// <summary>
    /// Creates text splitter service instances.
    /// </summary>
    /// <param name="vectorizationResourceProviderService">The vectorization resource provider service.</param>
    /// <param name="configuration">The global configuration provider.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> providing dependency injection services.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers.</param>
    public class IndexingServiceFactory(
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Vectorization)] IResourceProviderService vectorizationResourceProviderService,
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory) : IVectorizationServiceFactory<IIndexingService>
    {
        private readonly IResourceProviderService _vectorizationResourceProviderService = vectorizationResourceProviderService;
        private readonly IConfiguration _configuration = configuration;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;

        /// <inheritdoc/>
        public IIndexingService GetService(string serviceName)
        {
            var indexingProfile = _vectorizationResourceProviderService.GetResource<IndexingProfile>(
                $"/{VectorizationResourceTypeNames.IndexingProfiles}/{serviceName}");

            return indexingProfile.Indexer switch
            {
                IndexerType.AzureAISearchIndexer => CreateAzureAISearchIndexingService(),
                _ => throw new VectorizationException($"The text embedding type {indexingProfile.Indexer} is not supported."),
            };
        }

        /// <inheritdoc/>
        public (IIndexingService Service, ResourceBase Resource) GetServiceWithResource(string serviceName)
        {
            var indexingProfile = _vectorizationResourceProviderService.GetResource<IndexingProfile>(
                $"/{VectorizationResourceTypeNames.IndexingProfiles}/{serviceName}");

            return indexingProfile.Indexer switch
            {
                IndexerType.AzureAISearchIndexer => (CreateAzureAISearchIndexingService(), indexingProfile),
                IndexerType.AzureCosmosDBNoSQLIndexer => (CreateAzureCosmosDBNoSQLIndexingService(), indexingProfile),
                IndexerType.PostgresIndexer => (CreatePostgresIndexingService(), indexingProfile),
                _ => throw new VectorizationException($"The text embedding type {indexingProfile.Indexer} is not supported."),
            };
        }

        private IIndexingService CreateAzureAISearchIndexingService()
        {
            var indexingService = _serviceProvider.GetKeyedService<IIndexingService>(
                DependencyInjectionKeys.FoundationaLLM_Vectorization_AzureAISearchIndexingService)
                ?? throw new VectorizationException($"Could not retrieve the Azure AI Search indexing service instance.");

            return indexingService!;
        }

        private IIndexingService CreateAzureCosmosDBNoSQLIndexingService()
        {
            var indexingService = _serviceProvider.GetKeyedService<IIndexingService>(
                DependencyInjectionKeys.FoundationaLLM_Vectorization_AzureCosmosDBNoSQLIndexingService)
                ?? throw new VectorizationException($"Could not retrieve the Azure Cosmos DB NoSQL indexing service instance.");

            return indexingService!;
        }

        private IIndexingService CreatePostgresIndexingService()
        {
            var indexingService = _serviceProvider.GetKeyedService<IIndexingService>(
                DependencyInjectionKeys.FoundationaLLM_Vectorization_PostgresIndexingService)
                ?? throw new VectorizationException($"Could not retrieve the PostgreSQL indexing service instance.");

            return indexingService!;
        }
    }
}
