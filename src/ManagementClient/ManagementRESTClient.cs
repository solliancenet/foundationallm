using FoundationaLLM.Common.Models.Infrastructure;
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
    }
}
