using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Orchestration
{
    /// <summary>
    /// Base LLM orchestration request
    /// </summary>
    public class LLMOrchestrationRequest
    {
        /// <summary>
        /// Prompt entered by the user.
        /// </summary>
        [JsonProperty("user_prompt")]
        public string? UserPrompt { get; set; }
    }
}
