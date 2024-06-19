using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Core.Examples.Exceptions;
using FoundationaLLM.Core.Examples.Interfaces;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace FoundationaLLM.Core.Examples.Services
{
    /// <inheritdoc/>
    public class CoreAPITestManager(IHttpClientManager httpClientManager) : ICoreAPITestManager
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();

        /// <inheritdoc/>
        public async Task<string> CreateSessionAsync()
        {
            var coreClient = await httpClientManager.GetHttpClientAsync(HttpClients.CoreAPI);
            var responseSession = await coreClient.PostAsync("sessions", null);

            if (responseSession.IsSuccessStatusCode)
            {
                var responseContent = await responseSession.Content.ReadAsStringAsync();
                var sessionResponse = JsonSerializer.Deserialize<Session>(responseContent, _jsonSerializerOptions);
                var sessionId = string.Empty;
                if (sessionResponse?.SessionId != null)
                {
                    sessionId = sessionResponse.SessionId;
                }

                var sessionName = "Test: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                await coreClient.PostAsync($"sessions/{sessionId}/rename?newChatSessionName={UrlEncoder.Default.Encode(sessionName)}", null);

                return sessionId;
            }
            
            throw new FoundationaLLMException($"Failed to create a new chat session. Status code: {responseSession.StatusCode}. Reason: {responseSession.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task<Completion> SendSessionCompletionRequestAsync(CompletionRequest completionRequest)
        {
            var coreClient = await httpClientManager.GetHttpClientAsync(HttpClients.CoreAPI);
            var serializedRequest = JsonSerializer.Serialize(completionRequest, _jsonSerializerOptions);

            var sessionUrl = $"sessions/{completionRequest.SessionId}/completion"; // Session-based - message history and data is retained in Cosmos DB. Must create a session if it does not exist.
            var responseMessage = await coreClient.PostAsync(sessionUrl,
                new StringContent(
                    serializedRequest,
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var completionResponse =
                    JsonSerializer.Deserialize<Completion>(responseContent, _jsonSerializerOptions);
                return completionResponse ?? throw new InvalidOperationException("The returned completion response is invalid.");
            }

            throw new FoundationaLLMException($"Failed to send completion request. Status code: {responseMessage.StatusCode}. Reason: {responseMessage.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task<CompletionPrompt> GetCompletionPromptAsync(string sessionId, string completionPromptId)
        {
            var coreClient = await httpClientManager.GetHttpClientAsync(HttpClients.CoreAPI);
            var responseMessage = await coreClient.GetAsync($"sessions/{sessionId}/completionprompts/{completionPromptId}");

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var completionPrompt =
                    JsonSerializer.Deserialize<CompletionPrompt>(responseContent, _jsonSerializerOptions);
                return completionPrompt ?? throw new InvalidOperationException("The returned completion prompt is invalid.");
            }

            throw new FoundationaLLMException($"Failed to get completion prompt. Status code: {responseMessage.StatusCode}. Reason: {responseMessage.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Message>> GetChatSessionMessagesAsync(string sessionId)
        {
            var coreClient = await httpClientManager.GetHttpClientAsync(HttpClients.CoreAPI);
            var responseMessage = await coreClient.GetAsync($"sessions/{sessionId}/messages");

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var messages = JsonSerializer.Deserialize<IEnumerable<Message>>(responseContent, _jsonSerializerOptions);
                return messages ?? throw new InvalidOperationException("The returned messages are invalid.");
            }

            throw new FoundationaLLMException($"Failed to get chat session messages. Status code: {responseMessage.StatusCode}. Reason: {responseMessage.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task<Completion> SendOrchestrationCompletionRequestAsync(CompletionRequest completionRequest)
        {
            var coreClient = await httpClientManager.GetHttpClientAsync(HttpClients.CoreAPI);
            var serializedRequest = JsonSerializer.Serialize(completionRequest, _jsonSerializerOptions);

            var responseMessage = await coreClient.PostAsync("orchestration/completion", // Session-less - no message history or data retention in Cosmos DB.
                new StringContent(
                    serializedRequest,
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var completionResponse =
                    JsonSerializer.Deserialize<Completion>(responseContent, _jsonSerializerOptions);
                return completionResponse ?? throw new InvalidOperationException("The returned completion response is invalid.");
            }

            throw new FoundationaLLMException($"Failed to send completion request. Status code: {responseMessage.StatusCode}. Reason: {responseMessage.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task DeleteSessionAsync(string sessionId)
        {
            var coreClient = await httpClientManager.GetHttpClientAsync(HttpClients.CoreAPI);
            await coreClient.DeleteAsync($"sessions/{sessionId}");
        }
    }
}
