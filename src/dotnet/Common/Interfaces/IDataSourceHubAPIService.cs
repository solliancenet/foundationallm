using FoundationaLLM.Common.Models.Messages;

namespace FoundationaLLM.Common.Interfaces;

/// <summary>
/// Interface for the Agent Factory Service.
/// </summary>
public interface IDataSourceHubAPIService : IHubAPIService, ICacheControlAPIService
{
    /// <summary>
    /// Gets a list of DataSources from the DataSource Hub.
    /// </summary>
    /// <param name="sources">The data sources to resolve.</param>
    /// <param name="sessionId">The session ID.</param>
    /// <returns></returns>
    Task<DataSourceHubResponse> ResolveRequest(List<string> sources, string sessionId);
}
