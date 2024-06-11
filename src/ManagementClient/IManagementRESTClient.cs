using FoundationaLLM.Common.Models.Infrastructure;

namespace FoundationaLLM.Client.Management
{
    /// <summary>
    /// Low level REST API client for making direct calls to the Management API
    /// </summary>
    public interface IManagementRESTClient
    {
        /// <summary>
        /// Gets service status information for the Management API
        /// </summary>
        /// <returns>Returns the ServiceStatusInfo for the management API</returns>
        Task<ServiceStatusInfo> GetServiceStatusAsync(string accessToken);
        /// <summary>
        /// Checks whether calls to the Management API are authenticated
        /// </summary>
        /// <returns></returns>
        Task<bool> IsAuthenticatedAsync(string accessToken);
        /// <summary>
        /// Retrieves a collection of resources of a given type
        /// from the resource provider for a specific resource type
        /// </summary>
        /// <typeparam name="T">The resource type</typeparam>
        /// <param name="instanceId">The instance Id of the resource.</param>
        /// <param name="resourceProvider">The resource provider for the resource type being upserted</param>
        /// <param name="resourcePath">The path given to the resource provider to locate and update the resource</param>
        /// <param name="accessToken">JWT token for authenticating calls</param>
        /// <returns>A list of the resources</returns>
        Task<List<T>> GetResources<T>(string instanceId, string resourceProvider, string resourcePath, string accessToken);
        /// <summary>
        /// Upserts a resource
        /// </summary>
        /// <typeparam name="T">The resource type</typeparam>
        /// <param name="resource">The resource object to insert or update.</param>
        /// <returns>The persisted resource</returns>
        /// <param name="instanceId">The instance Id of the resource.</param>
        /// <param name="resourceProvider">The resource provider for the resource type being upserted</param>
        /// <param name="resourcePath">The path given to the resource provider to locate and update the resource</param>
        /// <param name="accessToken">JWT token for authenticating calls</param>
        Task<T> UpsertResource<T>(T resource, string instanceId, string resourceProvider, string resourcePath, string accessToken);
        /// <summary>
        /// Deletes a resource identified by the input parameters
        /// </summary>
        /// <param name="instanceId">The instance Id of the resource.</param>
        /// <param name="resourceProvider">The resource provider for the resource type being upserted</param>
        /// <param name="resourcePath">The path given to the resource provider to locate and update the resource</param>
        /// <param name="accessToken">JWT token for authenticating calls</param>
        Task DeleteResource(string instanceId, string resourceProvider, string resourcePath, string accessToken);
    }
}
