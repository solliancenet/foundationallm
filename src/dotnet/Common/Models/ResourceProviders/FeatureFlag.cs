using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// Represents a feature flag.
    /// </summary>
    public class FeatureFlag
    {
        /// <summary>
        /// Indicates whether the feature flag is enabled.
        /// </summary>
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }
    }
}
