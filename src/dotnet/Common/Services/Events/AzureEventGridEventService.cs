using Azure;
using Azure.Messaging;
using Azure.Messaging.EventGrid.Namespaces;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.Events;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Events;
using FoundationaLLM.Common.Models.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Common.Services.Events
{
    /// <summary>
    /// Provides services to integrate with the Azure Event Grid eventing platform.
    /// </summary>
    public class AzureEventGridEventService : IEventService
    {
        private readonly ILogger<AzureEventGridEventService> _logger;
        private readonly AzureEventGridEventServiceSettings _settings;
        private readonly AzureEventGridEventServiceProfile _profile;
        private readonly IAzureResourceManagerService _azureResourceManager;
        private readonly IHttpClientFactoryService _httpClientFactory;

        private readonly TimeSpan _eventProcessingCycle;
        private readonly string _serviceInstanceName;
        private bool _active = false;

        private readonly Dictionary<string, EventGridSenderClient> _senderClients = [];
        private readonly Dictionary<string, EventGridReceiverClient> _receiverClients = [];

        private readonly Dictionary<string, EventTypeEventDelegate?> _eventTypeDelegates =
            EventTypes.All.ToDictionary(ns => ns, ns => (EventTypeEventDelegate?)null);

        /// <inheritdoc/>
        public string ServiceInstanceName => _serviceInstanceName;

        /// <inheritdoc/>
        public bool Active => _active;

        /// <summary>
        /// Creates a new instance of the <see cref="AzureEventGridEventService"/> event service.
        /// </summary>
        /// <param name="settingsOptions">The options providing the settings for the service.</param>
        /// <param name="profileOptions">The options providing the profile for the service.</param>
        /// <param name="azureResourceManager">The <see cref="IAzureResourceManagerService"/> service providing access to Azure ARM services.</param>
        /// <param name="httpClientFactory">The <see cref="IHttpClientFactoryService"/> service used to create HTTP clients.</param>
        /// <param name="logger">The logger used for logging.</param>
        public AzureEventGridEventService(
            IOptions<AzureEventGridEventServiceSettings> settingsOptions,
            IOptions<AzureEventGridEventServiceProfile> profileOptions,
            IAzureResourceManagerService azureResourceManager,
            IHttpClientFactoryService httpClientFactory,
            ILogger<AzureEventGridEventService> logger)
        {
            _serviceInstanceName = $"https://foundationallm.ai/events/serviceinstances/{Environment.GetEnvironmentVariable(EnvironmentVariables.AKS_Pod_Name)
                ?? Environment.GetEnvironmentVariable(EnvironmentVariables.ACA_Container_App_Replica_Name)
                ?? Guid.NewGuid().ToString().ToLower()}";

            _settings = settingsOptions.Value;
            _profile = profileOptions.Value;
            _azureResourceManager = azureResourceManager;
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            _eventProcessingCycle = TimeSpan.FromSeconds(_profile.EventProcessingCycleSeconds);
        }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Create the topic subscriptions according to the service profile.
                if (!await CreateTopicSubscriptions(cancellationToken))
                {
                    _logger.LogError("The Azure Event Grid event service was not able to create the necessary topic subscriptions and/or sender/receiver clients and will not listen for any events.");

                    // Attempt to delete the topic subscriptions that were successfully created.
                    // No need to keep them around since we're not listening for events anyway.
                    await DeleteTopicSubscriptions(cancellationToken);

                    return;
                }

                _logger.LogInformation("The Azure Event Grid event service was successfully initialized.");

                _active = true;
            }
            catch (Exception ex)
            {
                throw new EventException("The Azure Event Grid event service encountered an error while starting and will not listen for any events.", ex);
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken) =>
            await DeleteTopicSubscriptions(cancellationToken);

        /// <inheritdoc/>
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (!_active)
            {
                _logger.LogCritical("The Azure Event Grid events service is not properly initialized and will not listen for any events.");
                return;
            }

            if (_profile.Topics.Count == 0)
            {
                _logger.LogWarning("The Azure Event Grid events service is not configured to listen to any events.");
                return;
            }

            _logger.LogInformation("The Azure Event Grid event service is starting to process messages.");

            while (true)
            {
                if (cancellationToken.IsCancellationRequested) return;

                foreach (var topic in _profile.Topics)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    if (!_receiverClients.TryGetValue(topic.Name, out var receiverClient))
                    {
                        _logger.LogError("The Azure Event Grid event service does not have a receiver client for topic {TopicName}.",
                            topic.Name);
                        continue;
                    }

                    try
                    {
                        var receiveResult = await receiverClient.ReceiveAsync(
                            maxEvents: 10,
                            maxWaitTime: TimeSpan.FromSeconds(10),
                            cancellationToken: cancellationToken);

                        if (receiveResult?.Value?.Details != null
                            && receiveResult.Value.Details.Count > 0)
                        await ProcessTopicSubscriptionEvents(
                            receiverClient,
                            receiveResult.Value.Details);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occured while trying to retrieve events for topic {TopicName} and subscription {SubscriptionName}.",
                            topic.Name, topic.SubscriptionName);
                    }
                }

                await Task.Delay(_eventProcessingCycle, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public void SubscribeToEventTypeEvent(string eventType, EventTypeEventDelegate eventHandler)
        {
            ArgumentNullException.ThrowIfNull(eventHandler);

            if (!_eventTypeDelegates.TryGetValue(eventType, out var eventDelegate))
                throw new EventException($"The namespace {eventType} is invalid.");

            if (eventDelegate == null)
                _eventTypeDelegates[eventType] = eventHandler!;
            else
                eventDelegate += eventHandler;
        }

        /// <inheritdoc/>
        public void UnsubscribeFromEventTypeEvent(string eventType, EventTypeEventDelegate eventHandler)
        {
            if (!_eventTypeDelegates.TryGetValue(eventType, out var eventDelegate))
                throw new EventException($"The namespace {eventType} is invalid.");

            eventDelegate -= eventHandler;
        }

        /// <inheritdoc/>
        public async Task SendEvent(string topicName, CloudEvent cloudEvent)
        {
            if (!_active)
            {
                _logger.LogError("Could not send event {EventId} of type {EventType} from source {EventSource}. The Azure Event Grid event service is not active and cannot send events.",
                    cloudEvent.Id, cloudEvent.Type, cloudEvent.Source);
                return;
            }

            // Enforce the correct event source when sending through the Azure Event Grid service.
            cloudEvent.Source = _serviceInstanceName;

            if (!_senderClients.TryGetValue(topicName, out var senderClient))
            {
                _logger.LogError("Could not send event {EventId} of type {EventType} from source {EventSource}. The Azure Event Grid event service does not have a sender client for the topic {EventTopic}.",
                    cloudEvent.Id, cloudEvent.Type, cloudEvent.Source, topicName);
                return;
            }

            try
            {
                await senderClient.SendAsync(cloudEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured while trying to send event {EventId} of type {EventType} from source {EventSource}.",
                    cloudEvent.Id, cloudEvent.Type, cloudEvent.Source);
            }
        }

        private async Task<bool> CreateTopicSubscriptions(CancellationToken cancellationToken)
        {
            foreach (var topic in _profile.Topics)
            {
                if (cancellationToken.IsCancellationRequested)
                    return false;

                topic.SubscriptionName = $"{topic.SubscriptionPrefix}-{Guid.NewGuid().ToString("N").ToLower()}";
                _logger.LogInformation("The Azure Event Grid event service will create a subscription named {SubscriptionName} in topic {TopicName}.",
                    topic.SubscriptionName, topic.Name);

                try
                {
                    topic.SubscriptionAvailable = await _azureResourceManager.CreateEventGridNamespaceTopicSubscription(
                        _settings.NamespaceId,
                        topic.Name,
                        topic.SubscriptionName,
                        EventTypes.All,
                        cancellationToken);

                    if (topic.SubscriptionAvailable)
                        _logger.LogInformation("The Azure Event Grid event service successfully created a subscription named {SubscriptionName} in topic {TopicName}.",
                            topic.SubscriptionName, topic.Name);

                    _senderClients.Add(topic.Name, await GetSenderClient(topic.Name)
                        ?? throw new EventException($"Failed to create the Azure Event Grid sender client for topic {topic.Name}."));
                    _receiverClients.Add(topic.Name, await GetReceiverClient(topic.Name, topic.SubscriptionName)
                        ?? throw new EventException($"Failed to create the Azure Event Grid receiver client for topic {topic.Name} and subscription {topic.SubscriptionName}."));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occured while creating a subscription named {SubscriptionName} in topic {TopicName}.",
                        topic.SubscriptionName, topic.Name);

                    return false;
                }
            }

            return true;
        }

        private async Task DeleteTopicSubscriptions(CancellationToken cancellationToken)
        {
            foreach (var topic in _profile.Topics)
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                if (!topic.SubscriptionAvailable)
                {
                    _logger.LogInformation("The Azure Event Grid event service did not create the subscription named {SubscriptionName} from topic {TopicName}. Delete will not be attempted.",
                        topic.SubscriptionName, topic.Name);
                    continue;
                }

                _logger.LogInformation("The Azure Event Grid event service will delete the subscription named {SubscriptionName} from topic {TopicName}.",
                    topic.SubscriptionName, topic.Name);

                try
                {
                    await _azureResourceManager.DeleteEventGridNamespaceTopicSubscription(
                        _settings.NamespaceId,
                        topic.Name,
                        topic.SubscriptionName!,
                        cancellationToken);

                    topic.SubscriptionAvailable = false;
                    _logger.LogInformation("The Azure Event Grid event service successfully deleted the subscription named {SubscriptionName} from topic {TopicName}.",
                        topic.SubscriptionName, topic.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occured while deleting the subscription named {SubscriptionName} from topic {TopicName}.",
                        topic.SubscriptionName, topic.Name);
                }
            }
        }

        private async Task ProcessTopicSubscriptionEvents(
            EventGridReceiverClient receiverClient,
            IReadOnlyList<ReceiveDetails> eventDetails)
        {
            Dictionary<string, List<CloudEvent>> eventsToProcess = EventTypes.All
                .ToDictionary(ns => ns, ns => new List<CloudEvent>());

            foreach (var receiveDetails in eventDetails)
            {
                if (String.CompareOrdinal(receiveDetails.Event.Source, _serviceInstanceName) == 0)
                    // Ignore events that were sent by this service instance.
                    continue;

                eventsToProcess[receiveDetails.Event.Type].Add(receiveDetails.Event);
            }

            foreach (var eventTypeEventsToProcess in eventsToProcess)
            {
                if (eventTypeEventsToProcess.Value.Count > 0)
                {
                    if (!_eventTypeDelegates.TryGetValue(eventTypeEventsToProcess.Key, out var eventTypeDelegate))
                    {
                        _logger.LogWarning("The Azure Event Grid event service does not have any event handlers for the event type {EventType}.",
                            eventTypeEventsToProcess.Key);
                        continue;
                    }

                    if (eventTypeDelegate != null)
                    {
                        try
                        {
                            // We have new events and we have at least one event handler attached to the delegate associated with this namespace.
                            // Fire up the event and call all registered delegates.
                            eventTypeDelegate(this, new EventTypeEventArgs
                            {
                                EventType = eventTypeEventsToProcess.Key,
                                Events = eventTypeEventsToProcess.Value
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error invoking registered delegates for event types {EventType}.",
                                eventTypeEventsToProcess.Key);
                        }
                    }
                }
            }

            // Messages have been handled if needed.
            // Acknowledge the messages to remove them from the Azure Event Grid topic subscription.
            await AcknowledgeMessages(receiverClient, eventDetails);
        }

        private async Task AcknowledgeMessages(
            EventGridReceiverClient receiverClient,
            IReadOnlyList<ReceiveDetails> eventDetails)
        {
            var acknowledgeResult = await receiverClient.AcknowledgeAsync(
                eventDetails.Select(ed => ed.BrokerProperties.LockToken));

            foreach (var ackFailure in acknowledgeResult.Value.FailedLockTokens)
            {
                _logger.LogError("Failed to acknowledge Event Grid message. Lock token: {LockToken}. Error code: {ErrorCode}. Error description: {ErrorDescription}.",
                    ackFailure.LockToken, ackFailure.Error, ackFailure.ToString());
            }
        }

        #region Create Event Grid client

        private async Task<EventGridSenderClient?> GetSenderClient(string topicName) =>
            await _httpClientFactory.CreateClient<EventGridSenderClient?>(
                HttpClientNames.AzureEventGrid,
                DefaultAuthentication.ServiceIdentity!,
                BuildSenderClient,
                new Dictionary<string, object>()
                {
                    { HttpClientFactoryServiceKeyNames.AzureEventGridTopicName, topicName }
                });

        private EventGridSenderClient? BuildSenderClient(Dictionary<string, object> parameters)
        {
            EventGridSenderClient? client = null;
            try
            {
                var topicName = parameters[HttpClientFactoryServiceKeyNames.AzureEventGridTopicName].ToString();
                var endpoint = parameters[HttpClientFactoryServiceKeyNames.Endpoint].ToString();
                var authenticationType = (AuthenticationTypes)parameters[HttpClientFactoryServiceKeyNames.AuthenticationType];
                switch (authenticationType)
                {
                    case AuthenticationTypes.AzureIdentity:
                        client = new EventGridSenderClient(new Uri(endpoint!), topicName, DefaultAuthentication.AzureCredential);
                        break;
                    case AuthenticationTypes.APIKey:
                        var apiKey = parameters[HttpClientFactoryServiceKeyNames.APIKey].ToString();
                        client = new EventGridSenderClient(new Uri(endpoint!), topicName, new AzureKeyCredential(apiKey!));
                        break;
                    default:
                        throw new ConfigurationValueException($"The {authenticationType} authentication type is not supported by the Azure Event Grid events service.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error creating the Azure Event Grid client.");
            }

            return client;
        }

        private async Task<EventGridReceiverClient?> GetReceiverClient(string topicName, string subscriptionName) =>
            await _httpClientFactory.CreateClient<EventGridReceiverClient?>(
                HttpClientNames.AzureEventGrid,
                DefaultAuthentication.ServiceIdentity!,
                BuildReceiverClient,
                new Dictionary<string, object>()
                {
                    { HttpClientFactoryServiceKeyNames.AzureEventGridTopicName, topicName },
                    { HttpClientFactoryServiceKeyNames.AzureEventGridTopicSubscriptionName, subscriptionName }
                });

        private EventGridReceiverClient? BuildReceiverClient(Dictionary<string, object> parameters)
        {
            EventGridReceiverClient? client = null;
            try
            {
                var topicName = parameters[HttpClientFactoryServiceKeyNames.AzureEventGridTopicName].ToString();
                var subscriptionName = parameters[HttpClientFactoryServiceKeyNames.AzureEventGridTopicSubscriptionName].ToString();
                var endpoint = parameters[HttpClientFactoryServiceKeyNames.Endpoint].ToString();
                var authenticationType = (AuthenticationTypes)parameters[HttpClientFactoryServiceKeyNames.AuthenticationType];
                switch (authenticationType)
                {
                    case AuthenticationTypes.AzureIdentity:
                        client = new EventGridReceiverClient(new Uri(endpoint!), topicName, subscriptionName, DefaultAuthentication.AzureCredential);
                        break;
                    case AuthenticationTypes.APIKey:
                        var apiKey = parameters[HttpClientFactoryServiceKeyNames.APIKey].ToString();
                        client = new EventGridReceiverClient(new Uri(endpoint!), topicName, subscriptionName, new AzureKeyCredential(apiKey!));
                        break;
                    default:
                        throw new ConfigurationValueException($"The {authenticationType} authentication type is not supported by the Azure Event Grid events service.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "There was an error creating the Azure Event Grid client.");
            }

            return client;
        }

        #endregion
    }
}
