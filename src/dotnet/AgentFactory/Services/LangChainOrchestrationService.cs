using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration.LangChain;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;
using FoundationaLLM.Common.Models.Orchestration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;
using System.Net.Http;
using FoundationaLLM.Common.Settings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FoundationaLLM.AgentFactory.Services
{
    public class LangChainOrchestrationService : ILangChainOrchestrationService
    {
        readonly LangChainOrchestrationServiceSettings _settings;
        readonly ILogger<LangChainOrchestrationService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        readonly JsonSerializerSettings _jsonSerializerSettings;

        public LangChainOrchestrationService(
            IOptions<LangChainOrchestrationServiceSettings> options,
            ILogger<LangChainOrchestrationService> logger,
            IHttpClientFactory httpClientFactory) 
        {
            _settings = options.Value;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _jsonSerializerSettings = CommonJsonSerializerSettings.GetJsonSerializerSettings();
        }

        public bool IsInitialized => GetServiceStatus();

        public async Task<CompletionResponseBase> GetResponse(string userPrompt, List<MessageHistoryItem> messageHistory)
        {
            var client = _httpClientFactory.CreateClient(Common.Constants.HttpClients.LangChainAPIClient);

            var responseMessage = await client.PostAsync("/orchestration/completion",
                new StringContent(
                    JsonConvert.SerializeObject(new LangChainCompletionRequest { Prompt = userPrompt }, _jsonSerializerSettings),
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
            throw new NotImplementedException();
        }

        private bool GetServiceStatus()
        {
            var client = _httpClientFactory.CreateClient(Common.Constants.HttpClients.LangChainAPIClient);
            var responseMessage = client.Send(
                new HttpRequestMessage(HttpMethod.Get, "/status"));

            return responseMessage.Content.ToString() == "ready";
        }
    }
}
