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
    /// <summary>
    /// The FoundationaLLM Semantic Kernal Service
    /// </summary>
    public class SemanticKernelService : ISemanticKernelService
    {
        readonly SemanticKernelServiceSettings _settings;
        readonly ILogger<SemanticKernelService> _logger;
        private readonly IHttpClientFactoryService _httpClientFactoryService;
        readonly JsonSerializerSettings _jsonSerializerSettings;

        /// <summary>
        /// Constructor for the Semantic Kernal Service
        /// </summary>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        /// <param name="httpClientFactoryService"></param>
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

        /// <summary>
        /// Checks the Semantic Service returns a call to signal it is initialized and ready for requests.
        /// </summary>
        public bool IsInitialized => GetServiceStatus();

        /// <summary>
        /// Gets a completion from the Semantic Kernal
        /// </summary>
        /// <param name="userPrompt"></param>
        /// <param name="messageHistory"></param>
        /// <returns></returns>
        public async Task<CompletionResponse> GetCompletion(string userPrompt, List<MessageHistoryItem> messageHistory)
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.SemanticKernelAPI);

            var responseMessage = await client.PostAsync("orchestration/completion",
                new StringContent(
                    JsonConvert.SerializeObject(new CompletionRequest { UserPrompt = userPrompt, MessageHistory = messageHistory }, _jsonSerializerSettings),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var completionResponse = JsonConvert.DeserializeObject<CompletionResponse>(responseContent)!;
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

        /// <summary>
        /// Gets a summary from the Semantic Kernal
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<string> GetSummary(string content)
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.SemanticKernelAPI);

            var responseMessage = await client.PostAsync("orchestration/summary",
                new StringContent(
                    JsonConvert.SerializeObject(new SummaryRequest { UserPrompt = content }, _jsonSerializerSettings),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var summaryResponse = JsonConvert.DeserializeObject<SummaryResponse>(responseContent);

                return summaryResponse!.Summary;
            }
            else
                return "A problem on my side prevented me from responding.";
        }

        /// <summary>
        /// Adds an item to memory
        /// </summary>
        /// <param name="item"></param>
        /// <param name="itemName"></param>
        /// <param name="vectorizer"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task AddMemory(object item, string itemName, Action<object, float[]> vectorizer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes an item from memory
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task RemoveMemory(object item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the target Semantic Kernal API status
        /// </summary>
        /// <returns></returns>
        private bool GetServiceStatus()
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.SemanticKernelAPI);
            var responseMessage = client.Send(
                new HttpRequestMessage(HttpMethod.Get, "status"));

            return responseMessage.Content.ToString() == "ready";
        }

        /// <summary>
        /// Makes a call to get a completion from the Semantic Kernal
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<LLMOrchestrationCompletionResponse> GetCompletion(LLMOrchestrationCompletionRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
