using FoundationaLLM.Common.Models.Infrastructure;

namespace FoundationaLLM.Client.Management
{
    public interface IManagementRESTClient
    {
        /// <summary>
        /// Gets service status information for the Management API
        /// </summary>
        /// <returns></returns>
        Task<ServiceStatusInfo> GetServiceStatusAsync();
        /// <summary>
        /// Checks whether calls to the Management API are authenticated
        /// </summary>
        /// <returns></returns>
        Task<bool> IsAuthenticatedAsync();
        Task<>
    }
}
