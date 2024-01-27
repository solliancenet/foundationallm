using Newtonsoft.Json;

namespace FoundationaLLM.Common.Models.Orchestration
{
    /// <summary>
    /// Completion request object that excludes session-based properties.
    /// </summary>
    public class DirectCompletionRequest
    {
        /// <summary>
        /// Represent the input or user prompt.
        /// </summary>
        [JsonProperty("user_prompt")]
        public string? UserPrompt { get; set; }
    }
}
