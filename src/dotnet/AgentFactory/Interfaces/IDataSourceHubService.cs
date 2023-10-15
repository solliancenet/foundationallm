using FoundationaLLM.AgentFactory.Core.Models.Messages;
namespace FoundationaLLM.AgentFactory.Core.Interfaces;

/// <summary>
/// Interface for the Agent Factory Service
/// </summary>
public interface IDataSourceHubService
{

    Task<string> Status();

    Task<DataSourceHubResponse> ResolveRequest(List<string> sources, string userContext);
}
