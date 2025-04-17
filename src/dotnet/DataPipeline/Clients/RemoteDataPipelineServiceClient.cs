using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.DataPipeline.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace FoundationaLLM.DataPipeline.Clients
{
    /// <summary>
    /// Remote client for the Data Pipeline API.
    /// </summary>
    public class RemoteDataPipelineServiceClient : IDataPipelineServiceClient
    {
        private readonly Task<HttpClient> _httpClientTask;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes the remote client for the Data Pipeline API.
        /// </summary>
        /// <param name="httpClientFactoryService">The HTTP client factory used to create HTTP clients.</param>
        /// <param name="logger">The logger used for logging.</param>
        public RemoteDataPipelineServiceClient(
            IHttpClientFactoryService httpClientFactoryService,
            ILogger<RemoteDataPipelineServiceClient> logger)
        {
            _httpClientTask = CreateHttpClient(httpClientFactoryService);
            _logger = logger;
        }

        private static async Task<HttpClient> CreateHttpClient(
            IHttpClientFactoryService httpClientFactory) =>
           await httpClientFactory.CreateClient(
                HttpClientNames.DataPipelineAPI,
                ServiceContext.ServiceIdentity!);

        /// <inheritdoc/>
        public async Task<DataPipelineRun?> CreateDataPipelineRunAsync(
            string instanceId,
            DataPipelineRun dataPipelineRun,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                var httpClient = await _httpClientTask;

                var responseMessage = await httpClient.PostAsJsonAsync<DataPipelineRun>(
                    $"instances/{instanceId}/datapipelineruns",
                    dataPipelineRun);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<DataPipelineRun>(responseContent);
                    return response;
                }

                _logger.LogError(
                    "An error occurred while creating the data pipeline run. Status code: {StatusCode}.",
                    responseMessage.StatusCode);

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the data pipeline run.");
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<DataPipelineRun?> GetDataPipelineRunAsync(
            string instanceId,
            string runId,
            UnifiedUserIdentity userIdentity)
        {
            try
            {
                var httpClient = await _httpClientTask;

                var responseMessage = await httpClient.GetAsync(
                    $"instances/{instanceId}/datapipelineruns/{runId}");

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<DataPipelineRun>(responseContent);
                    return response;
                }

                _logger.LogError(
                    "An error occurred while retrieving the data pipeline run. Status code: {StatusCode}.",
                    responseMessage.StatusCode);

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the data pipeline run.");
                return null;
            }
        }
    }
}
