﻿using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Vectorization.Client
{
    /// <summary>
    /// Provides method calls to the Vectorization API service.
    /// </summary>
    public class VectorizationServiceClient : IVectorizationServiceClient
    {
        private readonly VectorizationServiceSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<VectorizationServiceClient> _logger;

        /// <summary>
        /// Creates a new instance of the Vectorization API client.
        /// </summary>
        /// <param name="httpClientFactory">The <see cref="IHttpClientFactory"/> used to create the HTTP client.</param>
        /// <param name="options">The options object containing the <see cref="VectorizationServiceSettings"/> object with the setting.</param>
        /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
        public VectorizationServiceClient(
            IHttpClientFactory httpClientFactory,
            IOptions<VectorizationServiceSettings> options,
            ILogger<VectorizationServiceClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _settings = options.Value;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<VectorizationResult> ProcessRequest(VectorizationRequest vectorizationRequest)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_settings.APIUrl);
            httpClient.DefaultRequestHeaders.Add(HttpHeaders.APIKey, _settings.APIKey);

            var serializedRequest = JsonSerializer.Serialize(vectorizationRequest);

            try
            {
                var response = await httpClient.PostAsync("vectorizationrequest", new StringContent(serializedRequest, Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<VectorizationResult>(responseContent);
                    if (result != null)
                    {
                        return result;
                    }
                    else
                    {
                        _logger.LogError($"Failed to deserialize Vectorization API response for request {vectorizationRequest.ObjectId}");
                        return new VectorizationResult(
                            vectorizationRequest.ObjectId!,
                            false,
                            "Failed to deserialize Vectorization API response"
                        );
                    }
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to process Vectorization API request {vectorizationRequest.ObjectId}: Code [{response.StatusCode}], Body [{errorContent}]");
                    return new VectorizationResult(
                        vectorizationRequest.ObjectId!,
                        false,
                        $"Call to Vectorization API failed: Code [{response.StatusCode}], Body [{errorContent}]"
                    );

                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Failed to process Vectorization API request {vectorizationRequest.ObjectId}: {ex.Message}");
                return new VectorizationResult(
                    vectorizationRequest.ObjectId!,
                    false,
                    $"Failed to process Vectorization API request: {ex.Message}"
                );
            }
        }
    }
}
