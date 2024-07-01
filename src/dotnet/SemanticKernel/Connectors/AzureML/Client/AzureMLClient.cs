using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.SemanticKernel;
using FoundationaLLM.SemanticKernel.Core.Connectors.AzureML.Models;

namespace FoundationaLLM.SemanticKernel.Core.Connectors.AzureML.Client
{
    /// <summary>
    /// The AzureML client.
    /// </summary>
    public class AzureMLClient
    {
        private readonly string? _deploymentName;
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly ILogger? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureMLClient"/> class.
        /// </summary>
        /// <param name="deploymentName">The name of the AzureML deployment.</param>
        /// <param name="endpoint">The endpoint of the AzureML service.</param>
        /// <param name="apiKey">The API Key of the AzureML service</param>        
        /// <param name="logger"></param>
        /// <exception cref="ArgumentException"></exception>
        public AzureMLClient(string endpoint, string apiKey, string? deploymentName, ILogger? logger = null)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));
            }
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("API Key cannot be null or empty", nameof(apiKey));
            }
            _deploymentName = deploymentName;
            _endpoint = endpoint;
            _apiKey = apiKey;
        }

        /// <summary>
        /// Generates a chat message asynchronously.
        /// </summary>
        /// <param name="chatHistory">The chat history containing the conversation data.</param>
        /// <param name="executionSettings">Optional settings for prompt execution.</param>
        /// <param name="kernel">A kernel instance.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>Returns a list of chat message contents.</returns>
        public async Task<IReadOnlyList<ChatMessageContent>> GenerateChatMessageAsync(
            ChatHistory chatHistory,
            PromptExecutionSettings? executionSettings = null,
            Kernel? kernel = null,
            CancellationToken cancellationToken = default)
        {
            ValidateChatHistory(chatHistory);
            var azureMLExecutionSettings = AzureMLPromptExecutionSettings.FromExecutionSettings(executionSettings);
            var azureMLRequest = AzureMLChatRequest.FromChatHistoryAndExecutionSettings(chatHistory, azureMLExecutionSettings);
            using (var httpClient = new HttpClient())
            {
                using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, _endpoint))
                {
                    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                    var body = JsonSerializer.Serialize(azureMLRequest);
                    httpRequest.Content = new StringContent(body);
                    httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken);
                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException($"Request failed with status code {httpResponse.StatusCode} - {httpResponse.ReasonPhrase}");
                    }

                    var responseContent = await httpResponse.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<AzureMLChatResponse>(responseContent);
                    if (response is null)
                    {
                        throw new InvalidOperationException("Failed to deserialize the response from the AzureML service");
                    }
                    var messageContent = new ChatMessageContent(AuthorRole.Assistant, response.Output);
                    azureMLRequest.AddChatMessage(messageContent);
                    chatHistory.AddAssistantMessage(response.Output);
                    return [messageContent];
                }
            }
        }

        /// <summary>
        /// Messages are required and the first prompt role should be user or system.
        /// </summary>
        private void ValidateChatHistory(ChatHistory chatHistory)
        {
            if (chatHistory == null)
            {
                throw new ArgumentNullException(nameof(chatHistory));
            }

            if (chatHistory.Count == 0)
            {
                throw new ArgumentException("Chat history must contain at least one message", nameof(chatHistory));
            }
            var firstRole = chatHistory[0].Role.ToString();
            if (firstRole is not "system" and not "user")
            {
                throw new ArgumentException("The first message in chat history must have either the system or user role", nameof(chatHistory));
            }
        }
    }
}
