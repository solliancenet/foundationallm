using Microsoft.Graph.Models.Security;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration.Direct
{
    /// <summary>
    /// Input for a direct Azure OpenAI request.
    /// </summary>
    public class AzureOpenAICompletionRequest : AzureOpenAICompletionParameters
    {
        /// <summary>
        /// The prompt for which to generate completions.
        /// </summary>
        [JsonPropertyName("prompt")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Prompt { get; set; }

        /// <summary>
        /// Object defining the required input role and content key value pairs.
        /// </summary>
        [JsonPropertyName("messages")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CompletionMessage[]? Messages { get; set; }

        /// <summary>
        /// Any data sources that should be used
        /// </summary>
        [JsonPropertyName("dataSources")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<DataSource>? DataSources { get; set; }
    }
}
