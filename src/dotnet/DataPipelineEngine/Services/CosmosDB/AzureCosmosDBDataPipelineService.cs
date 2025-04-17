using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.Common.Text;
using FoundationaLLM.DataPipelineEngine.Exceptions;
using FoundationaLLM.DataPipelineEngine.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FoundationaLLM.DataPipelineEngine.Services.CosmosDB
{
    /// <summary>
    /// Provides the implementation for the Azure Cosmos DB data pipeline service.
    /// </summary>
    public class AzureCosmosDBDataPipelineService : IAzureCosmosDBDataPipelineService
    {
        protected readonly AzureCosmosDBSettings _settings;
        protected readonly ILogger<AzureCosmosDBDataPipelineService> _logger;

        protected readonly CosmosClient _cosmosClient;
        protected readonly Container _dataPipelineContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureCosmosDBDataPipelineService"/> class.
        /// </summary>
        /// <param name="options">The <see cref="IOptions"/> providing the <see cref="AzureCosmosDBSettings"/>) with the Azure Cosmos DB settings.</param>
        /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
        public AzureCosmosDBDataPipelineService(
            IOptions<AzureCosmosDBSettings> options,
            ILogger<AzureCosmosDBDataPipelineService> logger)
        {
            _settings = options.Value;
            _logger = logger;

            _logger.LogInformation("Initializing the Azure Cosmos DB data pipeline service...");
            _cosmosClient = GetCosmosDBClient();
            _dataPipelineContainer = GetCosmosDBDataPipelineContainer();
            _logger.LogInformation("Successfully initialized the Azure Cosmos DB datra pipeline service.");
        }

        #region Initialization

        private CosmosClient GetCosmosDBClient()
        {
            var opt = new JsonSerializerOptions()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            // Configure CosmosSystemTextJsonSerializer
            var serializer
                = new CosmosSystemTextJsonSerializer(opt);
            var result = new CosmosClientBuilder(
                    _settings.Endpoint,
                    ServiceContext.AzureCredential)
                .WithCustomSerializer(serializer)
                .WithConnectionModeGateway()
                .Build();

            if (!_settings.EnableTracing)
            {
                var defaultTrace =
                    Type.GetType("Microsoft.Azure.Cosmos.Core.Trace.DefaultTrace,Microsoft.Azure.Cosmos.Direct");
                var traceSource = (TraceSource)defaultTrace?.GetProperty("TraceSource")?.GetValue(null)!;
                traceSource.Switch.Level = SourceLevels.All;
                traceSource.Listeners.Clear();
            }

            return result;
        }

        private Container GetCosmosDBDataPipelineContainer()
        {
            var database = _cosmosClient.GetDatabase(_settings.Database)
                ?? throw new DataPipelineServiceException($"The Azure Cosmos DB database '{_settings.Database}' was not found.");

            // The Context container name is the first container in the list.
            var contextContainerName = _settings.Containers.Split(',')[0];
            var result = database.GetContainer(contextContainerName)
                ?? throw new DataPipelineServiceException($"The Azure Cosmos DB container [{contextContainerName}] was not found.");

            return result;
        }

        #endregion

        /// <inheritdoc/>
        public async Task<T> UpsertItemAsync<T>(
            string partitionKey,
            T item,
            CancellationToken cancellationToken = default)
        {
            var response = await _dataPipelineContainer.UpsertItemAsync(
                item: item,
                partitionKey: new PartitionKey(partitionKey),
                cancellationToken: cancellationToken
            );

            return response.Resource;
        }

        public async Task<List<T>> RetrieveItems<T>(
            QueryDefinition query)
        {
            var results = _dataPipelineContainer.GetItemQueryIterator<T>(query);

            List<T> output = [];
            while (results.HasMoreResults)
            {
                var response = await results.ReadNextAsync();
                output.AddRange(response);
            }

            return output;
        }

        public async Task<T> RetrieveItem<T>(string id, string partitionKey)
        {
            var result = await _dataPipelineContainer.ReadItemAsync<T>(id, new PartitionKey(partitionKey));

            return result;
        }
    }
}
