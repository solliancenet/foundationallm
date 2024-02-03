using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.TextEmbedding;
using FoundationaLLM.Vectorization.Exceptions;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FoundationaLLM.Vectorization.Handlers
{
    /// <summary>
    /// Handles the embedding stage of the vectorization pipeline.
    /// </summary>
    /// <param name="messageId">The identifier of underlying message retrieved from the request source.</param>
    /// <param name="parameters">The dictionary of named parameters used to configure the handler.</param>
    /// <param name="stepsConfiguration">The app configuration section containing the configuration for vectorization pipeline steps.</param>
    /// <param name="stateService">The <see cref="IVectorizationStateService"/> that manages vectorization state.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> implemented by the dependency injection container.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers for logging.</param>
    public class EmbeddingHandler(
        string messageId,
        Dictionary<string, string> parameters,
        IConfigurationSection? stepsConfiguration,
        IVectorizationStateService stateService,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory) : VectorizationStepHandlerBase(
            VectorizationSteps.Embed,
            messageId,
            parameters,
            stepsConfiguration,
            stateService,
            serviceProvider,
            loggerFactory)
    {
        /// <inheritdoc/>
        protected override async Task<bool> ProcessRequest(
            VectorizationRequest request,
            VectorizationState state,
            IConfigurationSection? stepConfiguration,
            CancellationToken cancellationToken)
        {
            await _stateService.LoadArtifacts(state, VectorizationArtifactType.TextPartition);

            var textPartitioningArtifacts = state.Artifacts.Where(a => a.Type == VectorizationArtifactType.TextPartition).ToList();

            if (textPartitioningArtifacts == null
                || textPartitioningArtifacts.Count == 0)
            {
                state.Log(this, request.Id!, _messageId, "The text partition artifacts were not found.");
                return false;
            }

            var serviceFactory = _serviceProvider.GetService<IVectorizationServiceFactory<ITextEmbeddingService>>()
                ?? throw new VectorizationException($"Could not retrieve the text embedding service factory instance.");
            var textEmbedding = serviceFactory.GetService(_parameters["text_embedding_profile_name"]);

            var embeddingResult = await textEmbedding.GetEmbeddingsAsync(
                textPartitioningArtifacts.Select(tpa => tpa.Content!).ToList());

            var position = 0;
            var serializerOptions = new JsonSerializerOptions
            {
                Converters =
                {
                    new Embedding.JsonConverter()
                }
            };

            foreach (var embedding in embeddingResult.Embeddings)
                state.AddOrReplaceArtifact(new VectorizationArtifact
                {
                    Type = VectorizationArtifactType.TextEmbeddingVector,
                    Position = ++position,
                    Content = JsonSerializer.Serialize(embedding, serializerOptions)
                });

            return true;
        }
    }
}
