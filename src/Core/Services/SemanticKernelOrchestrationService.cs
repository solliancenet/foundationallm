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
        readonly HttpClient _httpClient;
        readonly JsonSerializerSettings _jsonSerializerSettings;

        public SemanticKernelOrchestrationService(
            IOptions<SemanticKernelOrchestrationServiceSettings> options,
            ILogger<SemanticKernelOrchestrationService> logger)
        {
            _settings = options.Value;
            _logger = logger;

            _httpClient = GetHttpClient();
            _jsonSerializerSettings = GetJsonSerializerSettings();
        }

        #region ISemanticKernelOrchestrationService

        public bool IsInitialized => GetServiceStatus();

        public async Task<(string Completion, string UserPrompt, int UserPromptTokens, int ResponseTokens, float[]? UserPromptEmbedding)> GetResponse(string userPrompt, List<Message> messageHistory)
        {
            var responseMessage = await _httpClient.PostAsync("api/orchestration/complete",
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
            var responseMessage = await _httpClient.PostAsync("api/orchestration/summarize",
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

        private HttpClient GetHttpClient()
        {
            var httpClient = new HttpClient()
            {
                BaseAddress = new Uri(_settings.APIUrl)
            };

            httpClient.DefaultRequestHeaders.Add("X-API-KEY", _settings.APIKey);

            return httpClient;
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
            var responseMessage = _httpClient.Send(
                new HttpRequestMessage(HttpMethod.Get, "/status"));

            return responseMessage.Content.ToString() == "ready";
        }
    }
}
