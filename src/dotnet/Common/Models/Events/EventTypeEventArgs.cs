using Azure.Messaging;

namespace FoundationaLLM.Common.Models.Events
{
    /// <summary>
    /// Event arguments required for event type event delegates.
    /// </summary>
    public class EventTypeEventArgs : EventArgs
    {
        /// <summary>
        /// The event type.
        /// </summary>
        public required string EventType { get; set; }

        /// <summary>
        /// The list of subjects associated with the event.
        /// </summary>
        public required IList<CloudEvent> Events { get; set; }
    }

    /// <summary>
    /// Multicast delegate used by the Azure Event Grid event service to provide support 
    /// for subscribing to event types.
    /// </summary>
    /// <param name="sender">The object raising the event.</param>
    /// <param name="e">The <see cref="EventTypeEventArgs"/> that contains the details about the events being raised.</param>
    public delegate void EventTypeEventDelegate(object sender, EventTypeEventArgs e);
}
