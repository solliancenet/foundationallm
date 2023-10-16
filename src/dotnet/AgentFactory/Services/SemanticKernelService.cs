using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Services
{
    public class SemanticKernelService : ISemanticKernelService
    {
        readonly SemanticKernelServiceSettings _settings;
        readonly ILogger<SemanticKernelService> _logger;
        private readonly IHttpClientFactoryService _httpClientFactoryService;
        readonly JsonSerializerSettings _jsonSerializerSettings;

        public SemanticKernelService(
            IOptions<SemanticKernelServiceSettings> options,
            ILogger<SemanticKernelService> logger,
            IHttpClientFactoryService httpClientFactoryService)
        {
            _settings = options.Value;
            _logger = logger;
            _httpClientFactoryService = httpClientFactoryService;
            _jsonSerializerSettings = CommonJsonSerializerSettings.GetJsonSerializerSettings();
        }

        #region ISemanticKernelOrchestrationService

        public bool IsInitialized => GetServiceStatus();

        public async Task<CompletionResponse> GetCompletion(string userPrompt, List<MessageHistoryItem> messageHistory)
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.SemanticKernelAPI);

            var responseMessage = await client.PostAsync("/orchestration/completion",
                new StringContent(
                    JsonConvert.SerializeObject(new CompletionRequest { UserPrompt = userPrompt, MessageHistory = messageHistory }, _jsonSerializerSettings),
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
                UserPrompt = userPrompt,
                PromptTokens = 0,
                CompletionTokens = 0,
                UserPromptEmbedding = new float[] { 0 }
            };
        }

        public async Task<string> GetSummary(string content)
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.SemanticKernelAPI);

            var responseMessage = await client.PostAsync("/orchestration/summary",
                new StringContent(
                    JsonConvert.SerializeObject(new SummaryRequest { Prompt = content }, _jsonSerializerSettings),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var summaryResponse = JsonConvert.DeserializeObject<SummaryResponse>(responseContent);

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
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.SemanticKernelAPI);
            var responseMessage = client.Send(
                new HttpRequestMessage(HttpMethod.Get, "/status"));

            return responseMessage.Content.ToString() == "ready";
        }

        public Task<LLMOrchestrationCompletionResponse> GetCompletion(LLMOrchestrationCompletionRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
