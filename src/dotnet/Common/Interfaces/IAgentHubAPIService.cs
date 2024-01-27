using FoundationaLLM.Common.Models.Messages;

namespace FoundationaLLM.Common.Interfaces;

/// <summary>
/// Interface for the AgentHub Service.
/// </summary>
public interface IAgentHubAPIService : IHubAPIService, ICacheControlAPIService
{
    /// <summary>
    /// Gets a set of agents from the Agent Hub based on the prompt and user context.
    /// </summary>
    /// <param name="userPrompt">The user prompt to resolve.</param>
    /// <param name="sessionId">The session ID.</param>
    /// <param name="agentHintOverride">Override the agent hint passed in the header.
    /// This is useful when warming the cache since the agent hint will not exist within
    /// a request context.</param>
    /// <returns></returns>
    Task<AgentHubResponse> ResolveRequest(string userPrompt, string sessionId,
        string? agentHintOverride = null);

    /// <summary>
    /// Gets the list with all the agent names and descriptions.
    /// </summary>
    /// <returns>A list of <see cref="AgentMetadata"/> objects containing the names and descriptions of the agents.</returns>
    Task<List<AgentMetadata>> ListAgents();
}
