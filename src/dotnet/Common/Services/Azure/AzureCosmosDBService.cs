using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.Common.Models.Configuration.Users;
using FoundationaLLM.Common.Models.Conversation;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using System.Diagnostics;

namespace FoundationaLLM.Common.Services
{
    /// <summary>
    /// Service to access Azure Cosmos DB for NoSQL.
    /// </summary>
    public class AzureCosmosDBService : ICosmosDBService
    {
        private Container _sessions;
        private Container _userSessions;
        private readonly Lazy<Task<Container>> _userProfiles;
        private Task<Container> _userProfilesTask => _userProfiles.Value;
        private readonly Database _database;
        private readonly Dictionary<string, Container> _containers;
        private readonly CosmosDbSettings _settings;
        private readonly ResiliencePipeline _resiliencePipeline;
        private readonly ILogger _logger;

        private const string SoftDeleteQueryRestriction = " (not IS_DEFINED(c.deleted) OR c.deleted = false)";

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureCosmosDBService"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="CosmosDbSettings"/> settings retrieved
        /// by the injected <see cref="IOptions{TOptions}"/>.</param>
        /// <param name="client">The Cosmos DB client.</param>
        /// <param name="logger">The logging interface used to log under the
        /// <see cref="AzureCosmosDBService"></see> type name.</param>
        /// <exception cref="ArgumentException">Thrown if any of the required settings
        /// are null or empty.</exception>
        public AzureCosmosDBService(
            IOptions<CosmosDbSettings> settings,
            CosmosClient client,
            ILogger<AzureCosmosDBService> logger)
        {
            _settings = settings.Value;
            ArgumentException.ThrowIfNullOrEmpty(_settings.Endpoint);
            ArgumentException.ThrowIfNullOrEmpty(_settings.Database);
            ArgumentException.ThrowIfNullOrEmpty(_settings.Containers);

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

            _resiliencePipeline = new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                MaxRetryAttempts = 6,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    var exception = args.Outcome.Exception!;
                    _logger.LogWarning($"Cosmos DB resilience strategy handling: {exception.Message}");
                    _logger.LogWarning($" ... automatically delaying for {args.RetryDelay.TotalMilliseconds}ms.");
                    return default;
                }
            }).Build();

            var database = client?.GetDatabase(_settings.Database);

            _database = database ??
                        throw new ArgumentException("Unable to connect to existing Azure Cosmos DB database.");

            // Dictionary of container references for all containers listed in config.
            _containers = new Dictionary<string, Container>();

            var containers = _settings.Containers.Split(',').ToList();

            foreach (var containerName in containers)
            {
                var container = database?.GetContainer(containerName.Trim()) ??
                                throw new ArgumentException(
                                    "Unable to connect to existing Azure Cosmos DB container or database.");

                _containers.Add(containerName.Trim(), container);
            }

            _sessions = database?.GetContainer(CosmosDbContainers.Sessions) ??
                        throw new ArgumentException(
                            $"Unable to connect to existing Azure Cosmos DB container ({CosmosDbContainers.Sessions}).");
            _userSessions = database?.GetContainer(CosmosDbContainers.UserSessions) ??
                            throw new ArgumentException(
                                $"Unable to connect to existing Azure Cosmos DB container ({CosmosDbContainers.UserSessions}).");
            _userProfiles = new Lazy<Task<Container>>(InitializeUserProfilesContainer);

            _logger.LogInformation("Cosmos DB service initialized.");
        }

        private async Task<Container> InitializeUserProfilesContainer() =>
            await _resiliencePipeline.ExecuteAsync<Container>(async token => await _database?.CreateContainerIfNotExistsAsync(new ContainerProperties(CosmosDbContainers.UserProfiles,
                "/upn"), ThroughputProperties.CreateAutoscaleThroughput(1000), cancellationToken: token)!);

        /// <inheritdoc/>
        public async Task<List<Conversation>> GetSessionsAsync(string type, string upn, CancellationToken cancellationToken = default)
        {
            var query = new QueryDefinition($"SELECT DISTINCT * FROM c WHERE c.type = @type AND c.upn = @upn AND {SoftDeleteQueryRestriction} ORDER BY c._ts DESC")
                .WithParameter("@type", type)
                .WithParameter("@upn", upn);

            var response = _userSessions.GetItemQueryIterator<Conversation>(query);

            List<Conversation> output = [];
            while (response.HasMoreResults)
            {
                var results = await response.ReadNextAsync(cancellationToken);
                output.AddRange(results);
            }

            return output;
        }

        /// <inheritdoc/>
        public async Task<Conversation> GetSessionAsync(string id, CancellationToken cancellationToken = default)
        {
            var session = await _sessions.ReadItemAsync<Conversation>(
                id: id,
                partitionKey: new PartitionKey(id),
                cancellationToken: cancellationToken);
            
            return session;
        }

        /// <inheritdoc/>
        public async Task<List<Message>> GetSessionMessagesAsync(string sessionId, string upn, CancellationToken cancellationToken = default)
        {
            var query =
                new QueryDefinition($"SELECT * FROM c WHERE c.sessionId = @sessionId AND c.type = @type AND c.upn = @upn AND {SoftDeleteQueryRestriction}")
                    .WithParameter("@sessionId", sessionId)
                    .WithParameter("@type", nameof(Message))
                    .WithParameter("@upn", upn);

            var results = _sessions.GetItemQueryIterator<Message>(query);

            List<Message> output = new();
            while (results.HasMoreResults)
            {
                var response = await results.ReadNextAsync(cancellationToken);
                output.AddRange(response);
            }

            return output;
        }

        /// <inheritdoc/>
        public async Task<Conversation> InsertSessionAsync(Conversation session, CancellationToken cancellationToken = default)
        {
            PartitionKey partitionKey = new(session.SessionId);
            return await _sessions.CreateItemAsync(
                item: session,
                partitionKey: partitionKey,
                cancellationToken: cancellationToken
            );
        }

        /// <inheritdoc/>
        public async Task<Message> InsertMessageAsync(Message message, CancellationToken cancellationToken = default)
        {
            PartitionKey partitionKey = new(message.SessionId);
            return await _sessions.CreateItemAsync(
                item: message,
                partitionKey: partitionKey,
                cancellationToken: cancellationToken
            );
        }

        /// <inheritdoc/>
        public async Task<Message> UpdateMessageAsync(Message message, CancellationToken cancellationToken = default)
        {
            PartitionKey partitionKey = new(message.SessionId);
            return await _sessions.ReplaceItemAsync(
                item: message,
                id: message.Id,
                partitionKey: partitionKey,
                cancellationToken: cancellationToken
            );
        }

        /// <inheritdoc/>
        public async Task<Message> UpdateMessageRatingAsync(string id, string sessionId, bool? rating, CancellationToken cancellationToken = default)
        {
            var response = await _sessions.PatchItemAsync<Message>(
                id: id,
                partitionKey: new PartitionKey(sessionId),
                patchOperations: new[]
                {
                    PatchOperation.Set("/rating", rating),
                },
                cancellationToken: cancellationToken
            );
            return response.Resource;
        }

        /// <inheritdoc/>
        public async Task<Conversation> UpdateSessionAsync(Conversation session, CancellationToken cancellationToken = default)
        {
            PartitionKey partitionKey = new(session.SessionId);
            return await _sessions.ReplaceItemAsync(
                item: session,
                id: session.Id,
                partitionKey: partitionKey,
                cancellationToken: cancellationToken
            );
        }

        /// <inheritdoc/>
        public async Task<Conversation> UpdateSessionNameAsync(string id, string sessionName, CancellationToken cancellationToken = default)
        {
            var response = await _sessions.PatchItemAsync<Conversation>(
                id: id,
                partitionKey: new PartitionKey(id),
                patchOperations: new[]
                {
                    PatchOperation.Set("/name", sessionName),
                },
                cancellationToken: cancellationToken
            );
            return response.Resource;
        }

        /// <inheritdoc/>
        public async Task UpsertSessionBatchAsync(params dynamic[] messages)
        {
            if (messages.Select(m => m.SessionId).Distinct().Count() > 1)
            {
                throw new ArgumentException("All items must have the same partition key.");
            }

            PartitionKey partitionKey = new(messages.First().SessionId);
            var batch = _sessions.CreateTransactionalBatch(partitionKey);
            foreach (var message in messages)
            {
                batch.UpsertItem(
                    item: message
                );
            }

            await batch.ExecuteAsync();
        }

        /// <inheritdoc/>
        public async Task UpsertUserSessionAsync(Conversation session, CancellationToken cancellationToken = default)
        {
            PartitionKey partitionKey = new(session.UPN);
            await _userSessions.UpsertItemAsync(
               item: session,
               partitionKey: partitionKey,
               cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public async Task DeleteSessionAndMessagesAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            PartitionKey partitionKey = new(sessionId);

            // TODO: await container.DeleteAllItemsByPartitionKeyStreamAsync(partitionKey);

            var query = new QueryDefinition($"SELECT * FROM c WHERE c.sessionId = @sessionId AND {SoftDeleteQueryRestriction}")
                .WithParameter("@sessionId", sessionId);

            var response = _sessions.GetItemQueryIterator<dynamic>(query);

            _logger.LogInformation($"Deleting {sessionId} session and related messages.");

            var batch = _sessions.CreateTransactionalBatch(partitionKey);
            var count = 0;

            // Local function to execute and reset the batch.
            async Task ExecuteBatchAsync()
            {
                if (count > 0) // Execute the batch only if it has any items.
                {
                    await batch.ExecuteAsync(cancellationToken);
                    count = 0;
                    batch = _sessions.CreateTransactionalBatch(partitionKey);
                }
            }

            while (response.HasMoreResults)
            {
                var results = await response.ReadNextAsync(cancellationToken);
                foreach (var item in results)
                {
                    item.deleted = true;
                    batch.UpsertItem(item);
                    count++;
                    if (count >= 100) // Execute the batch after adding 100 items (100 actions per batch execution is the limit).
                    {
                        await ExecuteBatchAsync();
                    }
                }
            }

            await ExecuteBatchAsync();
        }

        /// <inheritdoc/>
        public async Task<string> GetVectorSearchDocumentsAsync(List<DocumentVector> vectorDocuments, CancellationToken cancellationToken = default)
        {

            var searchDocuments = new List<string>();

            foreach (var document in vectorDocuments)
            {

                try
                {
                    var response = await _containers[document.containerName].ReadItemStreamAsync(
                        document.itemId, new PartitionKey(document.partitionKey),
                        cancellationToken: cancellationToken);


                    if ((int)response.StatusCode < 200 || (int)response.StatusCode >= 400)
                        _logger.LogError(
                            $"Failed to retrieve an item for id '{document.itemId}' - status code '{response.StatusCode}");

                    if (response.Content == null)
                    {
                        _logger.LogInformation(
                            $"Null content received for document '{document.itemId}' - status code '{response.StatusCode}");
                        continue;
                    }

                    string item;
                    using (var sr = new StreamReader(response.Content))
                        item = await sr.ReadToEndAsync(cancellationToken);

                    searchDocuments.Add(item);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);

                }
            }

            var resultDocuments = string.Join(Environment.NewLine + "-", searchDocuments);

            return resultDocuments;

        }

        /// <inheritdoc/>
        public async Task<CompletionPrompt> GetCompletionPrompt(string sessionId, string completionPromptId) =>
            await _sessions.ReadItemAsync<CompletionPrompt>(
                id: completionPromptId,
                partitionKey: new PartitionKey(sessionId));

        /// <inheritdoc/>
        public async Task<UserProfile> GetUserProfileAsync(string upn, CancellationToken cancellationToken = default)
        {
            var userProfiles = await _userProfilesTask;

            var response = await userProfiles.ReadItemAsync<UserProfile>(
                id: upn,
                partitionKey: new PartitionKey(upn),
                cancellationToken: cancellationToken);

            if (response == null)
            {
                var newUserProfile = new UserProfile(upn);
                await UpsertUserProfileAsync(newUserProfile, cancellationToken);
                return newUserProfile;
            }

            return response;
        }

        /// <inheritdoc/>
        public async Task UpsertUserProfileAsync(UserProfile userProfile, CancellationToken cancellationToken = default)
        {
            var userProfiles = await _userProfilesTask;
            PartitionKey partitionKey = new(userProfile.UPN);
            await userProfiles.UpsertItemAsync(
                item: userProfile,
                partitionKey: partitionKey,
                cancellationToken: cancellationToken);
        }
    }
}
