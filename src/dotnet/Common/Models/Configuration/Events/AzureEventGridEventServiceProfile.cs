using FoundationaLLM.Common.Services.Events;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Configuration.Events
{
    /// <summary>
    /// The profile used to configure event handling in the <see cref="AzureEventGridEventService"/> event service.
    /// </summary>
    public class AzureEventGridEventServiceProfile
    {
        /// <summary>
        /// The time interval in seconds between successive event processing cycles.
        /// </summary>
        public int EventProcessingCycleSeconds { get; set; } = 60;

        /// <summary>
        /// The list of <see cref="EventGridTopicProfile"/> topic profiles used to configure event handling for an Azure Event Grid namespace topic.
        /// </summary>
        public List<EventGridTopicProfile> Topics { get; set; } = [];
    }

    /// <summary>
    /// The profile used to configure event handling for an Azure Event Grid namespace topic.
    /// </summary>
    public class EventGridTopicProfile
    {
        /// <summary>
        /// The name of the Azure Event Grid namespace topic.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// The prefix used for the topic subscription name.
        /// </summary>
        public required string SubscriptionPrefix { get; set; }

        /// <summary>
        /// The topic subscription name.
        /// </summary>
        [JsonIgnore]
        public string? SubscriptionName { get; set; }

        /// <summary>
        /// Indicates whether the subscription is available for use or not.
        /// </summary>
        [JsonIgnore]
        public bool SubscriptionAvailable { get; set; }
    }
}
