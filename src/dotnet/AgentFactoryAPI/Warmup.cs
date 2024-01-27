using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;
using FoundationaLLM.AgentFactory.Core.Services;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Cache;
using System.Runtime.CompilerServices;
using FoundationaLLM.Common.Models.Messages;

namespace FoundationaLLM.AgentFactory.API
{
    /// <summary>
    /// Warms up the cache containing agent-related artifacts
    /// </summary>
    /// <param name="cacheService">The <see cref="ICacheService"/> providing caching operations.</param>
    /// <param name="serviceScopeFactory">The <see cref="IServiceScopeFactory"/> used to create the scope used to instantiate hub API services.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class Warmup(
        ICacheService cacheService,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<Warmup> logger) : BackgroundService
    {
        private readonly ICacheService _cacheService = cacheService;
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
        private IAgentHubAPIService? _agentHubAPIService;
        private IPromptHubAPIService? _promptHubAPIService;
        private IDataSourceHubAPIService? _dataSourceHubAPIService;
        private readonly ILogger<Warmup> _logger = logger;

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("The warmup hosted service is starting to initialize the cache.");

            using var scope = _serviceScopeFactory.CreateScope();
            // As a general rule, it's not recommended to inject scoped services into singletons.
            // In this particular case, we're only using the scoped services for a short while, just to warm up the cache.
            _agentHubAPIService = scope.ServiceProvider.GetService<IAgentHubAPIService>()!;
            _dataSourceHubAPIService = scope.ServiceProvider.GetService<IDataSourceHubAPIService>()!;
            _promptHubAPIService = scope.ServiceProvider.GetService<IPromptHubAPIService>()!;

            try
            {
                var agents = await LoadAgentArtifacts(stoppingToken);
                await LoadDataSourceArtifacts(agents, stoppingToken);
                await LoadPromptArtifacts(agents, stoppingToken);

                _logger.LogInformation("The warmup hosted service has initialized the cache successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The warmup hosted service could not complete the initialization of the cache.");
            }
        }

        private async Task<List<AgentMetadata>> LoadAgentArtifacts(
            CancellationToken cancellationToken)
        {
            if (!await WaitForHubAPIService(_agentHubAPIService!, cancellationToken))
                throw new Exception("Cannot reach the Agent Hub API.");
            _logger.LogInformation("The Agent Hub API is in the ready state.");
            var resolvedAgents = new List<AgentMetadata>();

            var agents = await _agentHubAPIService!.ListAgents();
            if (agents == null
                || agents.Count == 0)
            {
                _logger.LogInformation("The Agent Hub API has no agents registered. Cache initialization is not possible.");
                return new List<AgentMetadata>();
            }

            _logger.LogInformation(
                $"The Agent Hub API service has the following agents registered: {string.Join(", ", agents.Select(a => a.Name))}");

            foreach (var agent in agents)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var agentInfo = await _cacheService.Get<AgentHubResponse>(
                    new CacheKey(agent.Name!, "agent"),
                    async () =>
                    {
                        var resolvedAgent = await _agentHubAPIService!.ResolveRequest(string.Empty, string.Empty, agent.Name);
                        
                        if (resolvedAgent is {Agent: not null})
                            resolvedAgents.Add(resolvedAgent.Agent);
                        return resolvedAgent;
                    },
                    false,
                    TimeSpan.FromHours(1));
                if (agentInfo == null)
                    _logger.LogWarning($"Cannot populate the cache with data related to agent [{agent.Name!}].");
                else
                    _logger.LogInformation($"The cache was populated with data related to agent [{agent.Name!}].");
            }

            return resolvedAgents;
        }

        private async Task LoadDataSourceArtifacts(
            List<AgentMetadata> agents,
            CancellationToken cancellationToken)
        {
            if (!await WaitForHubAPIService(_dataSourceHubAPIService!, cancellationToken))
                throw new Exception("Cannot reach the Data Source Hub API.");
            _logger.LogInformation("The Data Source Hub API is in the ready state.");

            foreach (var agent in agents)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var dataSource = await _cacheService.Get<DataSourceHubResponse>(
                    new CacheKey(agent.Name!, "datasource"),
                    async () => { return await _dataSourceHubAPIService!.ResolveRequest(agent.AllowedDataSourceNames!, string.Empty); },
                    false,
                    TimeSpan.FromHours(1));
                if (dataSource == null
                    || dataSource.DataSources == null
                    || dataSource.DataSources.Count == 0)
                    _logger.LogWarning($"Cannot populate the cache with data source information related to agent [{agent.Name!}].");
                else
                    _logger.LogInformation($"The cache was populated with data source information related to agent [{agent.Name!}].");
            }
        }

        private async Task LoadPromptArtifacts(
            List<AgentMetadata> agents,
            CancellationToken cancellationToken)
        {
            if (!await WaitForHubAPIService(_promptHubAPIService!, cancellationToken))
                throw new Exception("Cannot reach the Prompt Hub API.");
            _logger.LogInformation("The Prompt Hub API is in the ready state.");

            foreach (var agent in agents)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var prompt = await _cacheService.Get<PromptHubResponse>(
                    new CacheKey(agent.Name!, "prompt"),
                    async () => { return await _promptHubAPIService!.ResolveRequest(!string.IsNullOrWhiteSpace(agent.PromptContainer) ? agent.PromptContainer : agent.Name!, string.Empty); },
                    false,
                    TimeSpan.FromHours(1));
                if (prompt == null
                    || prompt.Prompt == null)
                    _logger.LogWarning($"Cannot populate the cache with prompt information related to agent [{agent.Name!}].");
                else
                    _logger.LogInformation($"The cache was populated with prompt information related to agent [{agent.Name!}].");
            }
        }

        /// <summary>
        /// Enters a holding pattern waiting for a hub API service to return status "ready".
        /// </summary>
        /// <param name="hubAPIService">The <see cref="IHubAPIService"/> hub API service to wait for.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that is triggered when the background service must stop.</param>
        /// <returns>True if the hub API responsed in a reasonable amount of time.</returns>
        private async Task<bool> WaitForHubAPIService(
            IHubAPIService hubAPIService,
            CancellationToken cancellationToken)
        {
            for (int i = 0; i < 10; i++)
                // Attempt to call the hub API. By default, this will be retried 5 times with exponential delay.
                if (await hubAPIService.Status() == "ready")
                    // Hub API is ready, move on.
                    return true;
                else
                    if (cancellationToken.IsCancellationRequested)
                        return false;
                    else
                        // Hub API is probably initializing. Wait 10 seconds then attempt a new round of calls.
                        await Task.Delay(10000);

            // Enough time has passed to conclude that the hub API is unreachable.
            return false;
        }
    }
}
