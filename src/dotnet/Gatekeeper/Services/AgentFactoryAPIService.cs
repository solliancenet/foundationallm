using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using Newtonsoft.Json;
using System.Text;
using FoundationaLLM.Common.Interfaces;

namespace FoundationaLLM.Gatekeeper.Core.Services
{
    /// <summary>
    /// Implements the <see cref="IAgentFactoryAPIService"/> interface.
    /// </summary>
    public class AgentFactoryAPIService : IAgentFactoryAPIService
    {
        private readonly IHttpClientFactoryService _httpClientFactoryService;
        readonly JsonSerializerSettings _jsonSerializerSettings;

        /// <summary>
        /// Constructor for the Agent Factory APIS client.
        /// </summary>
        /// <param name="httpClientFactoryService">The HTTP client factory service.</param>
        public AgentFactoryAPIService(IHttpClientFactoryService httpClientFactoryService)
        {
            _httpClientFactoryService = httpClientFactoryService;
            _jsonSerializerSettings = CommonJsonSerializerSettings.GetJsonSerializerSettings();
        }

        /// <summary>
        /// Gets a completion from the Agent Factory API.
        /// </summary>
        /// <param name="completionRequest">The completion request containing the user prompt and message history.</param>
        /// <returns>The completion response.</returns>
        public async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
        {
            var fallback = new CompletionResponse
            {
                Completion = "A problem on my side prevented me from responding.",
                UserPrompt = completionRequest.UserPrompt,
                PromptTokens = 0,
                CompletionTokens = 0,
                UserPromptEmbedding = new float[] { 0 }
            };

            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.AgentFactoryAPI);

            var responseMessage = await client.PostAsync("orchestration/completion",
            new StringContent(
                    JsonConvert.SerializeObject(completionRequest, _jsonSerializerSettings),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var completionResponse = JsonConvert.DeserializeObject<CompletionResponse>(responseContent);

                return completionResponse ?? fallback;
            }

            return fallback;
        }

        /// <summary>
        /// Gets a summary from the Agent Factory API.
        /// </summary>
        /// <param name="summaryRequest">The summarize request containing the user prompt.</param>
        /// <returns>The summary response.</returns>
        public async Task<SummaryResponse> GetSummary(SummaryRequest summaryRequest)
        {
            var fallback = new SummaryResponse
            {
                Summary = "[No Summary]"
            };

            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.AgentFactoryAPI);

            var responseMessage = await client.PostAsync("orchestration/summary",
                new StringContent(
                    JsonConvert.SerializeObject(summaryRequest, _jsonSerializerSettings),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var summarizeResponse = JsonConvert.DeserializeObject<SummaryResponse>(responseContent);

                return summarizeResponse ?? fallback;
            }

            return fallback;
        }

        /// <summary>
        /// Sets the preffered orchestration service.
        /// </summary>
        /// <param name="orchestrationService">The name of the preferred orchestration service.</param>
        /// <returns>True if the preffered orchestration service was set. Otherwise, returns False.</returns>
        public async Task<bool> SetLLMOrchestrationPreference(string orchestrationService)
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.AgentFactoryAPI);

            var responseMessage = await client.PostAsync("orchestration/preference",
                new StringContent(orchestrationService));

            if (responseMessage.IsSuccessStatusCode)
            {
                // The response value should be a boolean indicating whether the orchestration service was set successfully.
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var orchestrationServiceSet = JsonConvert.DeserializeObject<bool>(responseContent);

                return orchestrationServiceSet;
            }

            return false;
        }
    }
}
