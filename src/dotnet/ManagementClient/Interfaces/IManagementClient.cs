using FoundationaLLM.Common.Models.ResourceProviders;

namespace FoundationaLLM.Client.Management.Interfaces
{
    /// <summary>
    /// Provides high-level methods to interact with the Management API.
    /// </summary>
    public interface IManagementClient
    {
        /// <summary>
        /// Contains methods to interact with Agent resources.
        /// </summary>
        IAgentManagementClient Agents { get; }
        /// <summary>
        /// Contains methods to interact with Attachment resources.
        /// </summary>
        IAttachmentManagementClient Attachments { get; }
        /// <summary>
        /// Contains methods to interact with Configuration resources.
        /// </summary>
        IConfigurationManagementClient Configuration { get; }
        /// <summary>
        /// Contains methods to interact with DataSource resources.
        /// </summary>
        IDataSourceManagementClient DataSources { get; }
        /// <summary>
        /// Contains methods to interact with Prompt resources.
        /// </summary>
        IPromptManagementClient Prompts { get; }
        /// <summary>
        /// Contains methods to interact with Vectorization resources.
        /// </summary>
        IVectorizationManagementClient Vectorization { get; }
        /// <summary>
        /// Retrieves a resource by its ObjectId.
        /// </summary>
        /// <typeparam name="T">The type of resource to retrieve. It must be derived from <see cref="ResourceBase"/>.</typeparam>
        /// <param name="objectId">The resource's Object ID (full resource path).</param>
        /// <returns>Returns a deserialized resource object.</returns>
        Task<T> GetResourceByObjectId<T>(string objectId) where T : ResourceBase;
        /// <summary>
        /// Retrieves a resource by its ObjectId, including actions and roles.
        /// </summary>
        /// <typeparam name="T">The type of resource to retrieve. It must be derived from <see cref="ResourceBase"/>.</typeparam>
        /// <param name="objectId">The resource's Object ID (full resource path).</param>
        /// <returns>Returns a deserialized resource object and its list of allowed actions and roles.</returns>
        Task<ResourceProviderGetResult<T>> GetResourceWithActionsAndRolesByObjectId<T>(string objectId) where T : ResourceBase;
    }
}
