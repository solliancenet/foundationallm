using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.Orchestration.Direct
{
    /// <summary>
    /// The completion response choice.
    /// </summary>
    public class AzureOpenAICompletionResponseChoice
    {
        /// <summary>
        /// The completion response text.
        /// </summary>
        [JsonPropertyName("text")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Text { get; set; }

        /// <summary>
        /// The completion response index.
        /// </summary>
        [JsonPropertyName("index")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Index { get; set; }

        /// <summary>
        /// The log probabilities on the logprobs most likely tokens, as well the chosen tokens.
        /// </summary>
        [JsonPropertyName("logprobs")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? LogProbs { get; set; }

        /// <summary>
        /// The finish reason for the completion response.
        /// </summary>
        [JsonPropertyName("finish_reason")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? FinishReason { get; set; }

        /// <summary>
        /// Contains the completion response message(s) if any.
        /// </summary>
        [JsonPropertyName("message")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CompletionMessage? Message { get; set; }

        /// <summary>
        /// Contains the completion response message(s) if any (extendedChat)
        /// </summary>
        [JsonPropertyName("messages")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<CompletionMessage>? Messages { get; set; }


    }
}
