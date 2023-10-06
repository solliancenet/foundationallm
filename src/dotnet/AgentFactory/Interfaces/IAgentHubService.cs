using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Core.Interfaces;

public interface IAgentHubService
{
    Task Ping();

    Task<string> Status();

    Task<List<AgentHubResponse>> ResolveRequest(string name, string body, string user_prompt, string user_context);
}
