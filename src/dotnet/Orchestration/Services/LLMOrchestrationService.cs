using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Infrastructure;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Orchestration.Response;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Orchestration.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Orchestration.Core.Services
{
    /// <summary>
    /// Provides methods to call an external LLM orchestration service.
    /// </summary>
    public class LLMOrchestrationService : ILLMOrchestrationService
    {
        private readonly string _serviceName;
        private readonly ILogger<LLMOrchestrationService> _logger;
        private readonly IHttpClientFactoryService _httpClientFactoryService;
        private readonly UnifiedUserIdentity _userIdentity;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// LLM Orchestration Service
        /// </summary>
        public LLMOrchestrationService(
            string serviceName,
            ILogger<LLMOrchestrationService> logger,
            IHttpClientFactoryService httpClientFactoryService,
            ICallContext callContext) 
        {
            _serviceName = serviceName;
            _logger = logger;
            _httpClientFactoryService = httpClientFactoryService;
            _userIdentity = callContext.CurrentUserIdentity
                ?? throw new ArgumentException("The provided call context does not have a valid user identity.");
            _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();
            _jsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        }

        /// <inheritdoc/>
        public async Task<ServiceStatusInfo> GetStatus(string instanceId)
        {
            var client = await _httpClientFactoryService.CreateClient(_serviceName, _userIdentity);
            var responseMessage = await client.SendAsync(
                new HttpRequestMessage(HttpMethod.Get, $"/instances/{instanceId}/status"));

            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ServiceStatusInfo>(responseContent)!;
        }

        /// <inheritdoc/>
        public string Name => _serviceName;

        /// <inheritdoc/>
        public async Task<LLMCompletionResponse> GetCompletion(string instanceId, LLMCompletionRequest request)
        {
            var client = await _httpClientFactoryService.CreateClient(_serviceName, _userIdentity);
            var pollingClient = new PollingHttpClient<LLMCompletionRequest, LLMCompletionResponse>(
                client,
                request,
                $"instances/{instanceId}/async-completions",
                TimeSpan.FromSeconds(10),
                client.Timeout.Subtract(TimeSpan.FromSeconds(1)),
                _logger);

            try
            {
                var completionResponse = await pollingClient.ExecuteOperationAsync()
                    ?? throw new Exception("The LangChain orchestration service did not return a valid completion response.");

                if (completionResponse != null)
                {
                    return new LLMCompletionResponse
                    {
                        OperationId = request.OperationId!,
                        Content = completionResponse!.Content,
                        Completion = completionResponse.Completion,
                        Citations = completionResponse.Citations,
                        UserPrompt = completionResponse.UserPrompt,
                        FullPrompt = completionResponse.FullPrompt,
                        PromptTemplate = string.Empty,
                        AgentName = request.Agent.Name,
                        PromptTokens = completionResponse.PromptTokens,
                        CompletionTokens = completionResponse.CompletionTokens,
                        AnalysisResults = completionResponse.AnalysisResults
                    };
                }

                _logger.LogWarning("The orchestration service was not able to return a response.");

                return new LLMCompletionResponse
                {
                    OperationId = request.OperationId!,
                    Completion = "A problem on my side prevented me from responding.",
                    UserPrompt = request.UserPrompt,
                    PromptTemplate = string.Empty,
                    AgentName = request.Agent.Name,
                    PromptTokens = 0,
                    CompletionTokens = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while executing the completion request against the {ServiceName} orchestration service.",
                    _serviceName);

                return new LLMCompletionResponse
                {
                    OperationId = request.OperationId!,
                    Completion = "A problem on my side prevented me from responding.",
                    UserPrompt = request.UserPrompt,
                    PromptTemplate = string.Empty,
                    AgentName = request.Agent.Name,
                    PromptTokens = 0,
                    CompletionTokens = 0
                };
            }
        }

        /// <inheritdoc/>
        public async Task<LongRunningOperation> StartCompletionOperation(string instanceId, LLMCompletionRequest completionRequest)
        {
            var pollingClient = await GetPollingClient(instanceId, completionRequest);
            return await pollingClient.StartOperationAsync();
        }

        /// <inheritdoc/>
        public async Task<LongRunningOperation> GetCompletionOperationStatus(string instanceId, string operationId)
        {
            var pollingClient = await GetPollingClient(instanceId);
            return await pollingClient.GetOperationStatusAsync(operationId);
        }

        private async Task<PollingHttpClient<LLMCompletionRequest, LLMCompletionResponse>> GetPollingClient(
            string instanceId,
            LLMCompletionRequest? request = null)
        {
            var client = await _httpClientFactoryService.CreateClient(_serviceName, _userIdentity);

            return new PollingHttpClient<LLMCompletionRequest, LLMCompletionResponse>(
                client,
                request,
                $"instances/{instanceId}/async-completions",
                TimeSpan.FromSeconds(10),
                client.Timeout.Subtract(TimeSpan.FromSeconds(1)),
                _logger);
        }
    }
}
