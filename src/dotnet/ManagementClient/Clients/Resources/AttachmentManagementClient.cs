using FoundationaLLM.Client.Management.Interfaces;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;

namespace FoundationaLLM.Client.Management.Clients.Resources
{
    internal class AttachmentManagementClient(IManagementRESTClient managementRestClient) : IAttachmentManagementClient
    {
        /// <inheritdoc/>
        public async Task<List<ResourceProviderGetResult<AttachmentFile>>> GetAttachmentsAsync() =>
            await managementRestClient.Resources.GetResourcesAsync<List<ResourceProviderGetResult<AttachmentFile>>>(
                ResourceProviderNames.FoundationaLLM_Attachment,
                AttachmentResourceTypeNames.Attachments
            );

        /// <inheritdoc/>
        public async Task<ResourceProviderGetResult<AttachmentFile>> GetAttachmentAsync(string attachmentName)
        {
            var result = await managementRestClient.Resources.GetResourcesAsync<List<ResourceProviderGetResult<AttachmentFile>>>(
                ResourceProviderNames.FoundationaLLM_Attachment,
                $"{AttachmentResourceTypeNames.Attachments}/{attachmentName}"
            );

            if (result == null || result.Count == 0)
            {
                throw new Exception($"Attachment '{attachmentName}' not found.");
            }

            var resource = result[0];

            return resource;
        }

        /// <inheritdoc/>
        public async Task<ResourceProviderUpsertResult> UpsertAttachmentAsync(AttachmentFile attachment) => await managementRestClient.Resources.UpsertResourceAsync(
            ResourceProviderNames.FoundationaLLM_Attachment,
            $"{AttachmentResourceTypeNames.Attachments}/{attachment.Name}",
            attachment
            );

        /// <inheritdoc/>
        public async Task DeleteAttachmentAsync(string attachmentName) => await managementRestClient.Resources.DeleteResourceAsync(
                ResourceProviderNames.FoundationaLLM_Attachment,
                $"{AttachmentResourceTypeNames.Attachments}/{attachmentName}"
            );
    }
}
