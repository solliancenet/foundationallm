using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Common.Clients
{
    /// <summary>
    /// Runs a resource using a polling mechanism to check for completion.
    /// </summary>
    /// <typeparam name="TResource">The type of the runnable resource to run.</typeparam>
    public class PollingResourceRunner<TResource>
        where TResource: ResourceBase, IRunnableResource
    {
        /// <summary>
        /// Runs the resource and waits for it to complete.
        /// </summary>
        /// <param name="instanceId">The FoundationaLLM instance identifier.</param>
        /// <param name="resourceProviderService">The resource provider service that manages the runnable resource.</param>
        /// <param name="resource">The runnable resource to run.</param>
        /// <param name="pollingInterval">The interval to poll for the status.</param>
        /// <param name="maxWaitInterval">The maximum interval to wait for the resource run to complete.</param>
        /// <param name="logger">The logger used for logging.</param>
        /// <param name="userIdentity">The identity of the user driving the run.</param>
        /// <param name="cancellationToken">The cancellation token indicating the need to cancel the run.</param>
        /// <returns></returns>
        public static async Task<bool> Start(
            string instanceId,
            IResourceProviderService resourceProviderService,
            TResource resource,
            TimeSpan pollingInterval,
            TimeSpan maxWaitInterval,
            ILogger logger,
            UnifiedUserIdentity userIdentity,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var upsertResult = await resourceProviderService.UpsertResourceAsync<TResource, ResourceProviderUpsertResult<TResource>>(
                    instanceId,
                    resource,
                    userIdentity);
                var runnableResource = upsertResult.Resource as IRunnableResource;

                if (runnableResource!.Completed)
                    return runnableResource.Successful;

                var pollingStartTime = DateTimeOffset.UtcNow;
                var pollingCounter = 0;

                await Task.Delay(pollingInterval, cancellationToken);

                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        logger.LogInformation("Cancellation requested for resource: {ResourceName}", resource.Name);
                        return false;
                    }

                    var totalPollingTime = DateTimeOffset.UtcNow - pollingStartTime;
                    pollingCounter++;
                    logger.LogInformation(
                        "Polling for runnable resource {RunnableResourceName} (counter: {PollingCounter}, time elapsed: {PollingSeconds} seconds)...",
                        resource.Name,
                        pollingCounter,
                        totalPollingTime.TotalSeconds);

                    var existingResource = await resourceProviderService.GetResourceAsync<TResource>(
                    instanceId,
                    upsertResult.Resource!.Name,
                    userIdentity);
                    runnableResource = existingResource as IRunnableResource;

                    if (runnableResource!.Completed)
                        return runnableResource.Successful;

                    if (totalPollingTime > maxWaitInterval)
                    {
                        logger.LogWarning("Total polling time ({TotalTime} seconds) exceeded to maximum allowed ({MaxTime} seconds).",
                            totalPollingTime.TotalSeconds,
                            maxWaitInterval.TotalSeconds);
                        return false;
                    }

                    await Task.Delay(pollingInterval, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while running resource: {ResourceName}", resource.Name);
                return false;
            }
        }
    }
}
