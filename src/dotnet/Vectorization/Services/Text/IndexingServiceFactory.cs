using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Vectorization.Exceptions;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models.Resources;
using FoundationaLLM.Vectorization.ResourceProviders;
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
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_Vectorization_ResourceProviderService)] IResourceProviderService vectorizationResourceProviderService,
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
        public (IIndexingService Service, VectorizationProfileBase VectorizationProfile) GetServiceWithProfile(string serviceName)
        {
            var indexingProfile = _vectorizationResourceProviderService.GetResource<IndexingProfile>(
                $"/{VectorizationResourceTypeNames.IndexingProfiles}/{serviceName}");

            return indexingProfile.Indexer switch
            {
                IndexerType.AzureAISearchIndexer => (CreateAzureAISearchIndexingService(), indexingProfile),
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
    }
}
