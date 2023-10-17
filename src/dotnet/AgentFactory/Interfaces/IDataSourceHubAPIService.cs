using FoundationaLLM.AgentFactory.Core.Models.Messages;
namespace FoundationaLLM.AgentFactory.Core.Interfaces;

/// <summary>
/// Interface for the Agent Factory Service
/// </summary>
public interface IDataSourceHubAPIService
{
    /// <summary>
    /// Gets the status of the DataSource Hub Service
    /// </summary>
    /// <returns></returns>
    Task<string> Status();

    /// <summary>
    /// Calls the target DataSource Hub to retrieve a list of data sources.  Input will typically come from the AgentHub response.
    /// </summary>
    /// <param name="sources"></param>
    /// <param name="userContext"></param>
    /// <returns></returns>
    Task<DataSourceHubResponse> ResolveRequest(List<string> sources, string userContext);
}
