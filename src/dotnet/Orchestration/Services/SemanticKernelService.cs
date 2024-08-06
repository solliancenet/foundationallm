using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Infrastructure;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Orchestration.Core.Interfaces;
using FoundationaLLM.Orchestration.Core.Models.ConfigurationOptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Orchestration.Core.Services
{
    /// <summary>
    /// The FoundationaLLM SemanticKernel Service.
    /// </summary>
    /// <remarks>
    /// Constructor for the SemanticKernel Service.
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
        private readonly ICallContext _callContext = callContext;
        private readonly IHttpClientFactoryService _httpClientFactoryService = httpClientFactoryService;
        readonly JsonSerializerOptions _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();

        /// <inheritdoc/>
        public async Task<ServiceStatusInfo> GetStatus(string instanceId)
        {
            var client = await _httpClientFactoryService.CreateClient(HttpClientNames.SemanticKernelAPI, _callContext.CurrentUserIdentity);
            var responseMessage = await client.SendAsync(
                new HttpRequestMessage(HttpMethod.Get, $"instances/{instanceId}/status"));

            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ServiceStatusInfo>(responseContent)!;
        }

        /// <inheritdoc/>
        public string Name => LLMOrchestrationServiceNames.SemanticKernel;

        /// <summary>
        /// Gets a completion from the Semantic Kernel service.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance ID.</param>
        /// <param name="request">Request object populated from the hub APIs including agent, prompt, data source, and model information.</param>
        /// <returns>Returns a completion response from the orchestration engine.</returns>
        public async Task<LLMCompletionResponse> GetCompletion(string instanceId, LLMCompletionRequest request)
        {
            var client = await _httpClientFactoryService.CreateClient(HttpClientNames.SemanticKernelAPI, _callContext.CurrentUserIdentity);

            var body = JsonSerializer.Serialize(request, _jsonSerializerOptions);
            var responseMessage = await client.PostAsync($"instances/{instanceId}/orchestration/completion",
                new StringContent(
                    body,
                    Encoding.UTF8, "application/json"));
            var responseContent = await responseMessage.Content.ReadAsStringAsync();

            if (responseMessage.IsSuccessStatusCode)
            {
                var completionResponse = JsonSerializer.Deserialize<LLMCompletionResponse>(responseContent);

                return new LLMCompletionResponse
                {
                    OperationId = request.OperationId,
                    Completion = completionResponse!.Completion,
                    UserPrompt = completionResponse.UserPrompt,
                    FullPrompt = completionResponse.FullPrompt,
                    PromptTemplate = string.Empty,
                    AgentName = request.Agent.Name,
                    PromptTokens = completionResponse.PromptTokens,
                    CompletionTokens = completionResponse.CompletionTokens
                };
            }

            _logger.LogWarning("The Semantic Kernel orchestration service returned status code {StatusCode}: {ResponseContent}",
                responseMessage.StatusCode, responseContent);

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
