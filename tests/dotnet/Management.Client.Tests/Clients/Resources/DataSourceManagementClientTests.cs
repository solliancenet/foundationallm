using FoundationaLLM.Client.Management.Clients.Resources;
using FoundationaLLM.Client.Management.Interfaces;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;
using NSubstitute;

namespace Management.Client.Tests.Clients.Resources
{
    public class DataSourceManagementClientTests
    {
        private readonly IManagementRESTClient _mockRestClient;
        private readonly DataSourceManagementClient _dataSourceClient;

        public DataSourceManagementClientTests()
        {
            _mockRestClient = Substitute.For<IManagementRESTClient>();
            _dataSourceClient = new DataSourceManagementClient(_mockRestClient);
        }

        [Fact]
        public async Task GetDataSourcesAsync_ShouldReturnDataSources()
        {
            // Arrange
            var expectedDataSources = new List<ResourceProviderGetResult<DataSourceBase>>
            {
                new ResourceProviderGetResult<DataSourceBase>
                {
                    Resource = new AzureDataLakeDataSource()
                    {
                        Name = "test-datalake-dataSource",
                        Type = DataSourceTypes.AzureDataLake,
                        ConfigurationReferences = new Dictionary<string, string>
                        {
                            {"AuthenticationType", "AzureIdentity"},
                            {"AccountName", "mydatalake01"},
                        },
                        Folders = ["/folder1", "/folder2"],
                    },
                    Actions = [],
                    Roles = []
                },
                new ResourceProviderGetResult<DataSourceBase>()
                {
                    Resource = new AzureSQLDatabaseDataSource()
                    {
                        Name = "test-sql-dataSource",
                        Type = DataSourceTypes.AzureSQLDatabase,
                        ConfigurationReferences = new Dictionary<string, string>
                        {
                            {"AuthenticationType", "ConnectionString"},
                            {"ConnectionString", "secret-connection-string-value"},
                        },
                        Tables = ["Customers", "Orders"],
                    },
                    Actions = [],
                    Roles = []
                }
            };

            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<DataSourceBase>>>(
                    ResourceProviderNames.FoundationaLLM_DataSource,
                    DataSourceResourceTypeNames.DataSources
                )
                .Returns(Task.FromResult(expectedDataSources));

            // Act
            var result = await _dataSourceClient.GetDataSourcesAsync();

            // Assert
            Assert.Equal(expectedDataSources, result);
            await _mockRestClient.Resources.Received(1).GetResourcesAsync<List<ResourceProviderGetResult<DataSourceBase>>>(
                ResourceProviderNames.FoundationaLLM_DataSource,
                DataSourceResourceTypeNames.DataSources
            );
        }

        [Fact]
        public async Task GetDataSourceAsync_ShouldReturnDataSource()
        {
            // Arrange
            var dataSourceName = "test-datalake-dataSource";
            var expectedDataSource = new ResourceProviderGetResult<DataSourceBase>
            {
                Resource = new AzureDataLakeDataSource()
                {
                    Name = dataSourceName,
                    Type = DataSourceTypes.AzureDataLake,
                    ConfigurationReferences = new Dictionary<string, string>
                     {
                         {"AuthenticationType", "AzureIdentity"},
                         {"AccountName", "mydatalake01"},
                     },
                    Folders = ["/folder1", "/folder2"],
                },
                Actions = [],
                Roles = []
            };
            var expectedDataSources = new List<ResourceProviderGetResult<DataSourceBase>> { expectedDataSource };

            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<DataSourceBase>>>(
                    ResourceProviderNames.FoundationaLLM_DataSource,
                    $"{DataSourceResourceTypeNames.DataSources}/{dataSourceName}"
                )
                .Returns(Task.FromResult(expectedDataSources));

            // Act
            var result = await _dataSourceClient.GetDataSourceAsync(dataSourceName);

            // Assert
            Assert.Equal(expectedDataSource, result);
            await _mockRestClient.Resources.Received(1).GetResourcesAsync<List<ResourceProviderGetResult<DataSourceBase>>>(
                ResourceProviderNames.FoundationaLLM_DataSource,
                $"{DataSourceResourceTypeNames.DataSources}/{dataSourceName}"
            );
        }

        [Fact]
        public async Task GetDataSourceAsync_ShouldThrowException_WhenDataSourceNotFound()
        {
            // Arrange
            var dataSourceName = "test-dataSource";
            _mockRestClient.Resources
                .GetResourcesAsync<List<ResourceProviderGetResult<DataSourceBase>>>(
                    ResourceProviderNames.FoundationaLLM_DataSource,
                    $"{DataSourceResourceTypeNames.DataSources}/{dataSourceName}"
                )
                .Returns(Task.FromResult<List<ResourceProviderGetResult<DataSourceBase>>>(null));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _dataSourceClient.GetDataSourceAsync(dataSourceName));
            Assert.Equal($"DataSource '{dataSourceName}' not found.", exception.Message);
        }

        [Fact]
        public async Task CheckDataSourceNameAsync_ShouldReturnCheckResult()
        {
            // Arrange
            var resourceName = new ResourceName { Name = "test-dataSource", Type = "dataSource-type" };
            var expectedCheckResult = new ResourceNameCheckResult
            { 
                Name = resourceName.Name,
                Status = NameCheckResultType.Allowed,
                Message = "Name is allowed"
            };

            _mockRestClient.Resources
                .ExecuteResourceActionAsync<ResourceNameCheckResult>(
                    ResourceProviderNames.FoundationaLLM_DataSource,
                    $"{DataSourceResourceTypeNames.DataSources}/{DataSourceResourceProviderActions.CheckName}",
                    resourceName
                )
                .Returns(Task.FromResult(expectedCheckResult));

            // Act
            var result = await _dataSourceClient.CheckDataSourceNameAsync(resourceName);

            // Assert
            Assert.Equal(expectedCheckResult, result);
            await _mockRestClient.Resources.Received(1).ExecuteResourceActionAsync<ResourceNameCheckResult>(
                ResourceProviderNames.FoundationaLLM_DataSource,
                $"{DataSourceResourceTypeNames.DataSources}/{DataSourceResourceProviderActions.CheckName}",
                resourceName
            );
        }

        [Fact]
        public async Task CheckDataSourceNameAsync_ShouldThrowArgumentException_WhenResourceNameIsInvalid()
        {
            // Arrange
            var invalidResourceName = new ResourceName { Name = "", Type = "" };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _dataSourceClient.CheckDataSourceNameAsync(invalidResourceName));
        }

        [Fact]
        public async Task PurgeDataSourceAsync_ShouldReturnPurgeResult()
        {
            // Arrange
            var dataSourceName = "test-dataSource";
            var expectedPurgeResult = new ResourceProviderActionResult(true);

            _mockRestClient.Resources
                .ExecuteResourceActionAsync<ResourceProviderActionResult>(
                    ResourceProviderNames.FoundationaLLM_DataSource,
                    $"{DataSourceResourceTypeNames.DataSources}/{dataSourceName}/{DataSourceResourceProviderActions.Purge}",
                    Arg.Any<object>()
                )
                .Returns(Task.FromResult(expectedPurgeResult));

            // Act
            var result = await _dataSourceClient.PurgeDataSourceAsync(dataSourceName);

            // Assert
            Assert.Equal(expectedPurgeResult, result);
            await _mockRestClient.Resources.Received(1).ExecuteResourceActionAsync<ResourceProviderActionResult>(
                ResourceProviderNames.FoundationaLLM_DataSource,
                $"{DataSourceResourceTypeNames.DataSources}/{dataSourceName}/{DataSourceResourceProviderActions.Purge}",
                Arg.Any<object>()
            );
        }

        [Fact]
        public async Task PurgeDataSourceAsync_ShouldThrowArgumentException_WhenDataSourceNameIsInvalid()
        {
            // Arrange
            var invalidDataSourceName = "";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _dataSourceClient.PurgeDataSourceAsync(invalidDataSourceName));
        }

        [Fact]
        public async Task FilterDataSourceAsync_ShouldReturnFilteredDataSources()
        {
            // Arrange
            var resourceFilter = new ResourceFilter
            {
                Default = true
            };
            var expectedDataSources = new List<DataSourceBase>
            {
                new DataSourceBase
                {
                    Name = "test-dataSource",
                    Description = "Default data source"
                }
            };

            _mockRestClient.Resources
                .ExecuteResourceActionAsync<List<DataSourceBase>>(
                    ResourceProviderNames.FoundationaLLM_DataSource,
                    $"{DataSourceResourceTypeNames.DataSources}/{DataSourceResourceProviderActions.Filter}",
                    resourceFilter
                )
                .Returns(Task.FromResult(expectedDataSources));

            // Act
            var result = await _dataSourceClient.FilterDataSourceAsync(resourceFilter);

            // Assert
            Assert.Equal(expectedDataSources, result);
            await _mockRestClient.Resources.Received(1).ExecuteResourceActionAsync<List<DataSourceBase>>(
                ResourceProviderNames.FoundationaLLM_DataSource,
                $"{DataSourceResourceTypeNames.DataSources}/{DataSourceResourceProviderActions.Filter}",
                resourceFilter
            );
        }

        [Fact]
        public async Task UpsertDataSourceAsync_ShouldReturnUpsertResult()
        {
            // Arrange
            var dataSource = new DataSourceBase { Name = "test-dataSource" };
            var expectedUpsertResult = new ResourceProviderUpsertResult
            {
                ObjectId = "test-object-id"
            };

            _mockRestClient.Resources
                .UpsertResourceAsync(
                    ResourceProviderNames.FoundationaLLM_DataSource,
                    $"{DataSourceResourceTypeNames.DataSources}/{dataSource.Name}",
                    dataSource
                )
                .Returns(Task.FromResult(expectedUpsertResult));

            // Act
            var result = await _dataSourceClient.UpsertDataSourceAsync(dataSource);

            // Assert
            Assert.Equal(expectedUpsertResult, result);
            await _mockRestClient.Resources.Received(1).UpsertResourceAsync(
                ResourceProviderNames.FoundationaLLM_DataSource,
                $"{DataSourceResourceTypeNames.DataSources}/{dataSource.Name}",
                dataSource
            );
        }

        [Fact]
        public async Task DeleteDataSourceAsync_ShouldCallDeleteResource()
        {
            // Arrange
            var dataSourceName = "test-dataSource";

            // Act
            await _dataSourceClient.DeleteDataSourceAsync(dataSourceName);

            // Assert
            await _mockRestClient.Resources.Received(1).DeleteResourceAsync(
                ResourceProviderNames.FoundationaLLM_DataSource,
                $"{DataSourceResourceTypeNames.DataSources}/{dataSourceName}"
            );
        }
    }
}
