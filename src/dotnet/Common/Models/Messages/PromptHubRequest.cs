using Newtonsoft.Json;

namespace FoundationaLLM.Common.Models.Messages
{
    /// <summary>
    /// The format of a Prompt Hub request.
    /// </summary>
    public record PromptHubRequest
    {
        /// <summary>
        /// The session ID.
        /// </summary>
        [JsonProperty("session_id")]
        public string? SessionId { get; set; }

        /// <summary>
        /// The prompt container from which prompt values will be retrieved.
        /// </summary>
        [JsonProperty("prompt_container")]
        public string? PromptContainer { get; set; }

        /// <summary>
        /// Name of the prompt for which the prompt values should be retrieved from the Prompt Hub.
        /// </summary>
        [JsonProperty("prompt_name")]
        public string? PromptName { get; set; } = "default";
    }
}
