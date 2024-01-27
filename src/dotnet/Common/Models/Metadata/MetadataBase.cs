using Newtonsoft.Json;

namespace FoundationaLLM.Common.Models.Metadata
{
    /// <summary>
    /// Metadata model base class.
    /// </summary>
    public class MetadataBase
    {
        /// <summary>
        /// Name property.
        /// </summary>
        [JsonProperty("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Type property.
        /// </summary>
        [JsonProperty("type")]
        public string? Type { get; set; }

        /// <summary>
        /// Description property.
        /// </summary>
        [JsonProperty("description")]
        public string? Description { get; set; }
    }
}
