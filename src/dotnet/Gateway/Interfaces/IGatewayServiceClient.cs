using FoundationaLLM.Common.Models.Vectorization;

namespace FoundationaLLM.Gateway.Interfaces
{
    /// <summary>
    /// Defines the interface of the FoundationaLLM Gateway service.
    /// </summary>
    public interface IGatewayServiceClient
    {
        /// <summary>
        /// Starts an embedding operation.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="embeddingRequest">The <see cref="TextEmbeddingRequest"/> object containing the details of the embedding operation.</param>
        /// <returns>A <see cref="TextEmbeddingResult"/> object with the outcome of the operation.</returns>
        Task<TextEmbeddingResult> StartEmbeddingOperation(string instanceId, TextEmbeddingRequest embeddingRequest);

        /// <summary>
        /// Retrieves the outcome of an embedding operation.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance id.</param>
        /// <param name="operationId">The unique identifier of the text embedding operation.</param>
        /// <returns>A <see cref="TextEmbeddingResult"/> object with the outcome of the operation.</returns>
        Task<TextEmbeddingResult> GetEmbeddingOperationResult(string instanceId, string operationId);
    }
}
