using FoundationaLLM.Common.Models.Infrastructure;

namespace FoundationaLLM.Client.Management.Interfaces
{
    /// <summary>
    /// Provides methods to manage calls to the Management API's status endpoints.
    /// </summary>
    public interface IStatusRESTClient
    {
        /// <summary>
        /// Returns the status of the Management API service. A token is not required since
        /// the status endpoint supports anonymous access.
        /// </summary>
        /// <returns></returns>
        Task<ServiceStatusInfo> GetServiceStatusAsync();

        /// <summary>
        /// Checks whether the requester is authenticated and allowed to execute
        /// requests against the Management API service.
        /// </summary>
        Task<bool> IsAuthenticatedAsync();
    }
}
