using FoundationaLLM.Client.Management;
using FoundationaLLM.Client.Management.Interfaces;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.Vectorization;
using NSubstitute;

namespace Management.Client.Tests
{
    public class ManagementClientTests
    {
        private readonly IManagementRESTClient _mockRestClient;
        private readonly ManagementClient _managementClient;

        public ManagementClientTests()
        {
            _mockRestClient = Substitute.For<IManagementRESTClient>();
            _managementClient = new ManagementClient();
            _managementClient.GetType().GetField("_managementRestClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_managementClient, _mockRestClient);
            _managementClient.GetType().GetMethod("InitializeClients", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(_managementClient, null);
        }

        [Fact]
        public void InitializeClients_ShouldInitializeAllClients()
        {
            // Assert
            Assert.NotNull(_managementClient.Agents);
            Assert.NotNull(_managementClient.Attachments);
            Assert.NotNull(_managementClient.Configuration);
            Assert.NotNull(_managementClient.DataSources);
            Assert.NotNull(_managementClient.Prompts);
            Assert.NotNull(_managementClient.Vectorization);
        }

        #region GetResourceByObjectId Tests

        [Fact]
        public async Task GetResourceByObjectId_ShouldReturnResource()
        {
            // Arrange
            var objectId = "some/object/id";
            var expectedResource = new ResourceProviderGetResult<VectorizationPipeline>
            {
                Resource = new VectorizationPipeline
                {
                    Name = "test-pipeline",
                    TriggerType = VectorizationPipelineTriggerType.Event,
                    Active = true,
                    DataSourceObjectId = "test-datasource",
                    TextPartitioningProfileObjectId = "test-text-partitioning-profile",
                    TextEmbeddingProfileObjectId = "test-text-embedding-profile",
                    IndexingProfileObjectId = "test-indexing-profile",
                },
                Actions = [],
                Roles = []
            };
            var expectedResources = new List<ResourceProviderGetResult<VectorizationPipeline>> { expectedResource };

            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<VectorizationPipeline>>>(objectId)
                .Returns(Task.FromResult(expectedResources));

            // Act
            var result = await _managementClient.GetResourceByObjectId<VectorizationPipeline>(objectId);

            // Assert
            Assert.Equal(expectedResource.Resource, result);
            await _mockRestClient.Resources.Received(1).GetResourcesAsync<List<ResourceProviderGetResult<VectorizationPipeline>>>(objectId);
        }

        [Fact]
        public async Task GetResourceByObjectId_ShouldThrowException_WhenResourceNotFound()
        {
            // Arrange
            var objectId = "some/object/id";
            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<VectorizationPipeline>>>(objectId)
                .Returns(Task.FromResult(new List<ResourceProviderGetResult<VectorizationPipeline>>()));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _managementClient.GetResourceByObjectId<VectorizationPipeline>(objectId));
            Assert.Equal($"Resource with ID {objectId} not found.", exception.Message);
        }

        #endregion GetResourceByObjectId Tests

        #region GetResourceWithActionsAndRolesByObjectId Tests

        [Fact]
        public async Task GetResourceWithActionsAndRolesByObjectId_ShouldReturnResourceWithActionsAndRoles()
        {
            // Arrange
            var objectId = "some/object/id";
            var expectedResource = new ResourceProviderGetResult<VectorizationPipeline>
            {
                Resource = new VectorizationPipeline
                {
                    Name = "test-pipeline",
                    TriggerType = VectorizationPipelineTriggerType.Event,
                    Active = true,
                    DataSourceObjectId = "test-datasource",
                    TextPartitioningProfileObjectId = "test-text-partitioning-profile",
                    TextEmbeddingProfileObjectId = "test-text-embedding-profile",
                    IndexingProfileObjectId = "test-indexing-profile",
                },
                Actions = [],
                Roles = []
            };
            var expectedResources = new List<ResourceProviderGetResult<VectorizationPipeline>> { expectedResource };

            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<VectorizationPipeline>>>(objectId)
                .Returns(Task.FromResult(expectedResources));

            // Act
            var result = await _managementClient.GetResourceWithActionsAndRolesByObjectId<VectorizationPipeline>(objectId);

            // Assert
            Assert.Equal(expectedResource, result);
            await _mockRestClient.Resources.Received(1).GetResourcesAsync<List<ResourceProviderGetResult<VectorizationPipeline>>>(objectId);
        }

        [Fact]
        public async Task GetResourceWithActionsAndRolesByObjectId_ShouldThrowException_WhenResourceNotFound()
        {
            // Arrange
            var objectId = "some/object/id";
            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<VectorizationPipeline>>>(objectId)
                .Returns(Task.FromResult(new List<ResourceProviderGetResult<VectorizationPipeline>>()));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _managementClient.GetResourceWithActionsAndRolesByObjectId<VectorizationPipeline>(objectId));
            Assert.Equal($"Resource with ID {objectId} not found.", exception.Message);
        }

        #endregion GetResourceWithActionsAndRolesByObjectId Tests
    }
}