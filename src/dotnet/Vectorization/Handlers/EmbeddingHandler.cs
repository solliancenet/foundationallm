using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;

namespace FoundationaLLM.Vectorization.Handlers
{
    /// <summary>
    /// Handles the embedding stage of the vectorization pipeline.
    /// </summary>
    /// <param name="instanceId">The FoundationaLLM instance id.</param>
    /// <param name="messageId">The identifier of underlying message retrieved from the request source.</param>
    /// <param name="parameters">The dictionary of named parameters used to configure the handler.</param>
    /// <param name="stepsConfiguration">The app configuration section containing the configuration for vectorization pipeline steps.</param>
    /// <param name="stateService">The <see cref="IVectorizationStateService"/> that manages vectorization state.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> implemented by the dependency injection container.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers for logging.</param>
    public class EmbeddingHandler(
        string instanceId,
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
            var serviceFactory = _serviceProvider.GetService<IVectorizationServiceFactory<ITextEmbeddingService>>()
                ?? throw new VectorizationException($"Could not retrieve the text embedding service factory instance.");
            var (textEmbeddingService, textEmbeddingProfileResourceBase) = serviceFactory.GetServiceWithResource(_parameters["text_embedding_profile_name"]);
            var textEmbeddingProfile = textEmbeddingProfileResourceBase as TextEmbeddingProfile;
            var embeddingModelName = textEmbeddingProfile!.Settings?.TryGetValue("model_name", out var modelName) == true ? modelName : null;

            var embeddingResult = default(TextEmbeddingResult);

            if (request.RunningOperations.TryGetValue(_stepId, out var runningOperation))
            {
                // We have an ongoing operation, so we need to attempt to retrieve the emebdding results

                embeddingResult = await textEmbeddingService.GetEmbeddingsAsync(instanceId, runningOperation.OperationId);

                runningOperation.LastResponseTime = DateTime.UtcNow;
                runningOperation.PollingCount++;

                if (embeddingResult.InProgress)
                {
                    // The operation is still in progress
                    return false;
                } else if (embeddingResult.Failed)
                {
                    throw new VectorizationException($"The following error occured during the text embedding operation with id {runningOperation.OperationId}: {embeddingResult.ErrorMessage ?? "N/A"}");
                }
                else
                {
                    // The operation completed
                    runningOperation.Complete = true;
                }
            }
            else
            {
                // We don't have an ongoing operation, so we need to start the embedding operation

                await _stateService.LoadArtifacts(state, VectorizationArtifactType.TextPartition);

                var textPartitioningArtifacts = state.Artifacts.Where(a => a.Type == VectorizationArtifactType.TextPartition).ToList();

                if (textPartitioningArtifacts == null
                    || textPartitioningArtifacts.Count == 0)
                {
                    state.Log(this, request.Name!, _messageId, "The text partition artifacts were not found.");
                    return false;
                }

                embeddingResult = await textEmbeddingService.GetEmbeddingsAsync(
                    instanceId,
                    textPartitioningArtifacts.Select(tpa => new TextChunk
                    {
                        Position = tpa.Position,
                        Content = tpa.Content!,
                        TokensCount = tpa.Size
                    }).ToList(),
                    embeddingModelName);

                if (embeddingResult.InProgress)
                {
                    var now = DateTime.UtcNow;

                    // The operation is a long-running operation, so we need to persist the operation id and
                    // wait for the next opportunity to attempt to retrieve the operation results.
                    request.RunningOperations[_stepId] = new VectorizationLongRunningOperation
                    {
                        OperationId = embeddingResult.OperationId!,
                        FirstResponseTime = now,
                        LastResponseTime = now,
                        Complete = false,
                        PollingCount = 0
                    };
                    return false;
                }
                else if (embeddingResult.Failed)
                {
                    throw new VectorizationException($"The following error occured during the text embedding operation with id {embeddingResult.OperationId}: {embeddingResult.ErrorMessage ?? "N/A"}");
                }
            }

            var serializerOptions = new JsonSerializerOptions { Converters = { new Embedding.JsonConverter() } };
            foreach (var textChunk in embeddingResult!.TextChunks)
                state.AddOrReplaceArtifact(new VectorizationArtifact
                {
                    Type = VectorizationArtifactType.TextEmbeddingVector,
                    Position = textChunk.Position,
                    Content = JsonSerializer.Serialize(textChunk.Embedding!, serializerOptions),
                    Size = textChunk.Embedding!.Value.Length
                });

            return true;
        }
    }
}
