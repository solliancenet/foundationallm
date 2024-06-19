using Azure.Core;
using FoundationaLLM.Client.Core.Interfaces;

namespace FoundationaLLM.Client.Core.Clients.Rest
{
    internal class StatusRESTClient(
        IHttpClientFactory httpClientFactory,
        TokenCredential credential) : CoreRESTClientBase(httpClientFactory, credential), IStatusRESTClient
    {
        /// <inheritdoc/>
        public async Task<string> GetServiceStatusAsync()
        {
            var coreClient = await GetCoreClientAsync();
            var responseMessage = await coreClient.GetAsync("status");

            if (responseMessage.IsSuccessStatusCode)
            {
                return "Service is up and running.";
            }

            throw new Exception($"Failed to retrieve service status. Status code: {responseMessage.StatusCode}. Reason: {responseMessage.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task<string> GetAuthStatusAsync()
        {
            var coreClient = await GetCoreClientAsync();
            var responseMessage = await coreClient.GetAsync("status/auth");

            if (responseMessage.IsSuccessStatusCode)
            {
                return "Authentication is successful.";
            }

            throw new Exception($"Failed to retrieve authentication status. Status code: {responseMessage.StatusCode}. Reason: {responseMessage.ReasonPhrase}");
        }
    }
}
