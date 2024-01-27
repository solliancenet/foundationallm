using Newtonsoft.Json;

namespace FoundationaLLM.Common.Models.Messages
{
    /// <summary>
    /// Response from a Prompt Hub request.
    /// </summary>
    public record PromptHubResponse
    {
        /// <summary>
        /// The prompt metadata object returned from a Prompt Hub request.
        /// </summary>
        [JsonProperty("prompt")]
        public PromptMetadata? Prompt { get; set; }
    }

    /// <summary>
    /// PromptMetaData record
    /// </summary>
    public record PromptMetadata
    {
        /// <summary>
        /// Name of the prompt.
        /// </summary>
        [JsonProperty("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Text of the prompt prefix to be assigned to an agent.
        /// </summary>
        [JsonProperty("prompt_prefix")]
        public string? PromptPrefix { get; set; }

        /// <summary>
        /// Text of the prompt suffix to be assigned to an agent.
        /// </summary>
        [JsonProperty("prompt_suffix")]
        public string? PromptSuffix { get; set; }
    }
}
