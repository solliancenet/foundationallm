using FoundationaLLM.Client.Management.Clients.Resources;
using FoundationaLLM.Client.Management.Interfaces;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Models.Vectorization;
using NSubstitute;

namespace Management.Client.Tests.Clients.Resources
{
    public class VectorizationManagementClientTests
    {
        private readonly IManagementRESTClient _mockRestClient;
        private readonly VectorizationManagementClient _vectorizationClient;

        public VectorizationManagementClientTests()
        {
            _mockRestClient = Substitute.For<IManagementRESTClient>();
            _vectorizationClient = new VectorizationManagementClient(_mockRestClient);
        }

        #region Get Methods Tests

        [Fact]
        public async Task GetVectorizationPipelinesAsync_ShouldReturnPipelines()
        {
            // Arrange
            var expectedPipelines = new List<ResourceProviderGetResult<VectorizationPipeline>>
            {
                new ResourceProviderGetResult<VectorizationPipeline>
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
                },
                new ResourceProviderGetResult<VectorizationPipeline>()
                {
                    Resource = new VectorizationPipeline
                    {
                        Name = "test-pipeline-2",
                        TriggerType = VectorizationPipelineTriggerType.Manual,
                        Active = true,
                        DataSourceObjectId = "test-datasource-2",
                        TextPartitioningProfileObjectId = "test-text-partitioning-profile-2",
                        TextEmbeddingProfileObjectId = "test-text-embedding-profile-2",
                        IndexingProfileObjectId = "test-indexing-profile-2",
                    },
                    Actions = [],
                    Roles = []
                }
            };

            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<VectorizationPipeline>>>(
                    ResourceProviderNames.FoundationaLLM_Vectorization,
                    VectorizationResourceTypeNames.VectorizationPipelines
                )
                .Returns(Task.FromResult(expectedPipelines));

            // Act
            var result = await _vectorizationClient.GetVectorizationPipelinesAsync();

            // Assert
            Assert.Equal(expectedPipelines, result);
            await _mockRestClient.Resources.Received(1).GetResourcesAsync<List<ResourceProviderGetResult<VectorizationPipeline>>>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                VectorizationResourceTypeNames.VectorizationPipelines
            );
        }

        [Fact]
        public async Task GetVectorizationPipelineAsync_ShouldReturnPipeline()
        {
            // Arrange
            var pipelineName = "test-pipeline";
            var expectedPipeline = new ResourceProviderGetResult<VectorizationPipeline>
            {
                Resource = new VectorizationPipeline
                {
                    Name = pipelineName,
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
            var expectedPipelines = new List<ResourceProviderGetResult<VectorizationPipeline>> { expectedPipeline };

            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<VectorizationPipeline>>>(
                    ResourceProviderNames.FoundationaLLM_Vectorization,
                    $"{VectorizationResourceTypeNames.VectorizationPipelines}/{pipelineName}"
                )
                .Returns(Task.FromResult(expectedPipelines));

            // Act
            var result = await _vectorizationClient.GetVectorizationPipelineAsync(pipelineName);

            // Assert
            Assert.Equal(expectedPipeline, result);
            await _mockRestClient.Resources.Received(1).GetResourcesAsync<List<ResourceProviderGetResult<VectorizationPipeline>>>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.VectorizationPipelines}/{pipelineName}"
            );
        }

        [Fact]
        public async Task GetVectorizationPipelineAsync_ShouldThrowException_WhenPipelineNotFound()
        {
            // Arrange
            var pipelineName = "test-pipeline";
            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<VectorizationPipeline>>>(
                    ResourceProviderNames.FoundationaLLM_Vectorization,
                    $"{VectorizationResourceTypeNames.VectorizationPipelines}/{pipelineName}"
                )
                .Returns(Task.FromResult<List<ResourceProviderGetResult<VectorizationPipeline>>>(null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _vectorizationClient.GetVectorizationPipelineAsync(pipelineName));
            Assert.Equal($"VectorizationPipeline '{pipelineName}' not found.", exception.Message);
        }

        [Fact]
        public async Task GetTextPartitioningProfilesAsync_ShouldReturnProfiles()
        {
            // Arrange
            var expectedProfiles = new List<ResourceProviderGetResult<TextPartitioningProfile>>
            {
                new ResourceProviderGetResult<TextPartitioningProfile>
                {
                    Resource = new TextPartitioningProfile
                    {
                        Name = "test-profile",
                        TextSplitter = TextSplitterType.TokenTextSplitter,
                        ObjectId = "test-object-id"
                    },
                    Actions = [],
                    Roles = []
                },
                new ResourceProviderGetResult<TextPartitioningProfile>
                {
                    Resource = new TextPartitioningProfile
                    {
                        Name = "test-profile-2",
                        TextSplitter = TextSplitterType.TokenTextSplitter,
                        ObjectId = "test-object-id-2"
                    },
                    Actions = [],
                    Roles = []
                }
            };

            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<TextPartitioningProfile>>>(
                    ResourceProviderNames.FoundationaLLM_Vectorization,
                    VectorizationResourceTypeNames.TextPartitioningProfiles
                )
                .Returns(Task.FromResult(expectedProfiles));

            // Act
            var result = await _vectorizationClient.GetTextPartitioningProfilesAsync();

            // Assert
            Assert.Equal(expectedProfiles, result);
            await _mockRestClient.Resources.Received(1).GetResourcesAsync<List<ResourceProviderGetResult<TextPartitioningProfile>>>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                VectorizationResourceTypeNames.TextPartitioningProfiles
            );
        }

        [Fact]
        public async Task GetTextPartitioningProfileAsync_ShouldReturnProfile()
        {
            // Arrange
            var profileName = "test-profile";
            var expectedProfile = new ResourceProviderGetResult<TextPartitioningProfile>
            {
                Resource = new TextPartitioningProfile
                {
                    Name = profileName,
                    TextSplitter = TextSplitterType.TokenTextSplitter,
                    ObjectId = "test-object-id"
                },
                Actions = [],
                Roles = []
            };
            var expectedProfiles = new List<ResourceProviderGetResult<TextPartitioningProfile>> { expectedProfile };

            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<TextPartitioningProfile>>>(
                    ResourceProviderNames.FoundationaLLM_Vectorization,
                    $"{VectorizationResourceTypeNames.TextPartitioningProfiles}/{profileName}"
                )
                .Returns(Task.FromResult(expectedProfiles));

            // Act
            var result = await _vectorizationClient.GetTextPartitioningProfileAsync(profileName);

            // Assert
            Assert.Equal(expectedProfile, result);
            await _mockRestClient.Resources.Received(1).GetResourcesAsync<List<ResourceProviderGetResult<TextPartitioningProfile>>>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.TextPartitioningProfiles}/{profileName}"
            );
        }

        [Fact]
        public async Task GetTextPartitioningProfileAsync_ShouldThrowException_WhenProfileNotFound()
        {
            // Arrange
            var profileName = "test-profile";
            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<TextPartitioningProfile>>>(
                    ResourceProviderNames.FoundationaLLM_Vectorization,
                    $"{VectorizationResourceTypeNames.TextPartitioningProfiles}/{profileName}"
                )
                .Returns(Task.FromResult<List<ResourceProviderGetResult<TextPartitioningProfile>>>(null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _vectorizationClient.GetTextPartitioningProfileAsync(profileName));
            Assert.Equal($"TextPartitioningProfile '{profileName}' not found.", exception.Message);
        }

        [Fact]
        public async Task GetTextEmbeddingProfilesAsync_ShouldReturnProfiles()
        {
            // Arrange
            var expectedProfiles = new List<ResourceProviderGetResult<TextEmbeddingProfile>>
            {
                new ResourceProviderGetResult<TextEmbeddingProfile>
                {
                    Resource = new TextEmbeddingProfile
                    {
                        Name = "test-profile",
                        TextEmbedding = TextEmbeddingType.SemanticKernelTextEmbedding,
                        ObjectId = "test-object-id"
                    },
                    Actions = [],
                    Roles = []
                },
                new ResourceProviderGetResult<TextEmbeddingProfile>
                {
                    Resource = new TextEmbeddingProfile
                    {
                        Name = "test-profile-2",
                        TextEmbedding = TextEmbeddingType.GatewayTextEmbedding,
                        ObjectId = "test-object-id-2"
                    },
                    Actions = [],
                    Roles = []
                }
            };

            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<TextEmbeddingProfile>>>(
                    ResourceProviderNames.FoundationaLLM_Vectorization,
                    VectorizationResourceTypeNames.TextEmbeddingProfiles
                )
                .Returns(Task.FromResult(expectedProfiles));

            // Act
            var result = await _vectorizationClient.GetTextEmbeddingProfilesAsync();

            // Assert
            Assert.Equal(expectedProfiles, result);
            await _mockRestClient.Resources.Received(1).GetResourcesAsync<List<ResourceProviderGetResult<TextEmbeddingProfile>>>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                VectorizationResourceTypeNames.TextEmbeddingProfiles
            );
        }

        [Fact]
        public async Task GetTextEmbeddingProfileAsync_ShouldReturnProfile()
        {
            // Arrange
            var profileName = "test-profile";
            var expectedProfile = new ResourceProviderGetResult<TextEmbeddingProfile>
            {
                Resource = new TextEmbeddingProfile
                {
                    Name = profileName,
                    TextEmbedding = TextEmbeddingType.SemanticKernelTextEmbedding,
                    ObjectId = "test-object-id"
                },
                Actions = [],
                Roles = []
            };
            var expectedProfiles = new List<ResourceProviderGetResult<TextEmbeddingProfile>> { expectedProfile };

            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<TextEmbeddingProfile>>>(
                    ResourceProviderNames.FoundationaLLM_Vectorization,
                    $"{VectorizationResourceTypeNames.TextEmbeddingProfiles}/{profileName}"
                )
                .Returns(Task.FromResult(expectedProfiles));

            // Act
            var result = await _vectorizationClient.GetTextEmbeddingProfileAsync(profileName);

            // Assert
            Assert.Equal(expectedProfile, result);
            await _mockRestClient.Resources.Received(1).GetResourcesAsync<List<ResourceProviderGetResult<TextEmbeddingProfile>>>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.TextEmbeddingProfiles}/{profileName}"
            );
        }

        [Fact]
        public async Task GetTextEmbeddingProfileAsync_ShouldThrowException_WhenProfileNotFound()
        {
            // Arrange
            var profileName = "test-profile";
            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<TextEmbeddingProfile>>>(
                    ResourceProviderNames.FoundationaLLM_Vectorization,
                    $"{VectorizationResourceTypeNames.TextEmbeddingProfiles}/{profileName}"
                )
                .Returns(Task.FromResult<List<ResourceProviderGetResult<TextEmbeddingProfile>>>(null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _vectorizationClient.GetTextEmbeddingProfileAsync(profileName));
            Assert.Equal($"TextEmbeddingProfile '{profileName}' not found.", exception.Message);
        }

        [Fact]
        public async Task GetIndexingProfilesAsync_ShouldReturnProfiles()
        {
            // Arrange
            var expectedProfiles = new List<ResourceProviderGetResult<IndexingProfile>>
            {
                new ResourceProviderGetResult<IndexingProfile>
                {
                    Resource = new IndexingProfile
                    {
                        Name = "test-profile",
                        Indexer = IndexerType.AzureAISearchIndexer
                    },
                    Actions = [],
                    Roles = []
                },
                new ResourceProviderGetResult<IndexingProfile>
                {
                    Resource = new IndexingProfile
                    {
                        Name = "test-profile-2",
                        Indexer = IndexerType.AzureCosmosDBNoSQLIndexer
                    },
                    Actions = [],
                    Roles = []
                },
                new ResourceProviderGetResult<IndexingProfile>
                {
                    Resource = new IndexingProfile
                    {
                        Name = "test-profile-3",
                        Indexer = IndexerType.PostgresIndexer
                    },
                    Actions = [],
                    Roles = []
                }
            };

            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<IndexingProfile>>>(
                    ResourceProviderNames.FoundationaLLM_Vectorization,
                    VectorizationResourceTypeNames.IndexingProfiles
                )
                .Returns(Task.FromResult(expectedProfiles));

            // Act
            var result = await _vectorizationClient.GetIndexingProfilesAsync();

            // Assert
            Assert.Equal(expectedProfiles, result);
            await _mockRestClient.Resources.Received(1).GetResourcesAsync<List<ResourceProviderGetResult<IndexingProfile>>>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                VectorizationResourceTypeNames.IndexingProfiles
            );
        }

        [Fact]
        public async Task GetIndexingProfileAsync_ShouldReturnProfile()
        {
            // Arrange
            var profileName = "test-profile";
            var expectedProfile = new ResourceProviderGetResult<IndexingProfile>
            {
                Resource = new IndexingProfile
                {
                    Name = profileName,
                    Indexer = IndexerType.AzureCosmosDBNoSQLIndexer
                },
                Actions = [],
                Roles = []
            };
            var expectedProfiles = new List<ResourceProviderGetResult<IndexingProfile>> { expectedProfile };

            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<IndexingProfile>>>(
                    ResourceProviderNames.FoundationaLLM_Vectorization,
                    $"{VectorizationResourceTypeNames.IndexingProfiles}/{profileName}"
                )
                .Returns(Task.FromResult(expectedProfiles));

            // Act
            var result = await _vectorizationClient.GetIndexingProfileAsync(profileName);

            // Assert
            Assert.Equal(expectedProfile, result);
            await _mockRestClient.Resources.Received(1).GetResourcesAsync<List<ResourceProviderGetResult<IndexingProfile>>>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.IndexingProfiles}/{profileName}"
            );
        }

        [Fact]
        public async Task GetIndexingProfileAsync_ShouldThrowException_WhenProfileNotFound()
        {
            // Arrange
            var profileName = "test-profile";
            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<IndexingProfile>>>(
                    ResourceProviderNames.FoundationaLLM_Vectorization,
                    $"{VectorizationResourceTypeNames.IndexingProfiles}/{profileName}"
                )
                .Returns(Task.FromResult<List<ResourceProviderGetResult<IndexingProfile>>>(null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _vectorizationClient.GetIndexingProfileAsync(profileName));
            Assert.Equal($"IndexingProfile '{profileName}' not found.", exception.Message);
        }

        #endregion Get Methods Tests

        #region Action Methods Tests

        [Fact]
        public async Task ActivateVectorizationPipelineAsync_ShouldReturnResult()
        {
            // Arrange
            var pipelineName = "test-pipeline";
            var expectedResult = new VectorizationResult("test-pipeline-object-id", true, null);

            _mockRestClient.Resources
                .ExecuteResourceActionAsync<VectorizationResult>(
                    ResourceProviderNames.FoundationaLLM_Vectorization,
                    $"{VectorizationResourceTypeNames.VectorizationPipelines}/{pipelineName}/{VectorizationResourceProviderActions.Activate}",
                    Arg.Any<object>()
                )
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _vectorizationClient.ActivateVectorizationPipelineAsync(pipelineName);

            // Assert
            Assert.Equal(expectedResult, result);
            await _mockRestClient.Resources.Received(1).ExecuteResourceActionAsync<VectorizationResult>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.VectorizationPipelines}/{pipelineName}/{VectorizationResourceProviderActions.Activate}",
                Arg.Any<object>()
            );
        }

        [Fact]
        public async Task ActivateVectorizationPipelineAsync_ShouldThrowArgumentException_WhenPipelineNameIsInvalid()
        {
            // Arrange
            var invalidPipelineName = "";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _vectorizationClient.ActivateVectorizationPipelineAsync(invalidPipelineName));
        }

        [Fact]
        public async Task DeactivateVectorizationPipelineAsync_ShouldReturnResult()
        {
            // Arrange
            var pipelineName = "test-pipeline";
            var expectedResult = new VectorizationResult("test-pipeline-object-id", true, null);

            _mockRestClient.Resources
                .ExecuteResourceActionAsync<VectorizationResult>(
                    ResourceProviderNames.FoundationaLLM_Vectorization,
                    $"{VectorizationResourceTypeNames.VectorizationPipelines}/{pipelineName}/{VectorizationResourceProviderActions.Deactivate}",
                    Arg.Any<object>()
                )
                .Returns(Task.FromResult(expectedResult));

            // Act
            var result = await _vectorizationClient.DeactivateVectorizationPipelineAsync(pipelineName);

            // Assert
            Assert.Equal(expectedResult, result);
            await _mockRestClient.Resources.Received(1).ExecuteResourceActionAsync<VectorizationResult>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.VectorizationPipelines}/{pipelineName}/{VectorizationResourceProviderActions.Deactivate}",
                Arg.Any<object>()
            );
        }

        [Fact]
        public async Task DeactivateVectorizationPipelineAsync_ShouldThrowArgumentException_WhenPipelineNameIsInvalid()
        {
            // Arrange
            var invalidPipelineName = "";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _vectorizationClient.DeactivateVectorizationPipelineAsync(invalidPipelineName));
        }

        [Fact]
        public async Task PurgeVectorizationPipelineAsync_ShouldReturnPurgeResult()
        {
            // Arrange
            var pipelineName = "test-pipeline";
            var expectedPurgeResult = new ResourceProviderActionResult(true);

            _mockRestClient.Resources
                .ExecuteResourceActionAsync<ResourceProviderActionResult>(
                    ResourceProviderNames.FoundationaLLM_Vectorization,
                    $"{VectorizationResourceTypeNames.VectorizationPipelines}/{pipelineName}/{VectorizationResourceProviderActions.Purge}",
                    Arg.Any<object>()
                )
                .Returns(Task.FromResult(expectedPurgeResult));

            // Act
            var result = await _vectorizationClient.PurgeVectorizationPipelineAsync(pipelineName);

            // Assert
            Assert.Equal(expectedPurgeResult, result);
            await _mockRestClient.Resources.Received(1).ExecuteResourceActionAsync<ResourceProviderActionResult>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.VectorizationPipelines}/{pipelineName}/{VectorizationResourceProviderActions.Purge}",
                Arg.Any<object>()
            );
        }

        [Fact]
        public async Task PurgeVectorizationPipelineAsync_ShouldThrowArgumentException_WhenPipelineNameIsInvalid()
        {
            // Arrange
            var invalidPipelineName = "";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _vectorizationClient.PurgeVectorizationPipelineAsync(invalidPipelineName));
        }

        [Fact]
        public async Task PurgeTextPartitioningProfileAsync_ShouldReturnPurgeResult()
        {
            // Arrange
            var profileName = "test-profile";
            var expectedPurgeResult = new ResourceProviderActionResult(true);

            _mockRestClient.Resources
                .ExecuteResourceActionAsync<ResourceProviderActionResult>(
                    ResourceProviderNames.FoundationaLLM_Vectorization,
                    $"{VectorizationResourceTypeNames.TextPartitioningProfiles}/{profileName}/{VectorizationResourceProviderActions.Purge}",
                    Arg.Any<object>()
                )
                .Returns(Task.FromResult(expectedPurgeResult));

            // Act
            var result = await _vectorizationClient.PurgeTextPartitioningProfileAsync(profileName);

            // Assert
            Assert.Equal(expectedPurgeResult, result);
            await _mockRestClient.Resources.Received(1).ExecuteResourceActionAsync<ResourceProviderActionResult>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.TextPartitioningProfiles}/{profileName}/{VectorizationResourceProviderActions.Purge}",
                Arg.Any<object>()
            );
        }

        [Fact]
        public async Task PurgeTextPartitioningProfileAsync_ShouldThrowArgumentException_WhenProfileNameIsInvalid()
        {
            // Arrange
            var invalidProfileName = "";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _vectorizationClient.PurgeTextPartitioningProfileAsync(invalidProfileName));
        }

        [Fact]
        public async Task PurgeTextEmbeddingProfileAsync_ShouldReturnPurgeResult()
        {
            // Arrange
            var profileName = "test-profile";
            var expectedPurgeResult = new ResourceProviderActionResult(true);

            _mockRestClient.Resources
                .ExecuteResourceActionAsync<ResourceProviderActionResult>(
                    ResourceProviderNames.FoundationaLLM_Vectorization,
                    $"{VectorizationResourceTypeNames.TextEmbeddingProfiles}/{profileName}/{VectorizationResourceProviderActions.Purge}",
                    Arg.Any<object>()
                )
                .Returns(Task.FromResult(expectedPurgeResult));

            // Act
            var result = await _vectorizationClient.PurgeTextEmbeddingProfileAsync(profileName);

            // Assert
            Assert.Equal(expectedPurgeResult, result);
            await _mockRestClient.Resources.Received(1).ExecuteResourceActionAsync<ResourceProviderActionResult>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.TextEmbeddingProfiles}/{profileName}/{VectorizationResourceProviderActions.Purge}",
                Arg.Any<object>()
            );
        }

        [Fact]
        public async Task PurgeTextEmbeddingProfileAsync_ShouldThrowArgumentException_WhenProfileNameIsInvalid()
        {
            // Arrange
            var invalidProfileName = "";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _vectorizationClient.PurgeTextEmbeddingProfileAsync(invalidProfileName));
        }

        [Fact]
        public async Task CheckIndexingProfileNameAsync_ShouldReturnCheckResult()
        {
            // Arrange
            var resourceName = new ResourceName { Name = "test-profile", Type = "indexing-profile" };
            var expectedCheckResult = new ResourceNameCheckResult
            {
                Name = resourceName.Name,
                Status = NameCheckResultType.Allowed,
                Message = "Name is allowed"
            };

            _mockRestClient.Resources
                .ExecuteResourceActionAsync<ResourceNameCheckResult>(
                    ResourceProviderNames.FoundationaLLM_Vectorization,
                    $"{VectorizationResourceTypeNames.IndexingProfiles}/{VectorizationResourceProviderActions.CheckName}",
                    resourceName
                )
                .Returns(Task.FromResult(expectedCheckResult));

            // Act
            var result = await _vectorizationClient.CheckIndexingProfileNameAsync(resourceName);

            // Assert
            Assert.Equal(expectedCheckResult, result);
            await _mockRestClient.Resources.Received(1).ExecuteResourceActionAsync<ResourceNameCheckResult>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.IndexingProfiles}/{VectorizationResourceProviderActions.CheckName}",
                resourceName
            );
        }

        [Fact]
        public async Task CheckIndexingProfileNameAsync_ShouldThrowArgumentException_WhenResourceNameIsInvalid()
        {
            // Arrange
            var invalidResourceName = new ResourceName { Name = "", Type = "" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _vectorizationClient.CheckIndexingProfileNameAsync(invalidResourceName));
        }

        [Fact]
        public async Task FilterIndexingProfileAsync_ShouldReturnFilteredProfiles()
        {
            // Arrange
            var resourceFilter = new ResourceFilter
            {
                Default = false
            };
            var expectedProfiles = new List<IndexingProfile>
            {
                new IndexingProfile
                {
                    Name = "test-profile",
                    Indexer = IndexerType.AzureAISearchIndexer
                }
            };

            _mockRestClient.Resources
                .ExecuteResourceActionAsync<List<IndexingProfile>>(
                    ResourceProviderNames.FoundationaLLM_Vectorization,
                    $"{VectorizationResourceTypeNames.IndexingProfiles}/{VectorizationResourceProviderActions.Filter}",
                    resourceFilter
                )
                .Returns(Task.FromResult(expectedProfiles));

            // Act
            var result = await _vectorizationClient.FilterIndexingProfileAsync(resourceFilter);

            // Assert
            Assert.Equal(expectedProfiles, result);
            await _mockRestClient.Resources.Received(1).ExecuteResourceActionAsync<List<IndexingProfile>>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.IndexingProfiles}/{VectorizationResourceProviderActions.Filter}",
                resourceFilter
            );
        }

        [Fact]
        public async Task PurgeIndexingProfileAsync_ShouldReturnPurgeResult()
        {
            // Arrange
            var profileName = "test-profile";
            var expectedPurgeResult = new ResourceProviderActionResult(true);

            _mockRestClient.Resources
                .ExecuteResourceActionAsync<ResourceProviderActionResult>(
                    ResourceProviderNames.FoundationaLLM_Vectorization,
                    $"{VectorizationResourceTypeNames.IndexingProfiles}/{profileName}/{VectorizationResourceProviderActions.Purge}",
                    Arg.Any<object>()
                )
                .Returns(Task.FromResult(expectedPurgeResult));

            // Act
            var result = await _vectorizationClient.PurgeIndexingProfileAsync(profileName);

            // Assert
            Assert.Equal(expectedPurgeResult, result);
            await _mockRestClient.Resources.Received(1).ExecuteResourceActionAsync<ResourceProviderActionResult>(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.IndexingProfiles}/{profileName}/{VectorizationResourceProviderActions.Purge}",
                Arg.Any<object>()
            );
        }

        [Fact]
        public async Task PurgeIndexingProfileAsync_ShouldThrowArgumentException_WhenProfileNameIsInvalid()
        {
            // Arrange
            var invalidProfileName = "";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _vectorizationClient.PurgeIndexingProfileAsync(invalidProfileName));
        }

        #endregion Action Methods Tests

        #region Upsert Methods Tests

        [Fact]
        public async Task UpsertVectorizationPipelineAsync_ShouldReturnUpsertResult()
        {
            // Arrange
            var pipeline = new VectorizationPipeline
            {
                Name = "test-pipeline",
                TriggerType = VectorizationPipelineTriggerType.Event,
                Active = true,
                DataSourceObjectId = "test-datasource",
                TextPartitioningProfileObjectId = "test-text-partitioning-profile",
                TextEmbeddingProfileObjectId = "test-text-embedding-profile",
                IndexingProfileObjectId = "test-indexing-profile",
            };
        
            var expectedUpsertResult = new ResourceProviderUpsertResult
            {
                ObjectId = "test-object-id"
            };

            _mockRestClient.Resources
                .UpsertResourceAsync(
                    ResourceProviderNames.FoundationaLLM_Vectorization,
                    $"{VectorizationResourceTypeNames.VectorizationPipelines}/{pipeline.Name}",
                    pipeline
                )
                .Returns(Task.FromResult(expectedUpsertResult));

            // Act
            var result = await _vectorizationClient.UpsertVectorizationPipelineAsync(pipeline);

            // Assert
            Assert.Equal(expectedUpsertResult, result);
            await _mockRestClient.Resources.Received(1).UpsertResourceAsync(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.VectorizationPipelines}/{pipeline.Name}",
                pipeline
            );
        }

        [Fact]
        public async Task UpsertTextPartitioningProfileAsync_ShouldReturnUpsertResult()
        {
            // Arrange
            var profile = new TextPartitioningProfile
            {
                Name = "test-profile",
                TextSplitter = TextSplitterType.TokenTextSplitter
            };
            var expectedUpsertResult = new ResourceProviderUpsertResult
            {
                ObjectId = "test-object-id"
            };

            _mockRestClient.Resources
                .UpsertResourceAsync(
                    ResourceProviderNames.FoundationaLLM_Vectorization,
                    $"{VectorizationResourceTypeNames.TextPartitioningProfiles}/{profile.Name}",
                    profile
                )
                .Returns(Task.FromResult(expectedUpsertResult));

            // Act
            var result = await _vectorizationClient.UpsertTextPartitioningProfileAsync(profile);

            // Assert
            Assert.Equal(expectedUpsertResult, result);
            await _mockRestClient.Resources.Received(1).UpsertResourceAsync(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.TextPartitioningProfiles}/{profile.Name}",
                profile
            );
        }

        [Fact]
        public async Task UpsertTextEmbeddingProfileAsync_ShouldReturnUpsertResult()
        {
            // Arrange
            var profile = new TextEmbeddingProfile
            {
                Name = "test-profile",
                TextEmbedding = TextEmbeddingType.SemanticKernelTextEmbedding
            };
            var expectedUpsertResult = new ResourceProviderUpsertResult
            {
                ObjectId = "test-object-id"
            };

            _mockRestClient.Resources
                .UpsertResourceAsync(
                    ResourceProviderNames.FoundationaLLM_Vectorization,
                    $"{VectorizationResourceTypeNames.TextEmbeddingProfiles}/{profile.Name}",
                    profile
                )
                .Returns(Task.FromResult(expectedUpsertResult));

            // Act
            var result = await _vectorizationClient.UpsertTextEmbeddingProfileAsync(profile);

            // Assert
            Assert.Equal(expectedUpsertResult, result);
            await _mockRestClient.Resources.Received(1).UpsertResourceAsync(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.TextEmbeddingProfiles}/{profile.Name}",
                profile
            );
        }

        [Fact]
        public async Task UpsertIndexingProfileAsync_ShouldReturnUpsertResult()
        {
            // Arrange
            var profile = new IndexingProfile
            {
                Name = "test-profile",
                Indexer = IndexerType.AzureAISearchIndexer
            };
            var expectedUpsertResult = new ResourceProviderUpsertResult
            {
                ObjectId = "test-object-id"
            };

            _mockRestClient.Resources
                .UpsertResourceAsync(
                    ResourceProviderNames.FoundationaLLM_Vectorization,
                    $"{VectorizationResourceTypeNames.IndexingProfiles}/{profile.Name}",
                    profile
                )
                .Returns(Task.FromResult(expectedUpsertResult));

            // Act
            var result = await _vectorizationClient.UpsertIndexingProfileAsync(profile);

            // Assert
            Assert.Equal(expectedUpsertResult, result);
            await _mockRestClient.Resources.Received(1).UpsertResourceAsync(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.IndexingProfiles}/{profile.Name}",
                profile
            );
        }

        #endregion Upsert Methods Tests

        #region Delete Methods Tests

        [Fact]
        public async Task DeleteVectorizationPipelineAsync_ShouldCallDeleteResource()
        {
            // Arrange
            var pipelineName = "test-pipeline";

            // Act
            await _vectorizationClient.DeleteVectorizationPipelineAsync(pipelineName);

            // Assert
            await _mockRestClient.Resources.Received(1).DeleteResourceAsync(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.VectorizationPipelines}/{pipelineName}"
            );
        }

        [Fact]
        public async Task DeleteTextPartitioningProfileAsync_ShouldCallDeleteResource()
        {
            // Arrange
            var profileName = "test-profile";

            // Act
            await _vectorizationClient.DeleteTextPartitioningProfileAsync(profileName);

            // Assert
            await _mockRestClient.Resources.Received(1).DeleteResourceAsync(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.TextPartitioningProfiles}/{profileName}"
            );
        }

        [Fact]
        public async Task DeleteTextEmbeddingProfileAsync_ShouldCallDeleteResource()
        {
            // Arrange
            var profileName = "test-profile";

            // Act
            await _vectorizationClient.DeleteTextEmbeddingProfileAsync(profileName);

            // Assert
            await _mockRestClient.Resources.Received(1).DeleteResourceAsync(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.TextEmbeddingProfiles}/{profileName}"
            );
        }

        [Fact]
        public async Task DeleteIndexingProfileAsync_ShouldCallDeleteResource()
        {
            // Arrange
            var profileName = "test-profile";

            // Act
            await _vectorizationClient.DeleteIndexingProfileAsync(profileName);

            // Assert
            await _mockRestClient.Resources.Received(1).DeleteResourceAsync(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"{VectorizationResourceTypeNames.IndexingProfiles}/{profileName}"
            );
        }

        [Fact]
        public async Task DeleteVectorizationPipelineAsync_ShouldThrowArgumentException_WhenPipelineNameIsInvalid()
        {
            // Arrange
            var invalidPipelineName = "";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _vectorizationClient.DeleteVectorizationPipelineAsync(invalidPipelineName));
        }

        [Fact]
        public async Task DeleteTextPartitioningProfileAsync_ShouldThrowArgumentException_WhenProfileNameIsInvalid()
        {
            // Arrange
            var invalidProfileName = "";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _vectorizationClient.DeleteTextPartitioningProfileAsync(invalidProfileName));
        }

        [Fact]
        public async Task DeleteTextEmbeddingProfileAsync_ShouldThrowArgumentException_WhenProfileNameIsInvalid()
        {
            // Arrange
            var invalidProfileName = "";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _vectorizationClient.DeleteTextEmbeddingProfileAsync(invalidProfileName));
        }

        [Fact]
        public async Task DeleteIndexingProfileAsync_ShouldThrowArgumentException_WhenProfileNameIsInvalid()
        {
            // Arrange
            var invalidProfileName = "";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _vectorizationClient.DeleteIndexingProfileAsync(invalidProfileName));
        }

        #endregion Delete Methods Tests
    }
}
