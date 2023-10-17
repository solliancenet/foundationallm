using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Messages
{
    /// <summary>
    /// Response from a Prompt Hub request.
    /// </summary>
    public record PromptHubResponse
    {
        /// <summary>
        /// The list of prompts returned from a Prompt Hub request.
        /// </summary>
        [JsonProperty("prompts")]
        public PromptMetadata[]? Prompts { get; set; }
    }

    /// <summary>
    /// PromptMetaData record
    /// </summary>
    public record PromptMetadata
    {

        /// <summary>
        /// Name of the prompt
        /// </summary>
        [JsonProperty("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Content of the prompt
        /// </summary>
        [JsonProperty("prompt")]
        public string? Prompt { get; set; }

    }
}
