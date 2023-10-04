using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Core.Models.ConfigurationOptions;
using FoundationaLLM.Common.Models.Orchestration.SemanticKernel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace FoundationaLLM.Core.Services
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
            _jsonSerializerSettings = GetJsonSerializerSettings();
        }

        #region ISemanticKernelOrchestrationService

        public bool IsInitialized => GetServiceStatus();

        public async Task<(string Completion, string UserPrompt, int UserPromptTokens, int ResponseTokens, float[]? UserPromptEmbedding)> GetResponse(string userPrompt, List<MessageHistory> messageHistory)
        {
            var client = _httpClientFactory.CreateClient(Constants.HttpClients.SemanticKernelApiClient);

            var responseMessage = await client.PostAsync("api/orchestration/complete",
                new StringContent(
                    JsonConvert.SerializeObject(new SemanticKernelCompletionRequest { Prompt = userPrompt, MessageHistory = messageHistory }, _jsonSerializerSettings),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var completionResponse = JsonConvert.DeserializeObject<SemanticKernelCompletionResponse>(responseContent);

                return new(completionResponse?.Info, userPrompt, 0, 0, new float[] { 0 });
            }
            else
                return new("A problem on my side prevented me from responding.", userPrompt, 0, 0, new float[] { 0 });
        }

        public async Task<string> Summarize(string content)
        {
            var client = _httpClientFactory.CreateClient(Constants.HttpClients.SemanticKernelApiClient);

            var responseMessage = await client.PostAsync("api/orchestration/summarize",
                new StringContent(
                    JsonConvert.SerializeObject(new SemanticKernelSummarizeRequest { Prompt = content }, _jsonSerializerSettings),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var summarizeResponse = JsonConvert.DeserializeObject<SemanticKernelSummarizeResponse>(responseContent);

                return summarizeResponse?.Info;
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

        private JsonSerializerSettings GetJsonSerializerSettings()
        {
            return new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        private bool GetServiceStatus()
        {
            var client = _httpClientFactory.CreateClient(Constants.HttpClients.SemanticKernelApiClient);
            var responseMessage = client.Send(
                new HttpRequestMessage(HttpMethod.Get, "/status"));

            return responseMessage.Content.ToString() == "ready";
        }
    }
}
