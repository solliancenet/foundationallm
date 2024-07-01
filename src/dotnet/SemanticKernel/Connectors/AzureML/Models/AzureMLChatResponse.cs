using System.Text.Json.Serialization;

namespace FoundationaLLM.SemanticKernel.Core.Connectors.AzureML.Models
{
    /// <summary>
    /// Encapsulates the information returned from an AzureML chat model.
    /// </summary>
    public class AzureMLChatResponse
    {
        /// <summary>
        /// The output from the AzureML model.
        /// </summary>
        [JsonPropertyName("output")]
        public string Output { get; set; } = String.Empty;
    }
}
