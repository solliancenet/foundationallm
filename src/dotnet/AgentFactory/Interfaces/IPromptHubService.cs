using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Core.Interfaces;

/// <summary>
/// IPromptHubService interface
/// </summary>
public interface IPromptHubService
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
