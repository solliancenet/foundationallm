using System.Text.Json.Serialization;

namespace FoundationaLLM.SemanticKernel.Core.Connectors.AzureML.Models
{
    /// <summary>
    /// The input data for the AzureML model request.
    /// </summary>
    public class AzureMLInputData
    {
        /// <summary>
        /// The message history.
        /// </summary>
        [JsonPropertyName("input_string")]
        public List<AzureMLChatMessage> InputString { get; set; } = [];

        /// <summary>
        /// Parameters for the LLM. Example: temperature, max_new_tokens, top_p, etc.
        /// </summary>
        [JsonPropertyName("parameters")]
        public Dictionary<string, object> Parameters { get; set; } = [];

    }
}
