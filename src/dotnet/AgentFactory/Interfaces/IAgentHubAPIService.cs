using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Core.Interfaces;

/// <summary>
/// Interface for the AgentHub Service
/// </summary>
public interface IAgentHubAPIService
{
    /// <summary>
    /// Gets the status of the Agent Hub Service
    /// </summary>
    /// <returns></returns>
    Task<string> Status();

    /// <summary>
    /// Calls the target Agent Hub and resolves a user prompt with the user's context
    /// </summary>
    /// <param name="userPrompt"></param>
    /// <param name="userContext"></param>
    /// <returns></returns>
    Task<AgentHubResponse> ResolveRequest(string userPrompt, string userContext);
}
