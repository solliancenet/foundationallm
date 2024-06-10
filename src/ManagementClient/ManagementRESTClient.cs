using Azure;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.Infrastructure;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Runtime;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Client.Management
{
    /// <summary>
    /// Low level client for calling the Management API endpoints
    /// </summary>
    public class ManagementRESTClient : IManagementRESTClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ClientSettings _settings;

        /// <summary>
        /// Constructs the client that calls the Management API with a given base address
        /// </summary>
        /// <param name="httpClientFactory">Client factory for creation on HttpClient instances</param>
        public ManagementRESTClient(IHttpClientFactory httpClientFactory,
            IOptions<ClientSettings> options)
        {
            _httpClientFactory = httpClientFactory;
            _settings = options.Value;
        }

        /// <inheritdoc/>
        public async Task<ServiceStatusInfo> GetServiceStatusAsync(string accessToken)
        {
            var httpClient = await CreateHttpClient(accessToken);
            var response = await httpClient.GetAsync("/status");
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ServiceStatusInfo>(responseContent)!;
        }

        /// <inheritdoc/>
        public async Task<bool> IsAuthenticatedAsync(string accessToken)
        {
            var httpClient = await CreateHttpClient(accessToken);
            var response = await httpClient.GetAsync("/status/auth");
            return response.IsSuccessStatusCode;
        }

        /// <inheritdoc/>
        public async Task<List<T>> GetResources<T>(string instanceId, string resourceProvider, string resourcePath, string accessToken)
        {
            var httpClient = await CreateHttpClient(accessToken);
            var response = await httpClient.GetAsync($"instances/{instanceId}/providers/{resourceProvider}/{resourcePath}");
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<T>>(responseContent)!;

        }
        /// <inheritdoc/>
        public async Task<T> UpsertResource<T>(T resource, string instanceId, string resourceProvider, string resourcePath, string accessToken)
        {
            string jsonContent = JsonSerializer.Serialize(resource);
            var httpClient = await CreateHttpClient(accessToken);
            var response = await httpClient.PostAsync($"instances/{instanceId}/providers/{resourceProvider}/{resourcePath}",
                new StringContent(jsonContent, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent)!;
        }

        /// <inheritdoc/>
        public async Task DeleteResource(string instanceId, string resourceProvider, string resourcePath, string accessToken)
        {
            var httpClient = await CreateHttpClient(accessToken);
            var response = await httpClient.DeleteAsync($"instances/{instanceId}/providers/{resourceProvider}/{resourcePath}");
        }

        private async Task<HttpClient> CreateHttpClient(string accessToken)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_settings.APIUrl);

            var credentials = DefaultAuthentication.AzureCredential;
            var tokenResult = await credentials.GetTokenAsync(
                new([_settings.APIScope]),
                default);

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResult.Token);

            return httpClient;
        }

    }
}
