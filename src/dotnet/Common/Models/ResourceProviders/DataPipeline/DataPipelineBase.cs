using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.DataPipeline
{
    /// <summary>
    /// Provides the model for a data pipeline.
    /// </summary>
    public class DataPipelineBase : ResourceBase
    {
        /// <summary>
        /// Gets or sets a value that indicates whether the pipeline is active or not.
        /// </summary>
        /// <remarks>
        /// When the pipeline is inactive, it cannot be triggered to execute.
        /// </remarks>
        [JsonPropertyName("active")]
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets the type of trigger that initiates the execution of the pipeline.
        /// </summary>
        [JsonPropertyName("trigger_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required DataPipelineTriggerType TriggerType { get; set; }

        /// <summary>
        /// Gets or sets the schedule of the trigger in Cron format.
        /// </summary>
        /// <remarks>
        /// This propoerty is valid only when TriggerType = Schedule.
        /// </remarks>
        [JsonPropertyName("trigger_cron_schedule")]
        public string? TriggerCronSchedule { get; set; }

        /// <summary>
        /// Gets or sets the object identifier of the data source used to retrieve content in the initial stages of the data pipeline.
        /// </summary>
        [JsonPropertyName("data_source_object_id")]
        public required string DataSourceObjectId { get; set; }

        /// <summary>
        /// Gets or sets the list of starting stages in the data pipeline.
        /// </summary>
        [JsonPropertyName("starting_stages")]
        public List<DataPipelineStage> StartingStages { get; set; } = [];
    }
}
