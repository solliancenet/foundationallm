using Azure;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.CognitiveServices;
using Azure.ResourceManager.EventGrid;
using Azure.ResourceManager.EventGrid.Models;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Azure;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Xml;

namespace FoundationaLLM.Common.Services.Azure
{
    /// <summary>
    /// Provides services to interact with the Azure Resource Manager (ARM) infrastructure.
    /// </summary>
    /// <param name="logger">The logger used for logging.</param>
    public class AzureResourceManagerService(
        ILogger<AzureResourceManagerService> logger) : IAzureResourceManagerService
    {
        private readonly ArmClient _armClient = new(DefaultAuthentication.AzureCredential);
        private readonly ILogger<AzureResourceManagerService> _logger = logger;

        /// <inheritdoc/>
        public async Task<bool> CreateEventGridNamespaceTopicSubscription(
            string namespaceResourceId,
            string topicName,
            string topicSubscriptionName,
            List<string> includedEventTypes,
            CancellationToken cancellationToken)
        {
            var namespaceTopicId = ResourceIdentifier.Parse($"{namespaceResourceId}/topics/{topicName}");
            var namespaceTopicResource = _armClient.GetNamespaceTopicResource(namespaceTopicId);
            var namespaceTopic = await namespaceTopicResource.GetAsync(cancellationToken);
            var namespaceTopicSubscriptions = namespaceTopic.Value.GetNamespaceTopicEventSubscriptions();

            var eventSubscription = new NamespaceTopicEventSubscriptionData()
            {
                DeliveryConfiguration = new DeliveryConfiguration()
                {
                    DeliveryMode = DeliveryMode.Queue,
                    Queue = new QueueInfo()
                    {
                        ReceiveLockDurationInSeconds = 60,
                        MaxDeliveryCount = 10,
                        EventTimeToLive = XmlConvert.ToTimeSpan("P1D"),
                    },
                },
                EventDeliverySchema = DeliverySchema.CloudEventSchemaV10,
                FiltersConfiguration = new FiltersConfiguration()
            };

            foreach (var eventType in includedEventTypes)
                eventSubscription.FiltersConfiguration.IncludedEventTypes.Add(eventType);

            var newNamespaceTopicSubscription = await namespaceTopicSubscriptions.CreateOrUpdateAsync(
                WaitUntil.Completed,
                topicSubscriptionName,
                eventSubscription,
                cancellationToken);

            if (!newNamespaceTopicSubscription.HasCompleted)
            {
                _logger.LogError("The Azure resource manager operation returned a result without completing. This is unexpected.");
                return false;
            }
            else if (!newNamespaceTopicSubscription.HasValue)
            {
                _logger.LogError("The Azure resource manager operation completed but did not have a return value. This is unexpected.");
                return false;
            }
            else
                return true;
        }

        /// <inheritdoc/>
        public async Task DeleteEventGridNamespaceTopicSubscription(string namespaceResourceId, string topicName, string topicSubscriptionName, CancellationToken cancellationToken)
        {
            var eventSubscriptionId = ResourceIdentifier.Parse($"{namespaceResourceId}/topics/{topicName}/eventSubscriptions/{topicSubscriptionName}");
            var eventSubscriptionResource = _armClient.GetNamespaceTopicEventSubscriptionResource(eventSubscriptionId);

            await eventSubscriptionResource.DeleteAsync(WaitUntil.Completed, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<AzureOpenAIAccount> GetOpenAIAccountProperties(string openAIAccountResourceId)
        {
            var openAIAccount = _armClient.GetCognitiveServicesAccountResource(
                ResourceIdentifier.Parse(openAIAccountResourceId));

            var openAIAccountResource = await openAIAccount.GetAsync();
            var result = new AzureOpenAIAccount
            {
                Name = openAIAccountResource.Value.Data.Name,
                Endpoint = openAIAccountResource.Value.Data.Properties.Endpoint
            };

            var deployments = openAIAccount.GetCognitiveServicesAccountDeployments();

            foreach (var deployment in deployments)
            {
                var deploymentResource = await deployment.GetAsync();
                var deploymentData = deploymentResource.Value.Data;
                var tokenThrottlingRule = deploymentData.Properties.RateLimits.SingleOrDefault(rl => rl.Key == "token");
                var requestThrottlingRule = deploymentData.Properties.RateLimits.SingleOrDefault(rl => rl.Key == "request");

                result.Deployments.Add(new AzureOpenAIAccountDeployment
                {
                    AccountEndpoint = result.Endpoint,
                    Name = deploymentData.Name,
                    ModelName = deploymentData.Properties.Model.Name,
                    ModelVersion = deploymentData.Properties.Model.Version,
                    RequestRateLimit = (int) (requestThrottlingRule?.Count ?? 0),
                    RequestRateRenewalPeriod = (int) (requestThrottlingRule?.RenewalPeriod ?? 0),
                    TokenRateLimit = (int) (tokenThrottlingRule?.Count ?? 0),
                    TokenRateRenewalPeriod = (int) (tokenThrottlingRule?.RenewalPeriod ?? 0),
                    Capabilities = deploymentData.Properties.Capabilities.ToImmutableDictionary()
                });
            }

            return result;
        }
    }
}
