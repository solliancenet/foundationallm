using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Provides the core services implemented by all resource providers.
    /// </summary>
    public interface IResourceProviderService : IManagementProviderService
    {
        /// <summary>
        /// The name of the resource provider.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Indicates whether the resource provider is initialized or not.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets the resource types supported
        /// </summary>
        Dictionary<string, ResourceTypeDescriptor> GetResourceTypes();
        /// The metadata describing the resource types allowed by the resource provider.
        /// </summary>
        Dictionary<string, ResourceTypeDescriptor> AllowedResourceTypes { get; }

        /// <summary>
        /// Gets a resource based on its logical path.
        /// </summary>
        /// <typeparam name="T">The type of the resource.</typeparam>
        /// <param name="resourcePath">The logical path of the resource.</param>
        /// <returns>The instance of the resource corresponding to the specified logical path.</returns>
        T GetResource<T>(string resourcePath) where T : class;

        /// <summary>
        /// Creates or updates a resource based on its logical path.
        /// </summary>
        /// <typeparam name="T">The type of the resource.</typeparam>
        /// <param name="resourcePath">The logical path of the resource.</param>
        /// <param name="resource">The instance of the resource being created or updated.</param>
        /// <returns>The object id of the resource.</returns>
        Task<string> UpsertResourceAsync<T>(string resourcePath, T resource) where T : class;
    }
}
