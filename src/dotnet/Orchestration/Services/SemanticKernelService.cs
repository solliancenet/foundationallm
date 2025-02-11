using FoundationaLLM.Common.Authentication;
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
using FoundationaLLM.Orchestration.Core.Models.ConfigurationOptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FoundationaLLM.Orchestration.Core.Services
{
    /// <summary>
    /// The FoundationaLLM Semantic Kernal Service
    /// </summary>
    /// <remarks>
    /// Constructor for the Semantic Kernal Service
    /// </remarks>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    /// <param name="callContext"></param>
    /// <param name="httpClientFactoryService"></param>
    public class SemanticKernelService(
        IOptions<SemanticKernelServiceSettings> options,
        ILogger<SemanticKernelService> logger,
        ICallContext callContext,
        IHttpClientFactoryService httpClientFactoryService) : ISemanticKernelService
    {
        readonly SemanticKernelServiceSettings _settings = options.Value;
        readonly ILogger<SemanticKernelService> _logger = logger;

        private readonly UnifiedUserIdentity? _userIdentity = callContext.CurrentUserIdentity;
        private readonly IHttpClientFactoryService _httpClientFactoryService = httpClientFactoryService;
        readonly JsonSerializerOptions _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();

        /// <inheritdoc/>
        public async Task<ServiceStatusInfo> GetStatus(string instanceId)
        {
            var client = await _httpClientFactoryService.CreateClient(HttpClientNames.SemanticKernelAPI, ServiceContext.ServiceIdentity!, true);
            // Set the requestUri value to empty since we requested the status endpoint for this service.
            var responseMessage = await client.SendAsync(
                new HttpRequestMessage(HttpMethod.Get, ""));

            if (!responseMessage.IsSuccessStatusCode)
            {
                return new ServiceStatusInfo
                {
                    Name = HttpClientNames.SemanticKernelAPI,
                    Status = ServiceStatuses.Error,
                    Message = "The Semantic Kernel orchestration service is unavailable."
                };
            }

            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ServiceStatusInfo>(responseContent)!;
        }

        /// <inheritdoc/>
        public string Name => LLMOrchestrationServiceNames.SemanticKernel;

        /// <inheritdoc/>
        public async Task<LLMCompletionResponse> GetCompletion(string instanceId, LLMCompletionRequest request)
        {
            var pollingClient = await GetPollingClient(instanceId, request);

            try
            {
                var completionResponse = await pollingClient.ExecuteOperationAsync()
                    ?? throw new Exception("The Semantic Kernel orchestration service did not return a valid completion response.");
                return new LLMCompletionResponse
                {
                    OperationId = request.OperationId!,
                    Completion = completionResponse!.Completion,
                    ContentArtifacts = completionResponse.ContentArtifacts,
                    UserPrompt = completionResponse.UserPrompt,
                    FullPrompt = completionResponse.FullPrompt,
                    PromptTemplate = string.Empty,
                    AgentName = request.Agent.Name,
                    PromptTokens = completionResponse.PromptTokens,
                    CompletionTokens = completionResponse.CompletionTokens
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while executing the completion request against the {ServiceName} orchestration service.",
                    HttpClientNames.SemanticKernelAPI);

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
            if (_userIdentity == null)
            {
                throw new ArgumentException("The provided call context does not have a valid user identity.");
            }
            var operationStarterClient = await _httpClientFactoryService.CreateClient(HttpClientNames.LangChainAPI, _userIdentity);
            var operationRetrieverClient = await _httpClientFactoryService.CreateClient(HttpClientNames.StateAPI, _userIdentity);

            return new PollingHttpClient<LLMCompletionRequest, LLMCompletionResponse>(
                operationStarterClient,
                operationRetrieverClient,
                request,
                $"instances/{instanceId}/async-completions",
                $"instances/{instanceId}/operations/{{0}}",
                TimeSpan.FromSeconds(10),
                operationStarterClient.Timeout.Subtract(TimeSpan.FromSeconds(1)),
                _logger);
        }
    }
}
