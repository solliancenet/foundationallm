using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Agents;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Infrastructure;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.Direct;
using FoundationaLLM.Common.Models.Orchestration.Request;
using FoundationaLLM.Common.Models.Orchestration.Response;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Orchestration.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Orchestration.Core.Services
{
    /// <summary>
    /// The Azure OpenAI direct orchestration service.
    /// </summary>
    /// <param name="logger">The logger used for logging.</param>
    /// <param name="callContext">Stores context information extracted from the current HTTP request. This information
    /// is primarily used to inject HTTP headers into downstream HTTP calls.</param>
    /// <param name="httpClientFactoryService">The HTTP client factory service.</param>
    /// <param name="resourceProviderServices">A dictionary of <see cref="IResourceProviderService"/> resource providers hashed by resource provider name.</param>
    public class AzureOpenAIDirectService(
        ILogger<AzureOpenAIDirectService> logger,
        ICallContext callContext,
        IHttpClientFactoryService httpClientFactoryService,
        IEnumerable<IResourceProviderService> resourceProviderServices) : IAzureOpenAIDirectService
    {
        private readonly ILogger<AzureOpenAIDirectService> _logger = logger;
        private readonly ICallContext _callContext = callContext;
        private readonly IHttpClientFactoryService _httpClientFactoryService = httpClientFactoryService;
        private readonly JsonSerializerOptions _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();
        private readonly Dictionary<string, IResourceProviderService> _resourceProviderServices = resourceProviderServices.ToDictionary(
                rps => rps.Name);

        /// <inheritdoc/>
        public async Task<ServiceStatusInfo> GetStatus(string instanceId) =>
            await Task.FromResult(new ServiceStatusInfo
            {
                InstanceId = instanceId,
                Name = Name,
                Status = "ready",
            });

        /// <inheritdoc/>
        public string Name => LLMOrchestrationServiceNames.AzureOpenAIDirect;

        /// <inheritdoc/>
        public async Task<LLMCompletionResponse> GetCompletion(string instanceId, LLMCompletionRequest request)
        {
            request.Validate();

            var endpointConfiguration = request.AIModelEndpointConfiguration;
            var inputStrings = new List<CompletionMessage>();
            SystemCompletionMessage? systemPrompt = null;

            if (!string.IsNullOrWhiteSpace(endpointConfiguration.OperationType)
                && endpointConfiguration.OperationType == OperationTypes.Chat)
            {
                inputStrings.Add(new SystemCompletionMessage
                {
                    Role = InputMessageRoles.System,
                    Content = request.Prompt.Prefix ?? string.Empty
                });

                // Add conversation history.
                if (request.Agent.ConversationHistorySettings?.Enabled == true && request.MessageHistory != null)
                {
                    // The message history needs to be in a continuous order of user and assistant messages.
                    // If the MaxHistory value is odd, add one to the number of messages to take to ensure proper pairing.
                    if (request.Agent.ConversationHistorySettings.MaxHistory % 2 != 0)
                        request.Agent.ConversationHistorySettings.MaxHistory++;

                    var messageHistoryItems = request.MessageHistory?.TakeLast(request.Agent.ConversationHistorySettings.MaxHistory);
                    foreach (var item in messageHistoryItems!)
                    {
                        inputStrings.Add(new CompletionMessage
                        {
                            Role = item.Sender.ToLower(),
                            Content = item.Text
                        });
                    }
                }
                // Add current user prompt.
                var userPrompt = new UserCompletionMessage { Content = request.UserPrompt };
                inputStrings.Add(userPrompt);
            }

            var client = await _httpClientFactoryService.CreateClient(endpointConfiguration, _callContext.CurrentUserIdentity);

            var modelParameters = request.AIModel.ModelParameters;

            var azureOpenAIDirectRequest = modelParameters.ToObject<AzureOpenAICompletionRequest>();
            var chatOperation = string.Empty;

            switch (endpointConfiguration.OperationType)
            {
                case OperationTypes.Completions:
                    azureOpenAIDirectRequest.Prompt = request.UserPrompt;
                    break;
                case OperationTypes.Chat:
                    chatOperation = "/chat";
                    azureOpenAIDirectRequest.Messages = [.. inputStrings];
                    break;
            }

            var body = JsonSerializer.Serialize(azureOpenAIDirectRequest, _jsonSerializerOptions);
            var content = new StringContent(body, Encoding.UTF8, "application/json");

            var responseMessage = await client.PostAsync($"/openai/deployments/{request.AIModel.DeploymentName}{chatOperation}/completions?api-version={endpointConfiguration.APIVersion}", content);
            var responseContent = await responseMessage.Content.ReadAsStringAsync();

            if (responseMessage.IsSuccessStatusCode)
            {
                var completionResponse = JsonSerializer.Deserialize<AzureOpenAICompletionResponse>(responseContent);

                return new LLMCompletionResponse
                {
                    OperationId = request.OperationId,
                    Completion = !string.IsNullOrEmpty(endpointConfiguration.OperationType) && endpointConfiguration.OperationType == OperationTypes.Chat
                        ? completionResponse!.Choices?[0].Message?.Content
                        : completionResponse!.Choices?[0].Text,
                    UserPrompt = request.UserPrompt,
                    FullPrompt = body,
                    PromptTemplate = systemPrompt?.Content,
                    AgentName = request.Agent.Name,
                    PromptTokens = completionResponse!.Usage!.PromptTokens,
                    CompletionTokens = completionResponse!.Usage!.CompletionTokens
                };
            }

            _logger.LogWarning("The AzureOpenAIDirect orchestration service returned status code {StatusCode}: {ResponseContent}",
                responseMessage.StatusCode, responseContent);

            return new LLMCompletionResponse
            {
                OperationId = request.OperationId,
                Completion = "A problem on my side prevented me from responding.",
                UserPrompt = request.UserPrompt,
                PromptTemplate = systemPrompt?.Content,
                AgentName = request.Agent.Name,
                PromptTokens = 0,
                CompletionTokens = 0
            };
        }

        /// <inheritdoc/>
        public Task<LongRunningOperation> StartCompletionOperation(string instanceId, LLMCompletionRequest completionRequest) => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task<LongRunningOperation> GetCompletionOperationStatus(string instanceId, string operationId) => throw new NotImplementedException();
    }
}
