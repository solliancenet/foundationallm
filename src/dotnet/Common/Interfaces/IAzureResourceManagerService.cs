using FoundationaLLM.Common.Models.Azure;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Provides services to interact with the Azure Resource Manager (ARM) infrastructure.
    /// </summary>
    public interface IAzureResourceManagerService
    {
        /// <summary>
        /// Creates a new Azure Event Grid namespace topic subscription.
        /// </summary>
        /// <param name="namespaceResourceId">The Azure resource identifier of the Azure Event Grid namespace.</param>
        /// <param name="topicName">The name of the topic for which the subscription should be created.</param>
        /// <param name="topicSubscriptionName">The name of the subscription to be created.</param>
        /// <param name="includedEventTypes">The list of event types to include in the subscription as part of the filters configuration.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> signaling the request to cancel the operation.</param>
        /// <returns>True if the subscription was successfully created, false otherwise.</returns>
        Task<bool> CreateEventGridNamespaceTopicSubscription(
            string namespaceResourceId,
            string topicName,
            string topicSubscriptionName,
            List<string> includedEventTypes,
            CancellationToken cancellationToken);

        /// <summary>
        /// Deletes an Azure Event Grid namespace topic subscription.
        /// </summary>
        /// <param name="namespaceResourceId">The Azure resource identifier of the Azure Event Grid namespace.</param>
        /// <param name="topicName">The name of the topic for which the subscription should be deleted.</param>
        /// <param name="topicSubscriptionName">The name of the subscription to be deleted.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> signaling the request to cancel the operation.</param>
        /// <returns></returns>
        Task DeleteEventGridNamespaceTopicSubscription(
            string namespaceResourceId,
            string topicName,
            string topicSubscriptionName,
            CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the properties of an Azure OpenAI account.
        /// </summary>
        /// <param name="openAIAccountResourceId">The Azure resource identifier of the Azure OpenAI account.</param>
        /// <returns>An <see cref="AzureOpenAIAccount"/> object with the properties of the account, including model deployments.</returns>
        Task<AzureOpenAIAccount> GetOpenAIAccountProperties(string openAIAccountResourceId);
    }
}
