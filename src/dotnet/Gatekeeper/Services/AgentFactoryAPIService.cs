using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using Newtonsoft.Json;
using System.Text;
using FoundationaLLM.Common.Interfaces;

namespace FoundationaLLM.Gatekeeper.Core.Services
{
    public class AgentFactoryAPIService : IAgentFactoryAPIService
    {
        private readonly IHttpClientFactoryService _httpClientFactoryService;
        readonly JsonSerializerSettings _jsonSerializerSettings;

        public AgentFactoryAPIService(IHttpClientFactoryService httpClientFactoryService)
        {
            _httpClientFactoryService = httpClientFactoryService;
            _jsonSerializerSettings = CommonJsonSerializerSettings.GetJsonSerializerSettings();
        }

        public async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.AgentFactoryAPIClient);

            var responseMessage = await client.PostAsync("orchestration/completion",
            new StringContent(
                    JsonConvert.SerializeObject(completionRequest, _jsonSerializerSettings),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var completionResponse = JsonConvert.DeserializeObject<CompletionResponse>(responseContent);

                return completionResponse;
            }

            return new CompletionResponse
            {
                Completion = "A problem on my side prevented me from responding.",
                UserPrompt = completionRequest.Prompt,
                UserPromptTokens = 0,
                ResponseTokens = 0,
                UserPromptEmbedding = new float[] { 0 }
            };
        }

        public async Task<SummaryResponse> GetSummary(SummaryRequest summaryRequest)
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.AgentFactoryAPIClient);

            var responseMessage = await client.PostAsync("orchestration/summarize",
                new StringContent(
                    JsonConvert.SerializeObject(summaryRequest, _jsonSerializerSettings),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var summarizeResponse = JsonConvert.DeserializeObject<SummaryResponse>(responseContent);

                return summarizeResponse;
            }
            
            return new SummaryResponse
            {
                Info = "[No Summary]"
            };
        }

        public async Task<bool> SetLLMOrchestrationPreference(string orchestrationService)
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.AgentFactoryAPIClient);

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
