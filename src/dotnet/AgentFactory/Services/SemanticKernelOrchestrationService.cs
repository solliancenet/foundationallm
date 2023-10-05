using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;
using FoundationaLLM.Common.Models.Orchestration.SemanticKernel;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Common.Models.Orchestration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace FoundationaLLM.AgentFactory.Services
{
    public class SemanticKernelOrchestrationService : ISemanticKernelOrchestrationService
    {
        readonly SemanticKernelOrchestrationServiceSettings _settings;
        readonly ILogger<SemanticKernelOrchestrationService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        readonly JsonSerializerSettings _jsonSerializerSettings;

        public SemanticKernelOrchestrationService(
            IOptions<SemanticKernelOrchestrationServiceSettings> options,
            ILogger<SemanticKernelOrchestrationService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _settings = options.Value;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _jsonSerializerSettings = CommonJsonSerializerSettings.GetJsonSerializerSettings();
        }

        #region ISemanticKernelOrchestrationService

        public bool IsInitialized => GetServiceStatus();

        public async Task<CompletionResponseBase> GetResponse(string userPrompt, List<MessageHistoryItem> messageHistory)
        {
            var client = _httpClientFactory.CreateClient(Common.Constants.HttpClients.SemanticKernelAPIClient);

            var responseMessage = await client.PostAsync("/orchestration/completion",
                new StringContent(
                    JsonConvert.SerializeObject(new SemanticKernelCompletionRequest { Prompt = userPrompt, MessageHistory = messageHistory }, _jsonSerializerSettings),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var completionResponse = JsonConvert.DeserializeObject<CompletionResponseBase>(responseContent);

                return completionResponse;
            }
            
            return new CompletionResponseBase
            {
                Completion = "A problem on my side prevented me from responding.",
                UserPrompt = userPrompt,
                UserPromptTokens = 0,
                ResponseTokens = 0,
                UserPromptEmbedding = new float[] { 0 }
            };
        }

        public async Task<string> GetSummary(string content)
        {
            var client = _httpClientFactory.CreateClient(Common.Constants.HttpClients.SemanticKernelAPIClient);

            var responseMessage = await client.PostAsync("/orchestration/summary",
                new StringContent(
                    JsonConvert.SerializeObject(new SemanticKernelSummaryRequest { Prompt = content }, _jsonSerializerSettings),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var summaryResponse = JsonConvert.DeserializeObject<SemanticKernelSummaryResponse>(responseContent);

                return summaryResponse?.Info;
            }
            else
                return "A problem on my side prevented me from responding.";
        }

        public Task AddMemory(object item, string itemName, Action<object, float[]> vectorizer)
        {
            throw new NotImplementedException();
        }

        public Task RemoveMemory(object item)
        {
            throw new NotImplementedException();
        }
        #endregion

        private bool GetServiceStatus()
        {
            var client = _httpClientFactory.CreateClient(Common.Constants.HttpClients.SemanticKernelAPIClient);
            var responseMessage = client.Send(
                new HttpRequestMessage(HttpMethod.Get, "/status"));

            return responseMessage.Content.ToString() == "ready";
        }
    }
}
