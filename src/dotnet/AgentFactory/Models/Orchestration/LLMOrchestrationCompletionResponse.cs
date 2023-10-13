using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Orchestration
{
    internal class LLMOrchestrationCompletionResponse
    {
        /// <summary>
        /// The completion response from the orchestration engine.
        /// </summary>
        [JsonProperty("completion")]
        public string Completion { get; set; }

        /// <summary>
        /// The prompt received from the user.
        /// </summary>
        [JsonProperty("user_prompt")]
        public string UserPrompt { get; set; }

        /// <summary>
        /// The number of tokens in the prompt.
        /// </summary>
        [JsonProperty("prompt_tokens")]
        public int PromptTokens { get; set; } = 0;

        /// <summary>
        /// The number of tokens in the completion.
        /// </summary>
        [JsonProperty("completion_tokens")]
        public int CompletionTokens { get; set; } = 0;

        /// <summary>
        /// Total tokens used.
        /// </summary>
        [JsonProperty("total_tokens")]
        public int TotalTokens { get; set; } = 0;

        /// <summary>
        /// Total cost of the completion execution.
        /// </summary>
        [JsonProperty("total_cost")]
        public float TotalCost { get; set; } = 0f;
    }
}
