using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;

namespace FoundationaLLM.Client.Management.Interfaces
{
    /// <summary>
    /// Provides methods to manage attachment resources.
    /// </summary>
    public interface IAttachmentManagementClient
    {
        /// <summary>
        /// Retrieves all attachment resources.
        /// </summary>
        /// <returns>All attachment resources to which the caller has access.</returns>
        Task<List<ResourceProviderGetResult<AttachmentFile>>> GetAttachmentsAsync();

        /// <summary>
        /// Retrieves a specific attachment by name.
        /// </summary>
        /// <param name="attachmentName">The name of the attachment resource to retrieve.</param>
        /// <returns></returns>
        Task<ResourceProviderGetResult<AttachmentFile>> GetAttachmentAsync(string attachmentName);

        /// <summary>
        /// Upserts an attachment resource. If an attachment does not exist, it will be created. If an attachment
        /// does exist, it will be updated.
        /// </summary>
        /// <param name="attachment">The attachment resource to create or update.</param>
        /// <returns>Returns a <see cref="ResourceProviderUpsertResult"/>, which contains the
        /// Object ID of the resource.</returns>
        Task<ResourceProviderUpsertResult> UpsertAttachmentAsync(AttachmentFile attachment);

        /// <summary>
        /// Deletes an attachment resource by name.
        /// </summary>
        /// <param name="attachmentName">The name of the attachment resource to delete.</param>
        /// <returns></returns>
        Task DeleteAttachmentAsync(string attachmentName);
    }
}
