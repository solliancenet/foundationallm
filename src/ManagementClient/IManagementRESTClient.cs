using FoundationaLLM.Common.Models.Infrastructure;

namespace FoundationaLLM.Client.Management
{
    public interface IManagementRESTClient
    {
        /// <summary>
        /// Gets service status information for the Management API
        /// </summary>
        /// <returns></returns>
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
        /// <returns></returns>
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
        Task<T> UpsertResource<T>(T resource, string instanceId, string resourceProvider, string resourcePath, string accessToken);
        /// <summary>
        /// Deletes a resource identified by the input parameters
        /// </summary>
        /// <param name="instanceId">The instance Id of the resource.</param>
        /// <param name="resourceProvider">The resource provider for the resource type being deleted</param>
        /// <param name="resourcePath">The path given to the resource provider to locate and delete the resource</param>
        /// <returns></returns>
        Task DeleteResource(string instanceId, string resourceProvider, string resourcePath, string accessToken);
    }
}
