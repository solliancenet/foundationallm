using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Gateway.Interfaces;

namespace FoundationaLLM.Vectorization.Services.Text
{
    /// <summary>
    /// Generates text embeddings by routing requests through the FoundationaLLM Gateway API.
    /// </summary>
    /// <param name="gatewayService">The <see cref="IGatewayServiceClient"/> used to call the Gateway API.</param>
    public class GatewayTextEmbeddingService(
        IGatewayServiceClient gatewayService) : ITextEmbeddingService
    {
        private readonly IGatewayServiceClient _gatewayService = gatewayService;

        /// <inheritdoc/>
        public async Task<TextEmbeddingResult> GetEmbeddingsAsync(string instanceId, IList<TextChunk> textChunks, string modelName) =>
            await _gatewayService.StartEmbeddingOperation(instanceId, new TextEmbeddingRequest
            {
                EmbeddingModelName = modelName,
                TextChunks = textChunks
            });

        /// <inheritdoc/>
        public async Task<TextEmbeddingResult> GetEmbeddingsAsync(string instanceId, string operationId) =>
            await _gatewayService.GetEmbeddingOperationResult(instanceId, operationId);
    }
}
