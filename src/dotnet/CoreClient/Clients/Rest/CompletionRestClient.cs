using Azure.Core;
using FoundationaLLM.Client.Core.Interfaces;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Client.Core.Clients.Rest
{
    /// <summary>
    /// Provides methods to manage calls to the Core API's orchestration endpoints.
    /// </summary>
    internal class CompletionRESTClient(
        IHttpClientFactory httpClientFactory,
        TokenCredential credential) : CoreRESTClientBase(httpClientFactory, credential), ICompletionRESTClient
    {
        /// <inheritdoc/>
        public async Task<Completion> GetChatCompletionAsync(CompletionRequest completionRequest)
        {
            var coreClient = await GetCoreClientAsync();
            var serializedRequest = JsonSerializer.Serialize(completionRequest, SerializerOptions);

            var responseMessage = await coreClient.PostAsync("completions",
                new StringContent(
                    serializedRequest,
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var completionResponse =
                    JsonSerializer.Deserialize<Completion>(responseContent, SerializerOptions);
                return completionResponse ?? throw new InvalidOperationException("The returned completion response is invalid.");
            }

            throw new Exception($"Failed to send completion request. Status code: {responseMessage.StatusCode}. Reason: {responseMessage.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ResourceProviderGetResult<AgentBase>>> GetAgentsAsync()
        {
            var coreClient = await GetCoreClientAsync();
            var responseMessage = await coreClient.GetAsync("completions/agents");

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var agents = JsonSerializer.Deserialize<IEnumerable<ResourceProviderGetResult<AgentBase>>>(responseContent, SerializerOptions);
                return agents ?? throw new InvalidOperationException("The returned agents are invalid.");
            }

            throw new Exception($"Failed to retrieve agents. Status code: {responseMessage.StatusCode}. Reason: {responseMessage.ReasonPhrase}");
        }
    }
}
