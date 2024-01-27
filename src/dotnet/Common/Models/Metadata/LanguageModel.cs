using System.ComponentModel.DataAnnotations;
using FoundationaLLM.Common.Constants;
using Newtonsoft.Json;

namespace FoundationaLLM.Common.Models.Metadata
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

        /// <summary>
        /// The endpoint to use to access the language model.
        /// </summary>
        [JsonProperty("api_endpoint")]
        public string? ApiEndpoint { get; set; }

        /// <summary>
        /// The API key of the language model endpoint to use to access the language model.
        /// </summary>
        [JsonProperty("api_key")]
        public string? ApiKey { get; set; }

        /// <summary>
        /// API version of the language model endpoint.
        /// </summary>
        [JsonProperty("api_version")]
        public string? ApiVersion { get; set; }

        /// <summary>
        /// Version of the deployed model.
        /// </summary>
        [JsonProperty("version")]
        public string? Version { get; set; }

        /// <summary>
        /// Name of the deployment of the model.
        /// </summary>
        [JsonProperty("deployment")]
        public string? Deployment { get; set; }

    }
}
