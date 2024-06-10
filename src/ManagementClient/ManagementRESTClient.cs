using Azure;
using FoundationaLLM.Common.Models.Infrastructure;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Client.Management
{
    /// <summary>
    /// Low level client for calling the Management API endpoints
    /// </summary>
    public class ManagementRESTClient : IManagementRESTClient
    {
        private HttpClient _httpClient = new HttpClient();
        /// <summary>
        /// Constructs the client that calls the Management API with a given base address
        /// </summary>
        /// <param name="apiBaseAddress">The base address of the Management API</param>
        public ManagementRESTClient(string apiBaseAddress) => _httpClient.BaseAddress = new Uri(apiBaseAddress);

        /// <inheritdoc/>
        public async Task<ServiceStatusInfo> GetServiceStatusAsync()
        {
            var response = await _httpClient.GetAsync("/status");
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ServiceStatusInfo>(responseContent)!;
        }

        /// <inheritdoc/>
        public async Task<bool> IsAuthenticatedAsync()
        {
            var response = await _httpClient.GetAsync("/status/auth");
            return response.IsSuccessStatusCode;
        }

        /// <inheritdoc/>
        public async Task<List<T>> GetResources<T>(string instanceId, string resourceProvider, string resourcePath)
        {
            var response = await _httpClient.GetAsync($"instances/{instanceId}/providers/{resourceProvider}/{resourcePath}");
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<T>>(responseContent)!;

        }
        /// <inheritdoc/>
        public async Task<T> UpsertResource<T>(T resource, string instanceId, string resourceProvider, string resourcePath)
        {
            string jsonContent = JsonSerializer.Serialize(resource);

            var response = await _httpClient.PostAsync($"instances/{instanceId}/providers/{resourceProvider}/{resourcePath}",
                new StringContent(jsonContent, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(responseContent)!;
        }

        /// <inheritdoc/>
        public async Task DeleteResource(string instanceId, string resourceProvider, string resourcePath)
        {
            var response = await _httpClient.DeleteAsync($"instances/{instanceId}/providers/{resourceProvider}/{resourcePath}");
        }
    }
}
