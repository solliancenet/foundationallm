using Azure.Core;
using FoundationaLLM.Client.Core.Interfaces;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Configuration.API;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;

namespace FoundationaLLM.Client.Core
{
    /// <inheritdoc/>
    public class CoreClient : ICoreClient
    {
        private readonly ICoreRESTClient _coreRestClient;

        /// <summary>
        /// Constructor for mocking. This does not initialize the clients.
        /// </summary>
        public CoreClient() =>
            _coreRestClient = null!;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreClient"/> class with
        /// the specified Core API URI and TokenCredential.
        /// </summary>
        /// <param name="coreUri">The base URI of the Core API.</param>
        /// <param name="credential">A <see cref="TokenCredential"/> of an authenticated
        /// user or service principle from which the client library can generate auth tokens.</param>
        public CoreClient(string coreUri, TokenCredential credential)
            : this(coreUri, credential, new APIClientSettings()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreClient"/> class with
        /// the specified Core API URI, TokenCredential, and optional client settings.
        /// </summary>
        /// <param name="coreUri">The base URI of the Core API.</param>
        /// <param name="credential">A <see cref="TokenCredential"/> of an authenticated
        /// user or service principle from which the client library can generate auth tokens.</param>
        /// <param name="options">Additional options to configure the HTTP Client.</param>
        public CoreClient(string coreUri, TokenCredential credential, APIClientSettings options) =>
            _coreRestClient = new CoreRESTClient(coreUri, credential, options);

        /// <inheritdoc/>
        public async Task<string> CreateChatSessionAsync(string? sessionName)
        {
            var sessionId = await _coreRestClient.Sessions.CreateSessionAsync();
            if (!string.IsNullOrWhiteSpace(sessionName))
            {
                await _coreRestClient.Sessions.RenameChatSession(sessionId, sessionName);
            }

            return sessionId;
        }

        /// <inheritdoc/>
        public async Task<Completion> GetCompletionWithSessionAsync(string? sessionId, string? sessionName,
            string userPrompt, string agentName)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                sessionId = await CreateChatSessionAsync(sessionName);
            }

            var orchestrationRequest = new CompletionRequest
            {
                AgentName = agentName,
                SessionId = sessionId,
                UserPrompt = userPrompt
            };
            return await GetCompletionWithSessionAsync(orchestrationRequest);
        }

        /// <inheritdoc/>
        public async Task<Completion> GetCompletionWithSessionAsync(CompletionRequest completionRequest)
        {
            if (string.IsNullOrWhiteSpace(completionRequest.SessionId) ||
                string.IsNullOrWhiteSpace(completionRequest.AgentName) ||
                string.IsNullOrWhiteSpace(completionRequest.UserPrompt))
            {
                throw new ArgumentException("The completion request must contain a SessionID, AgentName, and UserPrompt at a minimum.");
            }

            var completion = await GetCompletionAsync(completionRequest);
            return completion;
        }

        /// <inheritdoc/>
        public async Task<Completion> GetCompletionAsync(string userPrompt, string agentName)
        {
            var completionRequest = new CompletionRequest
            {
                AgentName = agentName,
                UserPrompt = userPrompt
            };

            return await GetCompletionAsync(completionRequest);
        }

        /// <inheritdoc/>
        public async Task<Completion> GetCompletionAsync(CompletionRequest completionRequest)
        {
            if (string.IsNullOrWhiteSpace(completionRequest.AgentName) ||
                string.IsNullOrWhiteSpace(completionRequest.UserPrompt))
            {
                throw new ArgumentException("The completion request must contain an AgentName and UserPrompt at a minimum.");
            }

            var completion = await _coreRestClient.Completions.GetChatCompletionAsync(completionRequest);
            return completion;
        }

        /// <inheritdoc/>
        public async Task<Completion> AttachFileAndAskQuestionAsync(Stream fileStream, string fileName, string contentType,
            string agentName, string question, bool useSession, string? sessionId, string? sessionName)
        {
            if (fileStream == null)
            {
                throw new ArgumentNullException(nameof(fileStream));
            }

            var objectId = await _coreRestClient.Attachments.UploadAttachmentAsync(fileStream, fileName, contentType);

            if (useSession)
            {
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    sessionId = await CreateChatSessionAsync(sessionName);
                }

                var orchestrationRequest = new CompletionRequest
                {
                    AgentName = agentName,
                    SessionId = sessionId,
                    UserPrompt = question,
                    Attachments = [objectId]
                };
                var sessionCompletion = await GetCompletionAsync(orchestrationRequest);

                return sessionCompletion;
            }

            // Use the orchestrated completion request to ask a question about the file.
            var completionRequest = new CompletionRequest
            {
                AgentName = agentName,
                UserPrompt = question,
                Attachments = [objectId]
            };
            var completion = await _coreRestClient.Completions.GetChatCompletionAsync(completionRequest);

            return completion;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Message>> GetChatSessionMessagesAsync(string sessionId) => await _coreRestClient.Sessions.GetChatSessionMessagesAsync(sessionId);

        /// <inheritdoc/>
        public async Task<IEnumerable<ResourceProviderGetResult<AgentBase>>> GetAgentsAsync()
        {
            var agents = await _coreRestClient.Completions.GetAgentsAsync();
            return agents;
        }

        /// <inheritdoc/>
        public async Task DeleteSessionAsync(string sessionId) => await _coreRestClient.Sessions.DeleteSessionAsync(sessionId);
    }
}
