using Azure.Identity;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.Common.Models.Conversation;
using FoundationaLLM.Core.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace FoundationaLLM.Core.Services
{
    /// <inheritdoc/>
    public class AzureCosmosDBChangeFeedService : ICosmosDbChangeFeedService
    {
        private readonly Database _database;
        private readonly Container _sessions;
        private readonly Container _leases;

        private ChangeFeedProcessor? _changeFeedProcessorProcessUserSessions;

        private readonly ILogger<AzureCosmosDBChangeFeedService> _logger;
        private readonly IAzureCosmosDBService _cosmosDBService;
        private readonly ResiliencePipeline _resiliencePipeline;

        private bool _changeFeedsInitialized = false;

        /// <summary>
        /// Gets a value indicating whether the change feeds have been initialized.
        /// </summary>
        public bool IsInitialized => _changeFeedsInitialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureCosmosDBChangeFeedService"/> class.
        /// </summary>
        /// <param name="logger">The logging interface used to log under the
        /// <see cref="AzureCosmosDBChangeFeedService"/> type name.</param>
        /// <param name="cosmosDBService">Contains standard methods for managing data stored
        /// within the Azure Cosmos DB workspace.</param>
        /// <param name="settings">The <see cref="CosmosDbSettings"/> settings retrieved
        /// by the injected <see cref="IOptions{TOptions}"/>.</param>
        /// <exception cref="ArgumentException">Thrown if any of the required settings
        /// are null or empty.</exception>
        public AzureCosmosDBChangeFeedService(ILogger<AzureCosmosDBChangeFeedService> logger,
            IAzureCosmosDBService cosmosDBService,
            IOptions<CosmosDbSettings> settings)
        {
            _cosmosDBService = cosmosDBService;
            _logger = logger;

            CosmosSerializationOptions options = new()
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            };
            var client = new CosmosClientBuilder(settings.Value.Endpoint, new DefaultAzureCredential())
                .WithSerializerOptions(options)
                .WithConnectionModeGateway()
                .Build();

            var database = client.GetDatabase(settings.Value.Database);

            _database = database ??
                        throw new ArgumentException($"Unable to connect to existing Azure Cosmos DB database ({settings.Value.Database}).");
            _sessions = database?.GetContainer(AzureCosmosDBContainers.Sessions) ??
                        throw new ArgumentException($"Unable to connect to existing Azure Cosmos DB container ({AzureCosmosDBContainers.Sessions}).");
            _leases = database?.GetContainer(AzureCosmosDBContainers.Leases) ??
                      throw new ArgumentException($"Unable to connect to existing Azure Cosmos DB container ({AzureCosmosDBContainers.Leases}).");

            _resiliencePipeline = new ResiliencePipelineBuilder().AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                MaxRetryAttempts = 6,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                OnRetry = args =>
                {
                    var exception = args.Outcome.Exception!;
                    _logger.LogWarning($"Change Feed processor resilience strategy handling: {exception.Message}");
                    _logger.LogWarning($" ... automatically delaying for {args.RetryDelay.TotalMilliseconds}ms.");
                    return default;
                }
            }).Build();
        }

        /// <inheritdoc/>
        public async Task StartChangeFeedProcessorsAsync()
        {
            _logger.LogInformation("Starting Change Feed Processors...");
            try
            {
                _changeFeedProcessorProcessUserSessions = _sessions
                    .GetChangeFeedProcessorBuilder<Conversation>("ProcessUserSessions", ProcessUserSessionsChangeFeedHandler)
                    .WithInstanceName($"{Guid.NewGuid()}_ProcessUserSessions") // Prefix with a unique name to allow multiple instances to run at the same time.
                    .WithLeaseContainer(_leases)
                    .Build();

                await _changeFeedProcessorProcessUserSessions.StartAsync();

                _changeFeedsInitialized = true;
                _logger.LogInformation("Change Feed Processors started.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing change feed processors.");
            }
        }

        /// <inheritdoc/>
        public async Task StopChangeFeedProcessorAsync()
        {
            // Stop the ChangeFeedProcessor
            _logger.LogInformation("Stopping Change Feed Processors...");

            if (_changeFeedProcessorProcessUserSessions != null) await _changeFeedProcessorProcessUserSessions.StopAsync();

            _logger.LogInformation("Change Feed Processors stopped.");
        }

        private async Task ProcessUserSessionsChangeFeedHandler(
            ChangeFeedProcessorContext context,
            IReadOnlyCollection<Conversation> input,
            CancellationToken cancellationToken)
        {
            using var logScope = _logger.BeginScope("Cosmos DB Change Feed Processor: ProcessUserSessionsChangeFeedHandler");

            var sessions = input.Where(i => i.Type == nameof(Conversation)).ToArray();

            _logger.LogInformation("Cosmos DB Change Feed Processor: Processing {count} changes...", sessions.Count());

            await Parallel.ForEachAsync(sessions, cancellationToken, async (record, token) =>
            {
                try
                {
                    
                    await _resiliencePipeline.ExecuteAsync(async token =>
                    {
                        await _cosmosDBService.UpsertUserSessionAsync(record, token);
                    }, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                }
            });
        }
    }
}
