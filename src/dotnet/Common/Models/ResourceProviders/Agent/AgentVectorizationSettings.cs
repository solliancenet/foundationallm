using FoundationaLLM.Common.Models.Vectorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.ResourceProviders.Agent
{
    /// <summary>
    /// Vectorization settings related to a knowledge management agent.
    /// </summary>
    public class AgentVectorizationSettings
    {
        /// <summary>
        /// Determines if the agent uses a dedicated pipeline.
        /// </summary>
        [JsonPropertyName("dedicated_pipeline")]
        public bool DedicatedPipeline { get; set; }
        /// <summary>
        /// The data source resource path.
        /// </summary>
        [JsonPropertyName("data_source_object_id")]
        public string? DataSourceObjectId { get; set; }

        /// <summary>
        /// The vectorization indexing profile resource paths.
        /// </summary>
        [JsonPropertyName("indexing_profile_object_ids")]
        public List<string>? IndexingProfileObjectIds { get; set; }

        /// <summary>
        /// The vectorization text embedding profile resource path.
        /// </summary>
        [JsonPropertyName("text_embedding_profile_object_id")]
        public string? TextEmbeddingProfileObjectId { get; set; }

        /// <summary>
        /// The vectorization text partitioning profile resource path.
        /// </summary>
        [JsonPropertyName("text_partitioning_profile_object_id")]
        public string? TextPartitioningProfileObjectId { get; set; }

        /// <summary>
        /// The vectorization data pipeline object path.
        /// </summary>
        [JsonPropertyName("vectorization_data_pipeline_object_id")]
        public string? VectorizationDataPipelineObjectId { get; set; }

        /// <summary>
        /// The type of trigger that initiates the execution of the pipeline.
        /// </summary>
        [JsonPropertyName("trigger_type")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public VectorizationPipelineTriggerType? TriggerType { get; set; }

        /// <summary>
        /// The schedule of the trigger in Cron format.
        /// This property is valid only when TriggerType = Schedule.
        /// </summary>
        [JsonPropertyName("trigger_cron_schedule")]
        public string? TriggerCronSchedule { get; set; }
    }
}
