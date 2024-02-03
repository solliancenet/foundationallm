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
    /// Handles the indexing stage of the vectorization pipeline.
    /// </summary>
    /// <param name="messageId">The identifier of underlying message retrieved from the request source.</param>
    /// <param name="parameters">The dictionary of named parameters used to configure the handler.</param>
    /// <param name="stepsConfiguration">The app configuration section containing the configuration for vectorization pipeline steps.</param>
    /// <param name="stateService">The <see cref="IVectorizationStateService"/> that manages vectorization state.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> implemented by the dependency injection container.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers for logging.</param>
    public class IndexingHandler(
        string messageId,
        Dictionary<string, string> parameters,
        IConfigurationSection? stepsConfiguration,
        IVectorizationStateService stateService,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory) : VectorizationStepHandlerBase(
            VectorizationSteps.Index,
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
            await _stateService.LoadArtifacts(state, VectorizationArtifactType.TextEmbeddingVector);

            var textEmbeddingArtifacts = state.Artifacts.Where(a => a.Type == VectorizationArtifactType.TextEmbeddingVector).ToList();

            if (textEmbeddingArtifacts == null
                || textEmbeddingArtifacts.Count == 0)
            {
                state.Log(this, request.Id!, _messageId, "The text embedding artifacts were not found.");
                return false;
            }

            await _stateService.LoadArtifacts(state, VectorizationArtifactType.TextPartition);
            var textPartitioningArtifacts = state.Artifacts.Where(a => a.Type == VectorizationArtifactType.TextPartition).ToList();

            if (textPartitioningArtifacts == null
                || textPartitioningArtifacts.Count == 0)
            {
                state.Log(this, request.Id!, _messageId, "The text partition artifacts were not found.");
                return false;
            }

            var serializerOptions = new JsonSerializerOptions
            {
                Converters =
                {
                    new Embedding.JsonConverter()
                }
            };

            var embeddedContent = new EmbeddedContent
            {
                ContentId = request.ContentIdentifier,
                ContentParts = Enumerable.Range(0, textEmbeddingArtifacts.Count)
                    .Select(i => new EmbeddedContentPart
                    {
                        Id = $"{request.ContentIdentifier.UniqueId}#{textPartitioningArtifacts[i].Position:D6}",
                        Content = textPartitioningArtifacts[i].Content!,
                        Embedding = JsonSerializer.Deserialize<Embedding>(textEmbeddingArtifacts[i].Content!, serializerOptions)
                    }).ToList()
        };

            var serviceFactory = _serviceProvider.GetService<IVectorizationServiceFactory<IIndexingService>>()
                ?? throw new VectorizationException($"Could not retrieve the indexing service factory instance.");
            var (Service, VectorizationProfile) = serviceFactory.GetServiceWithProfile(_parameters["indexing_profile_name"]);

            var indexingResult = await Service.IndexEmbeddingsAsync(
                embeddedContent,
                VectorizationProfile.Settings!["IndexName"]);

            state.AddOrReplaceIndexReferences(indexingResult);

            return true;
        }
    }
}
