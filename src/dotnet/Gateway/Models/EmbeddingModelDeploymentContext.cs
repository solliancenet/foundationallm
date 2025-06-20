﻿using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Azure;
using FoundationaLLM.Common.Models.Gateway;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Gateway.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FoundationaLLM.Gateway.Models
{
    /// <summary>
    /// Provides context associated with an embedding model deployment.
    /// </summary>
    /// <param name="deployment">The <see cref="AzureOpenAIAccountDeployment"/> object with the details of the model deployment.</param>
    /// <param name="tokenRateLimitMultiplier">The token rate limit multiplier used to account for the tokenization differences
    /// between the Gateway API and the deployed model.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to create loggers for logging.</param>
    /// <param name="metrics">The FoundationaLLM Gateway telemetry metrics.</param>
    public class EmbeddingModelDeploymentContext(
        AzureOpenAIAccountDeployment deployment,
        double tokenRateLimitMultiplier,
        ILoggerFactory loggerFactory,
        GatewayMetrics metrics)
    {
        private readonly AzureOpenAIAccountDeployment _deployment = deployment;
        private readonly double _tokenRateLimitMultiplier = tokenRateLimitMultiplier;
        private readonly int _actualTokenRateLimit =
            (int)(deployment.TokenRateLimit * tokenRateLimitMultiplier) / (60 / deployment.TokenRateRenewalPeriod);
        private readonly int _actualRequestRateLimit =
            (int)(deployment.RequestRateLimit) / (60 / deployment.RequestRateRenewalPeriod);

        private readonly ILoggerFactory _loggerFactory = loggerFactory;
        private readonly ILogger<EmbeddingModelDeploymentContext> _logger = loggerFactory.CreateLogger<EmbeddingModelDeploymentContext>();
        private readonly GatewayMetrics _metrics = metrics;

        private readonly Dictionary<int, GatewayTextEmbeddingRequest> _embeddingRequests = [];

        private readonly ITextEmbeddingService _textEmbeddingService = new AzureOpenAITextEmbeddingService(
                deployment.AccountEndpoint,
                loggerFactory.CreateLogger<AzureOpenAITextEmbeddingService>());

        private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

        /// <summary>
        /// The cummulated number of tokens for the current token rate window.
        /// </summary>
        private int _tokenRateWindowTokenCount = 0;
        /// <summary>
        /// The cummulated number of requests for the current request rate window.
        /// </summary>
        private int _requestRateWindowRequestCount = 0;
        /// <summary>
        /// The start timestamp of the current token rate window.
        /// </summary>
        private DateTime _tokenRateWindowStart = DateTime.MinValue;
        /// <summary>
        /// The start timestamp of the current request rate window.
        /// </summary>
        private DateTime _requestRateWindowStart = DateTime.MinValue;

        public bool HasInput =>
            _embeddingRequests.Count > 0;

        public bool TryAddInputTextChunk(TextChunk textChunk, int embeddingDimensions)
        {
            UpdateRateWindows();

            if (_tokenRateWindowTokenCount + textChunk.TokensCount > _actualTokenRateLimit)
                // Adding a new text chunk would push us over the token rate limit, so we need to refuse.
                return false;

            if (_requestRateWindowRequestCount == _actualRequestRateLimit)
                // We have already reached the allowed number of requests, so we need to refuse.
                return false;

            if (!_embeddingRequests.ContainsKey(embeddingDimensions))
            {
                if (_requestRateWindowRequestCount + 1 == _actualRequestRateLimit)
                    // Adding a new embedding dimension would result in at least one additional request
                    // which would push us over the request rate limit, so we need to refuse.
                    return false;

                _embeddingRequests[embeddingDimensions] =
                    new GatewayTextEmbeddingRequest
                    {
                        Id = Guid.NewGuid().ToString().ToLower(),
                        AccountName = _deployment.AccountEndpoint,
                        ModelName = _deployment.ModelName,
                        ModelVersion = _deployment.ModelVersion,
                        EmbeddingDimensions = embeddingDimensions,
                        TokenRateWindowStart = _tokenRateWindowStart,
                        RequestRateWindowStart = _requestRateWindowStart,
                        TextChunks = [textChunk]
                    };
            }
            else
            {
                _embeddingRequests[embeddingDimensions].TextChunks.Add(textChunk);
            }

            _tokenRateWindowTokenCount += textChunk.TokensCount;

            return true;
        }

        public async Task<List<EmbeddingRequestResult>> GetEmbeddingsForInputTextChunks()
        {
            try
            {
                var embeddingRequestsTasks = _embeddingRequests
                    .Select(async x => await ProcessEmbeddingRequest(x.Value))
                    .ToArray();

                await Task.WhenAll(embeddingRequestsTasks);

                return [.. embeddingRequestsTasks.Select(t => t.Result)];
            }
            finally
            {
                _embeddingRequests.Clear();
            }
        }

        private async Task<EmbeddingRequestResult> ProcessEmbeddingRequest(
            GatewayTextEmbeddingRequest embeddingRequest)
        {
            Interlocked.Increment(ref _requestRateWindowRequestCount);

            embeddingRequest.TokenRateWindowTokenCount = _tokenRateWindowTokenCount;
            embeddingRequest.RequestRateWindowRequestCount = _requestRateWindowRequestCount;

            _logger.LogInformation("Submitting text embedding request with id {RequestId} and the following metrics: {RequestMetrics}",
                embeddingRequest.Id,
                JsonSerializer.Serialize<GatewayTextEmbeddingRequest>(embeddingRequest, _jsonSerializerOptions));

            // Priority is false since the embedding operation context is already added to the queue.
            var embeddingResult =
                await _textEmbeddingService.GetEmbeddingsAsync(
                    embeddingRequest.TextChunks,
                    _deployment.Name,
                    _deployment.ModelName == "text-embedding-ada-002"
                        ? -1
                        : embeddingRequest.EmbeddingDimensions,
                    false);

            var result = new EmbeddingRequestResult
            {
                TextChunks = embeddingResult.TextChunks,
                Failed = embeddingResult.Failed,
                ErrorMessage = embeddingResult.ErrorMessage
            };

            if (embeddingResult.Failed)
            {
                _logger.LogWarning("The text embedding request with id {RequestId} failed with the following error: {ErrorMessage}",
                    embeddingRequest.Id,
                    embeddingResult.ErrorMessage!);
                result.FailedOperationIds = [.. embeddingRequest.TextChunks
                    .Select(tc => tc.OperationId!)
                    .Distinct()];
            }
            else
            {
                _metrics.IncrementTextChunksEmbedded(embeddingRequest.TextChunksCount);
                _metrics.IncrementTextChunksSizeTokens(embeddingRequest.TokensCount);
            }

            return result;
        }

        private void UpdateRateWindows()
        {
            var refTime = DateTime.UtcNow;

            if ((refTime - _tokenRateWindowStart).TotalSeconds >= _deployment.TokenRateRenewalPeriod)
            {
                _tokenRateWindowStart = refTime;

                // Reset the rate window token count to the sum of token counts of all current embedding requests.
                _tokenRateWindowTokenCount = _embeddingRequests
                    .Sum(er => er.Value.TokensCount);
            }

            if ((refTime - _requestRateWindowStart).TotalSeconds >= _deployment.RequestRateRenewalPeriod)
            {
                _requestRateWindowStart = refTime;
                _requestRateWindowRequestCount = 0;
            }
        }
    }
}
