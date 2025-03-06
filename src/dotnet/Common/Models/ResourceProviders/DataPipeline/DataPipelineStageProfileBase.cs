using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders.DataPipeline
{
    /// <summary>
    /// Provides the definition of a data pipeline stage processor.
    /// </summary>
    public class DataPipelineStageProfileBase : ResourceBase
    {
        /// <summary>
        /// The type of the vectorization profile.
        /// </summary>
        [JsonIgnore]
        public override string? Type { get; set; }
    }
}
