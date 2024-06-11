using Azure;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.Infrastructure;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Runtime;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Extensions;

namespace FoundationaLLM.Client.Management
{
    /// <inheritdoc/>
    public class ManagementRESTClient(IHttpClientFactory httpClientFactory) : IManagementRESTClient
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();

        /// <inheritdoc/>
        public async Task<ServiceStatusInfo> GetServiceStatusAsync(string token)
        {
            var mgmtClient = GetManagementClient(token);
            var response = await mgmtClient.GetAsync("/status");
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ServiceStatusInfo>(responseContent, _jsonSerializerOptions)!;
            }
            throw new Exception($"Failed to get service status. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task<bool> IsAuthenticatedAsync(string token)
        {
            var mgmtClient = GetManagementClient(token);
            var response = await mgmtClient.GetAsync("/status/auth");
            return response.IsSuccessStatusCode;
        }

        /// <inheritdoc/>
        public async Task<List<T>> GetResources<T>(string instanceId, string resourceProvider, string resourcePath, string token)
        {
            var mgmtClient = GetManagementClient(token);
            var response = await mgmtClient.GetAsync($"instances/{instanceId}/providers/{resourceProvider}/{resourcePath}");
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<T>>(responseContent, _jsonSerializerOptions)!;
            }
            throw new Exception($"Failed to get requested resource. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");

        }
        /// <inheritdoc/>
        public async Task<T> UpsertResource<T>(T resource, string instanceId, string resourceProvider, string resourcePath, string token)
        {
            var mgmtClient = GetManagementClient(token);
            string jsonContent = JsonSerializer.Serialize(resource);
            var response = await mgmtClient.PostAsync(
                $"instances/{instanceId}/providers/{resourceProvider}/{resourcePath}",
                new StringContent(jsonContent, Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, _jsonSerializerOptions)!;
            }
            throw new Exception($"Failed to upsert requested resource. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task DeleteResource(string instanceId, string resourceProvider, string resourcePath, string token)
        {
            var mgmtClient = GetManagementClient(token);
            var response = await mgmtClient.DeleteAsync($"instances/{instanceId}/providers/{resourceProvider}/{resourcePath}");
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to get requested resource. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }

        private HttpClient GetManagementClient(string token)
        {
            var mgmtClient = httpClientFactory.CreateClient(HttpClients.ManagementAPI);
            mgmtClient.SetBearerToken(token);
            return mgmtClient;
             
        }

    }
}
