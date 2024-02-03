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
        /// Gets a completion from the Semantic Kernel service.
        /// </summary>
        /// <param name="request">Request object populated from the hub APIs including agent, prompt, data source, and model information.</param>
        /// <returns>Returns a completion response from the orchestration engine.</returns>
        public async Task<LLMOrchestrationCompletionResponse> GetCompletion(LLMOrchestrationCompletionRequest request)
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.SemanticKernelAPI);

            var completionRequest = new CompletionRequest
            {
                SessionId = request.SessionId ?? string.Empty,
                UserPrompt = request.UserPrompt ?? string.Empty,
                MessageHistory = request.MessageHistory
            };

            var responseMessage = await client.PostAsync("orchestration/completion",
                new StringContent(
                    JsonConvert.SerializeObject(completionRequest, _jsonSerializerSettings),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var completionResponse = JsonConvert.DeserializeObject<CompletionResponse>(responseContent)!;
                return new LLMOrchestrationCompletionResponse
                {
                    Completion = completionResponse!.Completion,
                    UserPrompt = completionResponse.UserPrompt,
                    PromptTemplate = request.Agent?.PromptPrefix,
                    AgentName = request.Agent?.Name,
                    PromptTokens = completionResponse.PromptTokens,
                    CompletionTokens = completionResponse.CompletionTokens
                };
            }
            
            return new LLMOrchestrationCompletionResponse
            {
                Completion = "A problem on my side prevented me from responding.",
                UserPrompt = request.UserPrompt,
                PromptTemplate = request.Agent?.PromptPrefix,
                AgentName = request.Agent?.Name,
                PromptTokens = 0,
                CompletionTokens = 0
            };
        }

        /// <summary>
        /// Gets a summary from the Semantic Kernel service.
        /// </summary>
        /// <param name="orchestrationRequest">The orchestration request that includes the text to summarize.</param>
        /// <returns>Returns a summary of the input text.</returns>
        public async Task<string> GetSummary(LLMOrchestrationRequest orchestrationRequest)
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.SemanticKernelAPI);

            var responseMessage = await client.PostAsync("orchestration/summary",
                new StringContent(
                    JsonConvert.SerializeObject(new SummaryRequest
                    {
                        SessionId = orchestrationRequest.SessionId,
                        UserPrompt = orchestrationRequest.UserPrompt
                    }, _jsonSerializerSettings),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var summaryResponse = JsonConvert.DeserializeObject<SummaryResponse>(responseContent);

                return summaryResponse!.Summary!;
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
        /// Gets the target Semantic Kernel API status.
        /// </summary>
        /// <returns></returns>
        private bool GetServiceStatus()
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.SemanticKernelAPI);
            var responseMessage = client.Send(
                new HttpRequestMessage(HttpMethod.Get, "status"));

            return responseMessage.Content.ToString() == "ready";
        }

        public Task<LLMOrchestrationCompletionResponse> GetCompletion(string agentName, string serializedRequest) => throw new NotImplementedException();
    }
}
