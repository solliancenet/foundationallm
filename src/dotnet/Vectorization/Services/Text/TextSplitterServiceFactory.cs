using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Text;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Services.TextSplitters;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.ResourceProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Vectorization.Services.Text
{
    /// <summary>
    /// Creates text splitter service instances.
    /// </summary>
    /// <param name="resourceProviderServices">The collection of registered resource providers.</param>
    /// <param name="configuration">The global configuration provider.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> providing dependency injection services.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers.</param>
    public class TextSplitterServiceFactory(        
        IEnumerable<IResourceProviderService> resourceProviderServices,
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory) : IVectorizationServiceFactory<ITextSplitterService>
    {       
        private readonly IConfiguration _configuration = configuration;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;

        private readonly Dictionary<string, IResourceProviderService> _resourceProviderServices =
            resourceProviderServices.ToDictionary<IResourceProviderService, string>(
                rps => rps.Name);

        /// <inheritdoc/>
        public ITextSplitterService GetService(string serviceName)
        {
            _resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Vectorization, out var vectorizationResourceProviderService);
            if (vectorizationResourceProviderService == null)
                throw new VectorizationException($"The resource provider {ResourceProviderNames.FoundationaLLM_DataSource} was not loaded.");

            var textPartitionProfile = vectorizationResourceProviderService.GetResource<TextPartitioningProfile>(
                $"/{VectorizationResourceTypeNames.TextPartitioningProfiles}/{serviceName}");

            return textPartitionProfile.TextSplitter switch
            {
                TextSplitterType.TokenTextSplitter => CreateTokenTextSplitterService(
                    TokenTextSplitterServiceSettings.FromDictionary(textPartitionProfile.Settings!)),
                _ => throw new VectorizationException($"The text splitter type {textPartitionProfile.TextSplitter} is not supported."),
            };
        }

        /// <inheritdoc/>
        public (ITextSplitterService Service, ResourceBase Resource) GetServiceWithResource(string serviceName)
        {
            _resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Vectorization, out var vectorizationResourceProviderService);
            if (vectorizationResourceProviderService == null)
                throw new VectorizationException($"The resource provider {ResourceProviderNames.FoundationaLLM_DataSource} was not loaded.");

            var textPartitionProfile = vectorizationResourceProviderService.GetResource<TextPartitioningProfile>(
                $"/{VectorizationResourceTypeNames.TextPartitioningProfiles}/{serviceName}");

            return textPartitionProfile.TextSplitter switch
            {
                TextSplitterType.TokenTextSplitter => (CreateTokenTextSplitterService(
                    TokenTextSplitterServiceSettings.FromDictionary(textPartitionProfile.Settings!)), textPartitionProfile),
                _ => throw new VectorizationException($"The text splitter type {textPartitionProfile.TextSplitter} is not supported."),
            };
        }

        private TokenTextSplitterService CreateTokenTextSplitterService(TokenTextSplitterServiceSettings settings)
        {
            var tokenizerService = _serviceProvider.GetKeyedService<ITokenizerService>(settings.Tokenizer)
                ?? throw new VectorizationException($"Could not retrieve the {settings.Tokenizer} tokenizer service instance.");

            return new TokenTextSplitterService(
                tokenizerService,
                Options.Create<TokenTextSplitterServiceSettings>(settings),
                _loggerFactory.CreateLogger<TokenTextSplitterService>());
        }
    }
}
