using FoundationaLLM.Core.Models.Configuration;

namespace FoundationaLLM.Core.Tests.Models.Configuration
{
    public class CosmosDbSettingsTests
    {
        [Fact]
        public static void Constructor_ShouldInitializeProperties()
        {
            // Arrange
            string expectedEndpoint = "endpoint";
            string expectedKey = "key"; 
            string expectedDatabase = "database";
            string expectedContainers = "containers";
            string expectedMonitoredContainers = "monitoredContainers";
            string expectedChangeFeedLeaseContainer = "changeFeedLeaseContainer";
            string expectedChangeFeedSourceContainer = "changeFeedSourceContainer";
            bool expectedEnableTracing = false;

            // Act
            var cosmosDbSettings = CreateCosmosDbSettings(
                expectedEndpoint, 
                expectedKey, 
                expectedDatabase, 
                expectedContainers,
                expectedMonitoredContainers,
                expectedChangeFeedLeaseContainer, 
                expectedChangeFeedSourceContainer,
                expectedEnableTracing
            );

            // Assert
            Assert.Equal(expectedEndpoint, cosmosDbSettings.Endpoint);
            Assert.Equal(expectedKey, cosmosDbSettings.Key);
            Assert.Equal(expectedDatabase, cosmosDbSettings.Database);
            Assert.Equal(expectedContainers, cosmosDbSettings.Containers);
            Assert.Equal(expectedMonitoredContainers, cosmosDbSettings.MonitoredContainers);
            Assert.Equal(expectedChangeFeedLeaseContainer, cosmosDbSettings.ChangeFeedLeaseContainer);
            Assert.Equal(expectedChangeFeedSourceContainer, cosmosDbSettings.ChangeFeedSourceContainer);
            Assert.Equal(expectedEnableTracing, cosmosDbSettings.EnableTracing);
        }

        private static CosmosDbSettings CreateCosmosDbSettings(string endpoint, string key, string database, string containers, 
            string monitoredContainers, string changeFeedLeaseContainer, string changeFeedSourceContainer, bool enableTracing)
        {
            return new CosmosDbSettings() 
            {
                Endpoint = endpoint,
                Key = key,
                Database = database,
                Containers = containers,
                MonitoredContainers = monitoredContainers,
                ChangeFeedLeaseContainer = changeFeedLeaseContainer,
                ChangeFeedSourceContainer = changeFeedSourceContainer,
                EnableTracing = enableTracing
            };
        }
    }
}
