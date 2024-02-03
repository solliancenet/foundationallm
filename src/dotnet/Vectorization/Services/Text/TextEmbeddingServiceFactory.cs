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
    public class TextEmbeddingServiceFactory(
        [FromKeyedServices(DependencyInjectionKeys.FoundationaLLM_Vectorization_ResourceProviderService)] IResourceProviderService vectorizationResourceProviderService,
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory) : IVectorizationServiceFactory<ITextEmbeddingService>
    {
        private readonly IResourceProviderService _vectorizationResourceProviderService = vectorizationResourceProviderService;
        private readonly IConfiguration _configuration = configuration;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;

        /// <inheritdoc/>
        public ITextEmbeddingService GetService(string serviceName)
        {
            var textEmbeddingProfile = _vectorizationResourceProviderService.GetResource<TextEmbeddingProfile>(
                $"/{VectorizationResourceTypeNames.TextEmbeddingProfiles}/{serviceName}");

            return textEmbeddingProfile.TextEmbedding switch
            {
                TextEmbeddingType.SemanticKernelTextEmbedding => CreateSemanticKernelTextEmbeddingService(),
                _ => throw new VectorizationException($"The text embedding type {textEmbeddingProfile.TextEmbedding} is not supported."),
            };
        }

        /// <inheritdoc/>
        public (ITextEmbeddingService Service, VectorizationProfileBase VectorizationProfile) GetServiceWithProfile(string serviceName)
        {
            var textEmbeddingProfile = _vectorizationResourceProviderService.GetResource<TextEmbeddingProfile>(
                $"/{VectorizationResourceTypeNames.TextEmbeddingProfiles}/{serviceName}");

            return textEmbeddingProfile.TextEmbedding switch
            {
                TextEmbeddingType.SemanticKernelTextEmbedding => (CreateSemanticKernelTextEmbeddingService(), textEmbeddingProfile),
                _ => throw new VectorizationException($"The text embedding type {textEmbeddingProfile.TextEmbedding} is not supported."),
            };
        }

        private ITextEmbeddingService CreateSemanticKernelTextEmbeddingService()
        {
            var textEmbeddingService = _serviceProvider.GetKeyedService<ITextEmbeddingService>(
                DependencyInjectionKeys.FoundationaLLM_Vectorization_SemanticKernelTextEmbeddingService)
                ?? throw new VectorizationException($"Could not retrieve the Semantic Kernel text embedding service instance.");

            return textEmbeddingService!;
        }
    }
}
