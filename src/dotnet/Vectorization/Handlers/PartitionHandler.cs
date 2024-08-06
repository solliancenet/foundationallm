using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;

namespace FoundationaLLM.Vectorization.Handlers
{
    /// <summary>
    /// Handles the partitioning stage of the vectorization pipeline.
    /// </summary>
    /// <param name="instanceId">The FoundationaLLM instance id.</param>
    /// <param name="messageId">The identifier of underlying message retrieved from the request source.</param>
    /// <param name="parameters">The dictionary of named parameters used to configure the handler.</param>
    /// <param name="stepsConfiguration">The app configuration section containing the configuration for vectorization pipeline steps.</param>
    /// <param name="stateService">The <see cref="IVectorizationStateService"/> that manages vectorization state.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> implemented by the dependency injection container.</param>
    /// <param name="loggerFactory">The logger factory used to create loggers for logging.</param>
    public class PartitionHandler(
        string instanceId,
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
                if(state.Artifacts.Count > 0)
                {
                    state.Log(this, request.Name!, _messageId, "The extracted text artifact does not contain any text");
                    throw new VectorizationException($"The extracted text artifact did not have text content. Request id: {request.Name} canonical id: {request.ContentIdentifier.CanonicalId}");
                }
                else
                {
                    state.Log(this, request.Name!, _messageId, "No extracted text artifacts found.");
                    throw new VectorizationException($"No extracted text artifacts were found for request id: {request.Name} canonical id: {request.ContentIdentifier.CanonicalId}");
                }
            }

            var serviceFactory = _serviceProvider.GetService<IVectorizationServiceFactory<ITextSplitterService>>()
                ?? throw new VectorizationException($"Could not retrieve the text splitter service factory instance.");
            var textSplitter = serviceFactory.GetService(_parameters["text_partitioning_profile_name"]);

            var splitResult = textSplitter.SplitPlainText(extractedTextArtifact.Content!);

            foreach (var textChunk in splitResult)
                state.AddOrReplaceArtifact(new VectorizationArtifact
                {
                    Type = VectorizationArtifactType.TextPartition,
                    Position = textChunk.Position,
                    Content = textChunk.Content,
                    Size = textChunk.TokensCount
                });

            return true;
        }
    }
}
