﻿using Azure.AI.OpenAI;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Vectorization;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Gateway.Services
{
    /// <summary>
    /// Implementation of <see cref="ITextEmbeddingService"/> using Azure OpenAI.
    /// </summary>
    public class AzureOpenAITextEmbeddingService : ITextEmbeddingService
    {
        private readonly string _accountEndpoint;
        private readonly AzureOpenAIClient _azureOpenAIClient;
        private readonly ILogger<AzureOpenAITextEmbeddingService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureOpenAITextEmbeddingService"/> class.
        /// </summary>
        /// <param name="accountEndpoint">The endpoint of the Azure OpenAI service.</param>
        /// <param name="logger"></param>
        public AzureOpenAITextEmbeddingService(
            string accountEndpoint,
            ILogger<AzureOpenAITextEmbeddingService> logger)
        {
            _accountEndpoint = accountEndpoint;
            _azureOpenAIClient = new AzureOpenAIClient(new Uri(_accountEndpoint), ServiceContext.AzureCredential);
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<TextEmbeddingResult> GetEmbeddingsAsync(IList<TextChunk> textChunks, string deploymentName, int embeddingDimensions, bool prioritized)
        {
            // Priority not relevant as the text embedding context has already been queued at a higher level.
            try
            {
                var embeddingClient = _azureOpenAIClient.GetEmbeddingClient(deploymentName);
                OpenAI.Embeddings.EmbeddingGenerationOptions? embeddingOptions =
                    embeddingDimensions == -1
                    ? null
                    : new OpenAI.Embeddings.EmbeddingGenerationOptions { Dimensions = embeddingDimensions };
                var result = await embeddingClient.GenerateEmbeddingsAsync(
                    textChunks.Select(tc => tc.Content!).ToList(),
                    embeddingOptions);

                return new TextEmbeddingResult
                {
                    InProgress = false,
                    TextChunks = [.. Enumerable.Range(0, result.Value.Count).Select(i =>
                    {
                        var textChunk = textChunks[i];
                        textChunk.Embedding = new Embedding(result.Value[i].ToFloats());
                        return textChunk;
                    })]
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating embeddings.");
                return new TextEmbeddingResult
                {
                    InProgress = false,
                    Failed = true,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <inheritdoc/>
        public Task<TextEmbeddingResult> GetEmbeddingsAsync(string operationId) => throw new NotImplementedException();
    }
}
