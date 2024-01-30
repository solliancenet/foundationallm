using FoundationaLLM.Common.Models.ResourceProvider;
using Newtonsoft.Json;

namespace FoundationaLLM.Prompt.Models.Metadata
{
    /// <summary>
    /// Prompt metadata model.
    /// </summary>
    public class Prompt : ResourceBase
    {
        /// <summary>
        /// The prompt prefix.
        /// </summary>
        [JsonProperty("prefix")]
        public string? Prefix { get; set; }
        /// <summary>
        /// The prompt suffix.
        /// </summary>
        [JsonProperty("suffix")]
        public string? Suffix { get; set; }
    }

}
