using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Vectorization.Exceptions;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using FoundationaLLM.Vectorization.Models.Resources;
using FoundationaLLM.Vectorization.ResourceProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Vectorization.Handlers
{
    /// <summary>
    /// Handles the partitioning stage of the vectorization pipeline.
    /// </summary>
    /// <param name="messageId">The identifier of underlying message retrieved from the request source.</param>
    /// <param name="parameters">The dictionary of named parameters used to configure the handler.</param>
    /// <param name="stepsConfiguration">The app configuration section containing the configuration for vectorization pipeline steps.</param>
    /// <param name="stateService">The <see cref="IVectorizationStateService"/> that manages vectorization state.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> implemented by the dependency injection container.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers for logging.</param>
    public class PartitionHandler(
        string messageId,
        Dictionary<string, string> parameters,
        IConfigurationSection? stepsConfiguration,
        IVectorizationStateService stateService,
        IServiceProvider serviceProvider,
        ILoggerFactory loggerFactory) : VectorizationStepHandlerBase(
            VectorizationSteps.Partition,
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
            await _stateService.LoadArtifacts(state, VectorizationArtifactType.ExtractedText);

            var extractedTextArtifact = state.Artifacts.SingleOrDefault(a => a.Type == VectorizationArtifactType.ExtractedText
                && a.Position == 1 && !string.IsNullOrWhiteSpace(a.Content));

            if (extractedTextArtifact == null)
            {
                state.Log(this, request.Id!, _messageId, "The extracted text artifact was not found.");
                return false;
            }

            var serviceFactory = _serviceProvider.GetService<IVectorizationServiceFactory<ITextSplitterService>>()
                ?? throw new VectorizationException($"Could not retrieve the text splitter service factory instance.");
            var textSplitter = serviceFactory.GetService(_parameters["text_partition_profile_name"]);

            var splitResult = textSplitter.SplitPlainText(extractedTextArtifact.Content!);

            var position = 0;
            foreach (var textChunk in splitResult.TextChunks)
                state.AddOrReplaceArtifact(new VectorizationArtifact
                {
                    Type = VectorizationArtifactType.TextPartition,
                    Position = ++position,
                    Content = textChunk
                });
            if (!string.IsNullOrWhiteSpace(splitResult.Message))
                state.Log(this, request.Id!, _messageId, splitResult.Message);

            return true;
        }
    }
}
