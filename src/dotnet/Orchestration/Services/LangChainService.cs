using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Infrastructure;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Orchestration.Core.Interfaces;
using FoundationaLLM.Orchestration.Core.Models.ConfigurationOptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Orchestration.Core.Services
{
    /// <summary>
    /// The LangChain orchestration service.
    /// </summary>
    public class LangChainService : ILangChainService
    {
        readonly LangChainServiceSettings _settings;
        readonly ILogger<LangChainService> _logger;
        private readonly ICallContext _callContext;
        private readonly IHttpClientFactoryService _httpClientFactoryService;
        readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// LangChain Orchestration Service
        /// </summary>
        public LangChainService(
            IOptions<LangChainServiceSettings> options,
            ILogger<LangChainService> logger,
            ICallContext callContext,
            IHttpClientFactoryService httpClientFactoryService) 
        {
            _settings = options.Value;
            _logger = logger;
            _callContext = callContext;
            _httpClientFactoryService = httpClientFactoryService;
            _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();
            _jsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        }

        /// <inheritdoc/>
        public async Task<ServiceStatusInfo> GetStatus(string instanceId)
        {
            var client = await _httpClientFactoryService.CreateClient(HttpClientNames.LangChainAPI, _callContext.CurrentUserIdentity);
            var responseMessage = await client.SendAsync(
                new HttpRequestMessage(HttpMethod.Get, $"instances/{instanceId}/status"));

            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ServiceStatusInfo>(responseContent)!;
        }

        /// <inheritdoc/>
        public string Name => LLMOrchestrationServiceNames.LangChain;

        /// <summary>
        /// Executes a completion request against the orchestration service.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance ID.</param>
        /// <param name="request">Request object populated from the hub APIs including agent, prompt, data source, and model information.</param>
        /// <returns>Returns a completion response from the orchestration engine.</returns>
        public async Task<LLMCompletionResponse> GetCompletion(string instanceId, LLMCompletionRequest request)
        {            
            var client = await _httpClientFactoryService.CreateClient(HttpClientNames.LangChainAPI, _callContext.CurrentUserIdentity);

            var pollingClient = new PollingHttpClient<LLMCompletionRequest, LLMCompletionResponse>(
                client,
                request,
                $"instances/{instanceId}/async-completions",
                TimeSpan.FromSeconds(0.5),
                client.Timeout.Subtract(TimeSpan.FromSeconds(1)),
                _logger);

            try
            {
                var completionResponse = await pollingClient.GetResponseAsync();
                if (completionResponse == null)
                    throw new Exception("The LangChain orchestration service did not return a valid completion response.");
                return new LLMCompletionResponse
                {
                    OperationId = request.OperationId,
                    Completion = completionResponse!.Completion,
                    Citations = completionResponse.Citations,
                    UserPrompt = completionResponse.UserPrompt,
                    FullPrompt = completionResponse.FullPrompt,
                    PromptTemplate = string.Empty,
                    AgentName = request.Agent.Name,
                    PromptTokens = completionResponse.PromptTokens,
                    CompletionTokens = completionResponse.CompletionTokens
                };
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing the completion request against the LangChain orchestration service.");
                
                return new LLMCompletionResponse
                {
                    OperationId = request.OperationId,
                    Completion = "A problem on my side prevented me from responding.",
                    UserPrompt = request.UserPrompt,
                    PromptTemplate = string.Empty,
                    AgentName = request.Agent.Name,
                    PromptTokens = 0,
                    CompletionTokens = 0
                };
            }
        }
    }
}
