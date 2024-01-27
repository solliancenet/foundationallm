using Azure.Identity;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.TextEmbedding;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.SemanticKernel.Core.Models.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using System.ComponentModel;

#pragma warning disable SKEXP0001, SKEXP0011

namespace FoundationaLLM.SemanticKernel.Core.Services
{
    /// <summary>
    /// Generates text embeddings using the Semantic Kernel orchestrator.
    /// </summary>
    public class SemanticKernelTextEmbeddingService : ITextEmbeddingService
    {
        private readonly SemanticKernelTextEmbeddingServiceSettings _settings;
        private readonly ILogger _logger;
        private readonly Kernel _kernel;
        private readonly ITextEmbeddingGenerationService _textEmbeddingService;

        /// <summary>
        /// Creates a new <see cref="SemanticKernelTextEmbeddingService"/> instance.
        /// </summary>
        /// <param name="options">The <see cref="IOptions{TOptions}"/> providing configuration settings.</param>
        /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
        public SemanticKernelTextEmbeddingService(
            IOptions<SemanticKernelTextEmbeddingServiceSettings> options,
            ILogger<SemanticKernelTextEmbeddingService> logger)
        {
            _settings = options.Value;
            _logger = logger;
            _kernel = CreateKernel();
            _textEmbeddingService = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
        }

        /// <inheritdoc/>
        public async Task<(Embedding Embedding, int TokenCount)> GetEmbeddingAsync(string text)
        {
            var embedding = await _textEmbeddingService.GenerateEmbeddingAsync(text);
            return new(new(embedding), 0);
        }

        /// <inheritdoc/>
        public async Task<(IList<Embedding> Embeddings, int TokenCount)> GetEmbeddingsAsync(IList<string> texts)
        {
            var embeddings = await _textEmbeddingService.GenerateEmbeddingsAsync(texts);
            return new(embeddings.Select(e => new Embedding(e)).ToList(), 0);
        }

        /// <summary>
        /// Creates a <see cref="Kernel"/> instance using the deployment name, endpoint, and API key.
        /// </summary>
        /// <param name="deploymentName">The name of the Azure Open AI deployment.</param>
        /// <param name="endpoint">The endpoint of the Azure Open AI deployment.</param>
        /// <param name="apiKey">The API key used to connect to the Azure Open AI deployment.</param>
        /// <returns>The <see cref="Kernel"/> instance.</returns>
        private Kernel CreateKernelFromAPIKey(string deploymentName, string endpoint, string apiKey)
        {
            var builder = Kernel.CreateBuilder();
            builder.AddAzureOpenAITextEmbeddingGeneration(deploymentName, endpoint, apiKey);
            return builder.Build();
        }

        /// <summary>
        /// Creates a <see cref="Kernel"/> instance using the deployment name, endpoint, and the Azure identity.
        /// </summary>
        /// <param name="deploymentName">The name of the Azure Open AI deployment.</param>
        /// <param name="endpoint">The endpoint of the Azure Open AI deployment.</param>
        /// <returns>The <see cref="Kernel"/> instance.</returns>
        private Kernel CreateKernelFromIdentity(string deploymentName, string endpoint)
        {
            var builder = Kernel.CreateBuilder();
            builder.AddAzureOpenAITextEmbeddingGeneration(deploymentName, endpoint, new DefaultAzureCredential());
            return builder.Build();
        }

        private void ValidateDeploymentName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _logger.LogCritical("The Azure Open AI deployment name is invalid.");
                throw new ConfigurationValueException("The Azure Open AI deployment name is invalid.");
            }
        }

        private void ValidateEndpoint(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _logger.LogCritical("The Azure Open AI endpoint is invalid.");
                throw new ConfigurationValueException("The Azure Open AI endpoint is invalid.");
            }
        }
        private void ValidateAPIKey(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _logger.LogCritical("The Azure Open AI API key is invalid.");
                throw new ConfigurationValueException("The Azure Open AI API key is invalid.");
            }
        }

        private Kernel CreateKernel()
        {
            switch (_settings.AuthenticationType)
            {
                case AzureOpenAIAuthenticationTypes.APIKey:
                    ValidateDeploymentName(_settings.DeploymentName);
                    ValidateEndpoint(_settings.Endpoint);
                    ValidateAPIKey(_settings.APIKey);
                    return CreateKernelFromAPIKey(_settings.DeploymentName, _settings.Endpoint, _settings.APIKey!);
                case AzureOpenAIAuthenticationTypes.AzureIdentity:
                    ValidateDeploymentName(_settings.DeploymentName);
                    ValidateEndpoint(_settings.Endpoint);
                    return CreateKernelFromIdentity(_settings.DeploymentName, _settings.Endpoint);
                default:
                    throw new InvalidEnumArgumentException($"The authentication type {_settings.AuthenticationType} is not supported.");
            }
        }
    }
}
