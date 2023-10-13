using FoundationaLLM.Core.Models.Configuration;

namespace FoundationaLLM.Core.Tests.Models.Configuration
{
    public class CosmosDbSettingsTests
    {
        [Fact]
        public static void Constructor_ShouldInitializeProperties()
        {
            // Arrange
            string expectedendpoint = "endpoint";
            string expectedkey = "key"; 
            string expecteddatabase = "database";
            string expectedcontainers = "containers";
            string expectedmonitoredContainers = "monitoredContainers";
            string expectedchangeFeedLeaseContainer = "changeFeedLeaseContainer";
            string expectedchangeFeedSourceContainer = "changeFeedSourceContainer";
            bool expectedenableTracing = false;

            // Act
            var cosmosDbSettings = CreateCosmosDbSettings(
                expectedendpoint, 
                expectedkey, 
                expecteddatabase, 
                expectedcontainers,
                expectedmonitoredContainers,
                expectedchangeFeedLeaseContainer, 
                expectedchangeFeedSourceContainer,
                expectedenableTracing
            );

            // Assert
            Assert.Equal(expectedendpoint, cosmosDbSettings.Endpoint);
            Assert.Equal(expectedkey, cosmosDbSettings.Key);
            Assert.Equal(expecteddatabase, cosmosDbSettings.Database);
            Assert.Equal(expectedcontainers, cosmosDbSettings.Containers);
            Assert.Equal(expectedmonitoredContainers, cosmosDbSettings.MonitoredContainers);
            Assert.Equal(expectedchangeFeedLeaseContainer, cosmosDbSettings.ChangeFeedLeaseContainer);
            Assert.Equal(expectedchangeFeedSourceContainer, cosmosDbSettings.ChangeFeedSourceContainer);
            Assert.Equal(expectedenableTracing, cosmosDbSettings.EnableTracing);
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
