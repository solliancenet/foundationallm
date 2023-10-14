using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Core.Interfaces;

public interface IPromptHubService
{
    Task<string> Status();

    Task<PromptHubResponse> ResolveRequest(string user_prompt, string user_context);
}
