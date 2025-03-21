﻿using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Orchestration;
using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.State.Exceptions;
using FoundationaLLM.State.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Dynamic;
using System.Text.Json;

namespace FoundationaLLM.State.Services
{
    /// <summary>
    /// Service to access Azure Cosmos DB for NoSQL.
    /// </summary>
    public class CosmosDbService : ICosmosDbService
    {
        private readonly Container _state;
        private readonly AzureCosmosDBSettings _settings;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbService"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="AzureCosmosDBSettings"/> settings retrieved
        /// by the injected <see cref="IOptions{TOptions}"/>.</param>
        /// <param name="client">The Cosmos DB client.</param>
        /// <param name="logger">The logging interface used to log under the
        /// <see cref="CosmosDbService"></see> type name.</param>
        /// <exception cref="ArgumentException">Thrown if any of the required settings
        /// are null or empty.</exception>
        public CosmosDbService(
            IOptions<AzureCosmosDBSettings> settings,
            CosmosClient client,
            ILogger<CosmosDbService> logger)
        {
            _settings = settings.Value;
            ArgumentException.ThrowIfNullOrEmpty(_settings.Endpoint);
            ArgumentException.ThrowIfNullOrEmpty(_settings.Database);

            _logger = logger;
            _logger.LogInformation("Initializing Cosmos DB service.");

            if (!_settings.EnableTracing)
            {
                var defaultTrace =
                    Type.GetType("Microsoft.Azure.Cosmos.Core.Trace.DefaultTrace,Microsoft.Azure.Cosmos.Direct");
                var traceSource = (TraceSource) defaultTrace?.GetProperty("TraceSource")?.GetValue(null)!;
                traceSource.Switch.Level = SourceLevels.All;
                traceSource.Listeners.Clear();
            }

            var database = client?.GetDatabase(_settings.Database);

            if (database == null)
            {
                throw new ArgumentException("Unable to connect to existing Azure Cosmos DB database.");
            }

            _state = database?.GetContainer(AzureCosmosDBContainers.State) ??
                 throw new ArgumentException(
                     $"Unable to connect to existing Azure Cosmos DB container ({AzureCosmosDBContainers.State}).");

            _logger.LogInformation("Cosmos DB service initialized.");
        }

        /// <inheritdoc/>
        public async Task<List<LongRunningOperation>> GetLongRunningOperations(CancellationToken cancellationToken = default)
        {
            try
            {
                var query = new QueryDefinition(
                        $"SELECT DISTINCT * FROM c WHERE c.type = @type ORDER BY c._ts DESC")
                    .WithParameter("@type", LongRunningOperationTypes.LongRunningOperation);

                var response = _state.GetItemQueryIterator<LongRunningOperation>(query);

                List<LongRunningOperation> output = new();
                while (response.HasMoreResults)
                {
                    var results = await response.ReadNextAsync(cancellationToken);
                    output.AddRange(results);
                }

                return output;
            }
            catch (CosmosException cosmosException)
            {
                _logger.LogError(cosmosException, "Cosmos DB error occurred while retrieving long-running operations.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving long-running operations.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<LongRunningOperation> GetLongRunningOperation(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                var record = await _state.ReadItemAsync<LongRunningOperation>(
                    id: id,
                    partitionKey: new PartitionKey(id),
                    cancellationToken: cancellationToken);

                var result = await GetLongRunningOperationResult(id, cancellationToken);
                record.Resource.Result = result;

                return record.Resource;
            }
            catch (Exception ex)
            {
                if (ex is CosmosException cosmosException)
                {
                    if ((int)cosmosException.StatusCode == StatusCodes.Status404NotFound)
                    {
                        throw new StateException(
                            message: $"An operation with the id '{id}' does not exist.",
                            statusCode: StatusCodes.Status404NotFound
                        );
                    }
                }
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<List<LongRunningOperationLogEntry>> GetLongRunningOperationLogEntries(string operationId, CancellationToken cancellationToken = default)
        {
            try
            {
                var query =
                    new QueryDefinition($"SELECT * FROM c WHERE c.operation_id = @operationId AND c.type = @type")
                        .WithParameter("@operationId", operationId)
                        .WithParameter("@type", LongRunningOperationTypes.LongRunningOperationLogEntry);

                var results = _state.GetItemQueryIterator<LongRunningOperationLogEntry>(query);

                List<LongRunningOperationLogEntry> output = new();
                while (results.HasMoreResults)
                {
                    var response = await results.ReadNextAsync(cancellationToken);
                    output.AddRange(response);
                }

                return output;
            }
            catch (CosmosException cosmosException)
            {
                _logger.LogError(cosmosException, "Cosmos DB error occurred while retrieving long-running operation log entries.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving long-running operation log entries.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<JsonDocument?> GetLongRunningOperationResult(string operationId, CancellationToken cancellationToken = default)
        {
            try
            {
                var query =
                    new QueryDefinition($"SELECT TOP 1 * FROM c WHERE c.operation_id = @operationId AND c.type = @type ORDER BY c._ts DESC")
                        .WithParameter("@operationId", operationId)
                        .WithParameter("@type", LongRunningOperationTypes.LongRunningOperationResult);

                var results = _state.GetItemQueryIterator<JsonDocument>(query);

                // There should just be a single result that has the operation_id and type. Get that result and return it.
                if (results.HasMoreResults)
                {
                    var response = await results.ReadNextAsync(cancellationToken);
                    return response.FirstOrDefault();
                }

                return default;
            }
            catch (CosmosException cosmosException)
            {
                _logger.LogError(cosmosException, "Cosmos DB error occurred while retrieving long-running operation result.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving long-running operation result.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<LongRunningOperation> UpsertLongRunningOperation(LongRunningOperation operation, CancellationToken cancellationToken = default)
        {
            try
            {
                PartitionKey partitionKey = new(operation.OperationId);
                var batch = _state.CreateTransactionalBatch(partitionKey);
                batch.UpsertItem<LongRunningOperation>(
                    operation
                );
                batch.CreateItem<LongRunningOperationLogEntry>(
                    new LongRunningOperationLogEntry(operation.OperationId!, operation.Status, operation.StatusMessage, operation.UPN)
                );

                var result = await batch.ExecuteAsync(cancellationToken);

                if (result.IsSuccessStatusCode)
                {
                    var operationResult = result.GetOperationResultAtIndex<LongRunningOperation>(0);
                    return operationResult.Resource;
                }
                else
                {
                    throw new Exception($"Failed to upsert long running operation. Status code: {result.StatusCode}, Error message: {result.ErrorMessage}");
                }
            }
            catch (CosmosException cosmosException)
            {
                _logger.LogError(cosmosException, "Cosmos DB error occurred while upserting long-running operation.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while upserting long-running operation.");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<object?> UpsertLongRunningOperationResult(dynamic operationResult, CancellationToken cancellationToken = default)
        {
            try
            {
                string operationId;
                if (operationResult.operation_id is JsonElement { ValueKind: JsonValueKind.String } operationIdElement)
                {
                    operationId = operationIdElement.GetString()!;
                }
                else if (operationResult.operation_id is string operationIdStr)
                {
                    operationId = operationIdStr;
                }
                else
                {
                    throw new ArgumentException("OperationResult must have a valid operation_id.");
                }

                if (string.IsNullOrWhiteSpace(operationId))
                {
                    throw new ArgumentException("OperationResult must have an operation_id.");
                }

                operationResult.type = LongRunningOperationTypes.LongRunningOperationResult;

                PartitionKey partitionKey = new(operationId);
                ItemResponse<ExpandoObject> result = await _state.UpsertItemAsync(
                    item: operationResult,
                    partitionKey: partitionKey,
                    cancellationToken: cancellationToken
                );

                return result.Resource;
            }
            catch (CosmosException cosmosException)
            {
                _logger.LogError(cosmosException, "Cosmos DB error occurred while upserting long-running operation result.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while upserting long-running operation result.");
                throw;
            }
        }
    }
}
