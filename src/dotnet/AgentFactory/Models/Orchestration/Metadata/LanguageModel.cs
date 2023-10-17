using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata
{
    /// <summary>
    /// Language model metadata model.
    /// </summary>
    public class LanguageModel
    {
        /// <summary>
        /// Type property.
        /// </summary>
        [JsonProperty("type")]
        public string? Type { get; set; }

        /// <summary>
        /// Provider of the language model.
        /// </summary>
        [JsonProperty("provider")]
        public string? Provider { get; set; } = LanguageModelProviders.MICROSOFT;

        /// <summary>
        /// Temperature value to assign on the language model.
        /// This indicates the "degree of creativity" the model can use when generating completions.
        /// </summary>
        [JsonProperty("temperature")]
        [RegularExpression("^(?:0?(?:\\.\\d)?|1(\\.0?)?)$", ErrorMessage = "The temperature values must be between 0 and 1."), ]
        public float Temperature { get; set; } = 0f;

        /// <summary>
        /// Flag indicating whether or not to use the chat model.
        /// </summary>
        [JsonProperty("use_chat")]
        public bool UseChat { get; set; } = true;
    }
}
