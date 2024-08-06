using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Azure;
using FoundationaLLM.Common.Models.Gateway;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.SemanticKernel.Core.Models.Configuration;
using FoundationaLLM.SemanticKernel.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FoundationaLLM.Gateway.Models
{
    /// <summary>
    /// Provides context associated with an embedding model deployment.
    /// </summary>
    /// <param name="deployment">The <see cref="AzureOpenAIAccountDeployment"/> object with the details of the model deployment.</param>
    /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to create loggers for logging.</param>
    public class EmbeddingModelDeploymentContext(
        AzureOpenAIAccountDeployment deployment,
        string instanceId,
        ILoggerFactory loggerFactory)
    {
        private const int OPENAI_MAX_INPUT_SIZE_TOKENS = 8191;

        private readonly AzureOpenAIAccountDeployment _deployment = deployment;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;
        private readonly ILogger<EmbeddingModelDeploymentContext> _logger = loggerFactory.CreateLogger<EmbeddingModelDeploymentContext>();
        private List<TextChunk> _inputTextChunks = [];

        private readonly ITextEmbeddingService _textEmbeddingService = new SemanticKernelTextEmbeddingService(
                Options.Create(new SemanticKernelTextEmbeddingServiceSettings
                {
                    AuthenticationType = AuthenticationTypes.AzureIdentity,
                    Endpoint = deployment.AccountEndpoint,
                    DeploymentName = deployment.Name
                }),
                loggerFactory);

        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };

        /// <summary>
        /// The cumulated number of tokens for the current token rate window.
        /// </summary>
        private int _tokenRateWindowTokenCount = 0;
        /// <summary>
        /// The cumulated number of requests for the current request rate window.
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

        private int _currentRequestTokenCount = 0;

        public bool HasInput =>
            _inputTextChunks.Count > 0;

        public bool TryAddInputTextChunk(TextChunk textChunk)
        {
            UpdateRateWindows();

            if (_tokenRateWindowTokenCount + textChunk.TokensCount > _deployment.TokenRateLimit
                || _currentRequestTokenCount + textChunk.TokensCount > OPENAI_MAX_INPUT_SIZE_TOKENS)
                // Adding a new text chunk would either push us over to the token rate limit or exceed the maximum input size, so we need to refuse.
                return false;

            if (_requestRateWindowRequestCount == _deployment.RequestRateLimit)
                // We have already reached the allowed number of requests, so we need to refuse.
                return false;

            _inputTextChunks.Add(textChunk);
            _tokenRateWindowTokenCount += textChunk.TokensCount;
            _currentRequestTokenCount += textChunk.TokensCount;

            return true;
        }

        public async Task<TextEmbeddingResult> GetEmbeddingsForInputTextChunks()
        {
            try
            {
                var gatewayMetrics = new GatewayTextEmbeddingRequestMetrics
                {
                    Id = Guid.NewGuid().ToString().ToLower(),
                    AccountName = _deployment.AccountEndpoint,
                    ModelName = _deployment.ModelName,
                    ModelVersion = _deployment.ModelVersion,
                    TokenRateWindowStart = _tokenRateWindowStart,
                    RequestRateWindowStart = _requestRateWindowStart,
                    TokenRateWindowTokenCount = _tokenRateWindowTokenCount,
                    RequestRateWindowRequestCount = _requestRateWindowRequestCount,
                    CurrentRequestTokenCount = _currentRequestTokenCount,
                    CurrentTextChunkCount = _inputTextChunks.Count,
                    OperationsDetails = _inputTextChunks
                        .GroupBy(tc => tc.OperationId!)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(tc => tc.Position).ToList())
                };

                _logger.LogInformation("Submitting text embedding request with id {RequestId} and the following metrics: {RequestMetrics}",
                    gatewayMetrics.Id,
                    JsonSerializer.Serialize<GatewayTextEmbeddingRequestMetrics>(gatewayMetrics, _jsonSerializerOptions));

                var embeddingResult =
                    await _textEmbeddingService.GetEmbeddingsAsync(instanceId, _inputTextChunks);

                if (embeddingResult.Failed)
                    _logger.LogWarning("The text embedding request with id {RequestId} failed with the following error: {ErrorMessage}",
                        gatewayMetrics.Id,
                        embeddingResult.ErrorMessage!);

                return embeddingResult;
            }
            finally
            {
                _requestRateWindowRequestCount++;

                _inputTextChunks.Clear();
                _currentRequestTokenCount = 0;
            }
        }

        private void UpdateRateWindows()
        {
            var refTime = DateTime.UtcNow;

            if ((refTime - _tokenRateWindowStart).TotalSeconds >= _deployment.TokenRateRenewalPeriod)
            {
                _tokenRateWindowStart = refTime;

                // Reset the rate window token count to the sum of token counts of all current input text chunks.
                _tokenRateWindowTokenCount = _inputTextChunks.Sum(tc => tc.TokensCount);
            }

            if ((refTime - _requestRateWindowStart).TotalSeconds >= _deployment.RequestRateRenewalPeriod)
            {
                _requestRateWindowStart = refTime;
                _requestRateWindowRequestCount = 0;
            }
        }
    }
}
