using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Core.Interfaces;

public interface IAgentHubService
{
    Task<string> Status();

    Task<AgentHubResponse> ResolveRequest(string userPrompt, string userContext);
}
