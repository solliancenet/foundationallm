using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Events;
using FoundationaLLM.Common.Models.Events;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace FoundationaLLM.Common.Services.Events
{
    /// <summary>
    /// Subscribes to <see cref="IEventService"/> event namespaces and handles events using a decoupled, queue-based pattern.
    /// </summary>
    /// <param name="settings">The <see cref="LocalEventServiceSettings"/> containing configuration settings for the service.</param>
    /// <param name="eventService">The <see cref="IEventService"/> event service that is publishing events through event namespaces.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class LocalEventService(
        LocalEventServiceSettings settings,
        IEventService eventService,
        ILogger logger)
    {
        private readonly LocalEventServiceSettings _settings = settings;
        private readonly IEventService _eventService = eventService;
        private readonly ILogger _logger = logger;

        private Task? _dequeueingTask;

        /// <summary>
        /// The queue containing <see cref="EventTypeEventArgs"/> events received via the subscriptions to the <see cref="IEventService"/>.
        /// </summary>
        private readonly ConcurrentQueue<EventTypeEventArgs> _eventsQueue = [];

        /// <summary>
        /// Subscribes this instance to a specified list of event types supported by the <see cref="IEventService"/>.
        /// </summary>
        /// <param name="eventTypes">The list with the event types to subscribe to.</param>
        public void SubscribeToEventTypes(List<string> eventTypes)
        {
            foreach (var eventType in eventTypes)
            {
                _eventService.SubscribeToEventTypeEvent(
                    eventType,
                    IngestEvents);
            }
        }

        /// <summary>
        /// Kicks off the extraction of events from the events queue.
        /// The events are initially received from the events service and are queued internally.
        /// In the background, the extraction thread will dequeue and submit them to final processing using the provided event handler.
        /// </summary>
        /// <param name="eventHandler">The event handler invoked to process each set of events.</param>
        public void StartLocalEventProcessing(Func<EventTypeEventArgs, Task> eventHandler) =>
            _dequeueingTask = Task.Run(() => DequeueEvents(eventHandler));

        private void IngestEvents(object sender, EventTypeEventArgs e) =>
            // Trying to minimize the impact of calling this handler,
            // so we're just queuing the event set - will be processed by a separate thread.
            _eventsQueue.Enqueue(e);

        private async Task DequeueEvents(Func<EventTypeEventArgs, Task> eventHandler)
        {
            _logger.LogInformation("The local event service has started processing events.");

            while (true)
            {
                while (_eventsQueue.TryDequeue(out EventTypeEventArgs? eventSet))
                {
                    if (eventSet != null)
                    {
                        // Play it safe, ensure that we're absorbing any exception resulting from handling the event.
                        try
                        {
                            await eventHandler(eventSet);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "An error occured while handling an event set originating from the {EventNamespace} event namespace.",
                                eventSet.EventType);
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(_settings.EventProcessingCycleSeconds));
            }
        }
    }
}
