using FoundationaLLM.Client.Management.Clients.Resources;
using FoundationaLLM.Client.Management.Interfaces;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Attachment;
using NSubstitute;

namespace Management.Client.Tests.Clients.Resources
{
    public class AttachmentManagementClientTests
    {
        private readonly IManagementRESTClient _mockRestClient;
        private readonly AttachmentManagementClient _attachmentClient;

        public AttachmentManagementClientTests()
        {
            _mockRestClient = Substitute.For<IManagementRESTClient>();
            _attachmentClient = new AttachmentManagementClient(_mockRestClient);
        }

        [Fact]
        public async Task GetAttachmentsAsync_ShouldReturnAttachments()
        {
            // Arrange
            var fileStream = new MemoryStream();
            var expectedAttachments = new List<ResourceProviderGetResult<AttachmentFile>>
            {
                new ResourceProviderGetResult<AttachmentFile>
                {
                    Resource = new AttachmentFile
                    {
                        Name = "test-attachment",
                        Content = fileStream,
                        ContentType = "application/octet-stream",
                        Path = "test-attachment.txt"
                    },
                    Actions = [],
                    Roles = []
                }
            };

            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<AttachmentFile>>>(
                    ResourceProviderNames.FoundationaLLM_Attachment,
                    AttachmentResourceTypeNames.Attachments
                )
                .Returns(Task.FromResult(expectedAttachments));

            // Act
            var result = await _attachmentClient.GetAttachmentsAsync();

            // Assert
            Assert.Equal(expectedAttachments, result);
            await _mockRestClient.Resources.Received(1).GetResourcesAsync<List<ResourceProviderGetResult<AttachmentFile>>>(
                ResourceProviderNames.FoundationaLLM_Attachment,
                AttachmentResourceTypeNames.Attachments
            );
        }

        [Fact]
        public async Task GetAttachmentAsync_ShouldReturnAttachment()
        {
            // Arrange
            var attachmentName = "test-attachment";
            var fileStream = new MemoryStream();
            var expectedAttachment = new ResourceProviderGetResult<AttachmentFile>
            {
                Resource = new AttachmentFile
                {
                    Name = attachmentName,
                    Content = fileStream,
                    ContentType = "application/octet-stream",
                    Path = "test-attachment.txt"
                },
                Actions = [],
                Roles = []
            };
            var expectedAttachments = new List<ResourceProviderGetResult<AttachmentFile>> { expectedAttachment };

            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<AttachmentFile>>>(
                    ResourceProviderNames.FoundationaLLM_Attachment,
                    $"{AttachmentResourceTypeNames.Attachments}/{attachmentName}"
                )
                .Returns(Task.FromResult(expectedAttachments));

            // Act
            var result = await _attachmentClient.GetAttachmentAsync(attachmentName);

            // Assert
            Assert.Equal(expectedAttachment, result);
            await _mockRestClient.Resources.Received(1).GetResourcesAsync<List<ResourceProviderGetResult<AttachmentFile>>>(
                ResourceProviderNames.FoundationaLLM_Attachment,
                $"{AttachmentResourceTypeNames.Attachments}/{attachmentName}"
            );
        }

        [Fact]
        public async Task GetAttachmentAsync_ShouldThrowException_WhenAttachmentNotFound()
        {
            // Arrange
            var attachmentName = "test-attachment";
            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<AttachmentFile>>>(
                    ResourceProviderNames.FoundationaLLM_Attachment,
                    $"{AttachmentResourceTypeNames.Attachments}/{attachmentName}"
                )
                .Returns(Task.FromResult<List<ResourceProviderGetResult<AttachmentFile>>>(null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _attachmentClient.GetAttachmentAsync(attachmentName));
            Assert.Equal($"Attachment '{attachmentName}' not found.", exception.Message);
        }

        [Fact]
        public async Task UpsertAttachmentAsync_ShouldReturnUpsertResult()
        {
            // Arrange
            var attachment = new AttachmentFile { Name = "test-attachment" };
            var expectedUpsertResult = new ResourceProviderUpsertResult
            {
                ObjectId = "test-object-id"
            }; ;

            _mockRestClient.Resources
                .UpsertResourceAsync(
                    ResourceProviderNames.FoundationaLLM_Attachment,
                    $"{AttachmentResourceTypeNames.Attachments}/{attachment.Name}",
                    attachment
                )
                .Returns(Task.FromResult(expectedUpsertResult));

            // Act
            var result = await _attachmentClient.UpsertAttachmentAsync(attachment);

            // Assert
            Assert.Equal(expectedUpsertResult, result);
            await _mockRestClient.Resources.Received(1).UpsertResourceAsync(
                ResourceProviderNames.FoundationaLLM_Attachment,
                $"{AttachmentResourceTypeNames.Attachments}/{attachment.Name}",
                attachment
            );
        }

        [Fact]
        public async Task DeleteAttachmentAsync_ShouldCallDeleteResource()
        {
            // Arrange
            var attachmentName = "test-attachment";

            // Act
            await _attachmentClient.DeleteAttachmentAsync(attachmentName);

            // Assert
            await _mockRestClient.Resources.Received(1).DeleteResourceAsync(
                ResourceProviderNames.FoundationaLLM_Attachment,
                $"{AttachmentResourceTypeNames.Attachments}/{attachmentName}"
            );
        }
    }
}
