using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration.LangChain;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Models.ConfigurationOptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;
using System.Net.Http;

namespace FoundationaLLM.Core.Services
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
            _jsonSerializerSettings = GetJsonSerializerSettings();
        }

        public bool IsInitialized => GetServiceStatus();

        public async Task<(string Completion, string UserPrompt, int UserPromptTokens, int ResponseTokens, float[]? UserPromptEmbedding)> GetCompletion(string userPrompt, List<MessageHistoryItem> messageHistory)
        {
            var client = _httpClientFactory.CreateClient(Constants.HttpClients.LangChainApiClient);

            var responseMessage = await client.PostAsync("/orchestration/completion",
                new StringContent(
                    JsonConvert.SerializeObject(new LangChainCompletionRequest { Prompt = userPrompt }, _jsonSerializerSettings),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var completionResponse = JsonConvert.DeserializeObject<LangChainCompletionResponse>(responseContent);

                return new(completionResponse?.Info, userPrompt, 0, 0, new float[] { 0 });
            }
            else
                return new("A problem on my side prevented me from responding.", userPrompt, 0, 0, new float[] { 0 });
        }

        public async Task<string> GetSummary(string content)
        {
            throw new NotImplementedException();
        }

        private JsonSerializerSettings GetJsonSerializerSettings()
        {
            return new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        private bool GetServiceStatus()
        {
            var client = _httpClientFactory.CreateClient(Constants.HttpClients.LangChainApiClient);
            var responseMessage = client.Send(
                new HttpRequestMessage(HttpMethod.Get, "/status"));

            return responseMessage.Content.ToString() == "ready";
        }
    }
}
