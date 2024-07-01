using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;

namespace FoundationaLLM.Client.Core.Interfaces
{
    /// <summary>
    /// Provides methods to manage calls to the Core API's completions endpoints.
    /// </summary>
    public interface ICompletionRESTClient
    {
        /// <summary>
        /// Performs a completion request to the Core API. For session-based requests, set the
        /// <see cref="CompletionRequest.SessionId"/> property to the unique identifier for the session.
        /// Set the property to <see langword="null"/> or empty for non-session-based requests.
        /// Session-less completion requests do not maintain message history or data retention in Cosmos DB.
        /// </summary>
        /// <param name="completionRequest">The completion request data sent to the endpoint.</param>
        /// <returns></returns>
        Task<Completion> GetChatCompletionAsync(CompletionRequest completionRequest);

        /// <summary>
        /// Retrieves agents available to the user for orchestration and session-based requests.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ResourceProviderGetResult<AgentBase>>> GetAgentsAsync();
    }
}
