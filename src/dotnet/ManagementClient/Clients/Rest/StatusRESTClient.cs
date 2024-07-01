using Azure;
using Azure.Core;
using FoundationaLLM.Client.Management.Interfaces;
using FoundationaLLM.Common.Models.Infrastructure;
using FoundationaLLM.Common.Settings;
using System.Text.Json;

namespace FoundationaLLM.Client.Management.Clients.Rest
{
    internal class StatusRESTClient(
        IHttpClientFactory httpClientFactory,
        TokenCredential credential) : ManagementRESTClientBase(httpClientFactory, credential), IStatusRESTClient
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();

        /// <inheritdoc/>
        public async Task<ServiceStatusInfo> GetServiceStatusAsync()
        {
            var managementClient = await GetManagementClientAsync();
            var response = await managementClient.GetAsync("status");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ServiceStatusInfo>(responseContent, _jsonSerializerOptions)!;
            }

            throw new Exception($"Failed to retrieve service status. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task<bool> IsAuthenticatedAsync()
        {
            var managementClient = await GetManagementClientAsync();
            var response = await managementClient.GetAsync("status/auth");

            return response.IsSuccessStatusCode;
        }
    }
}
