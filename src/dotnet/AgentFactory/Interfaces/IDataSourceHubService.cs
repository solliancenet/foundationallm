using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.AgentFactory.Core.Interfaces;

/// <summary>
/// Interface for the Agent Factory Service
/// </summary>
public interface IDataSourceHubService
{

    Task<string> Status();

    Task<DataSourceHubResponse> ResolveRequest(List<string> sources, string user_context);
}
