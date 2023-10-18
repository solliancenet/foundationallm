using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Core.Interfaces;

/// <summary>
/// Interface for a prompt hub service
/// </summary>
public interface IPromptHubAPIService
{
    /// <summary>
    /// Gets the status of the Prompt Hub Service
    /// </summary>
    /// <returns></returns>
    Task<string> Status();

    /// <summary>
    /// Gets an agent based on its name.  Passes the userContext such that the prompt is built with any user based attributes.
    /// </summary>
    /// <param name="agentName"></param>
    /// <param name="userContext"></param>
    /// <returns></returns>
    Task<PromptHubResponse> ResolveRequest(string agentName, string userContext);
}
