using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.SemanticKernel.Core.Models.Configuration;
using FoundationaLLM.SemanticKernel.Core.Services;
using FoundationaLLM.Vectorization.Interfaces;
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
    public class TextEmbeddingServiceFactory(
        IEnumerable<IResourceProviderService> resourceProviderServices,
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory) : IVectorizationServiceFactory<ITextEmbeddingService>
    {       
        private readonly IConfiguration _configuration = configuration;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;

        private readonly Dictionary<string, IResourceProviderService> _resourceProviderServices =
            resourceProviderServices.ToDictionary<IResourceProviderService, string>(
                rps => rps.Name);

        /// <inheritdoc/>
        public ITextEmbeddingService GetService(string serviceName)
        {
            _resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Vectorization, out var vectorizationResourceProviderService);
            if (vectorizationResourceProviderService == null)
                throw new VectorizationException($"The resource provider {ResourceProviderNames.FoundationaLLM_DataSource} was not loaded.");

            var textEmbeddingProfile = vectorizationResourceProviderService.GetResource<TextEmbeddingProfile>(
                $"/{VectorizationResourceTypeNames.TextEmbeddingProfiles}/{serviceName}");

            return textEmbeddingProfile.TextEmbedding switch
            {
                TextEmbeddingType.SemanticKernelTextEmbedding => CreateSemanticKernelTextEmbeddingService(textEmbeddingProfile),
                TextEmbeddingType.GatewayTextEmbedding => CreateGatewayTextEmbeddingService(),
                _ => throw new VectorizationException($"The text embedding type {textEmbeddingProfile.TextEmbedding} is not supported."),
            };
        }

        /// <inheritdoc/>
        public (ITextEmbeddingService Service, ResourceBase Resource) GetServiceWithResource(string serviceName)
        {
            _resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Vectorization, out var vectorizationResourceProviderService);
            if (vectorizationResourceProviderService == null)
                throw new VectorizationException($"The resource provider {ResourceProviderNames.FoundationaLLM_DataSource} was not loaded.");

            var textEmbeddingProfile = vectorizationResourceProviderService.GetResource<TextEmbeddingProfile>(
                $"/{VectorizationResourceTypeNames.TextEmbeddingProfiles}/{serviceName}");

            return textEmbeddingProfile.TextEmbedding switch
            {
                TextEmbeddingType.SemanticKernelTextEmbedding => (CreateSemanticKernelTextEmbeddingService(textEmbeddingProfile), textEmbeddingProfile),
                TextEmbeddingType.GatewayTextEmbedding => (CreateGatewayTextEmbeddingService(), textEmbeddingProfile),
                _ => throw new VectorizationException($"The text embedding type {textEmbeddingProfile.TextEmbedding} is not supported."),
            };
        }

        private ITextEmbeddingService CreateSemanticKernelTextEmbeddingService(TextEmbeddingProfile textEmbeddingProfile)
        {
            if (!textEmbeddingProfile.ConfigurationReferences!.TryGetValue("Endpoint", out string? endpointConfigurationItem)
                || string.IsNullOrWhiteSpace(endpointConfigurationItem))
                throw new VectorizationException("The text embedding profile does not contain a valid Endpoint configuration reference.");

            if (!textEmbeddingProfile.ConfigurationReferences!.TryGetValue("DeploymentName", out string? deploymentNameConfigurationItem)
                || string.IsNullOrWhiteSpace(deploymentNameConfigurationItem))
                throw new VectorizationException("The text embedding profile does not contain a valid DeploymentName configuration reference.");

            var deploymentName = 
                (textEmbeddingProfile.Settings!.TryGetValue("deployment_name", out string? deploymentNameOverride)
                || !string.IsNullOrWhiteSpace(deploymentNameOverride))
                ? deploymentNameOverride
                : _configuration[deploymentNameConfigurationItem];

            return new SemanticKernelTextEmbeddingService(
                Options.Create<SemanticKernelTextEmbeddingServiceSettings>(new SemanticKernelTextEmbeddingServiceSettings
                {
                    AuthenticationType = AzureOpenAIAuthenticationTypes.AzureIdentity,
                    Endpoint = _configuration[endpointConfigurationItem]!,
                    DeploymentName = deploymentName!
                }),
                _loggerFactory);
        }

        private ITextEmbeddingService CreateGatewayTextEmbeddingService()
        {
            using var scope = _serviceProvider.CreateScope();
            var textEmbeddingService = scope.ServiceProvider.GetKeyedService<ITextEmbeddingService>(
                DependencyInjectionKeys.FoundationaLLM_Vectorization_GatewayTextEmbeddingService)
                ?? throw new VectorizationException($"Could not retrieve the Gateway text embedding service instance.");

            return textEmbeddingService!;
        }
    }
}
