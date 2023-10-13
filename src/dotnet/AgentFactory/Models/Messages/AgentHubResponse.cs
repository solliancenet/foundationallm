using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FoundationaLLM.AgentFactory.Core.Models.Messages
{
    public record AgentHubResponse
    {
        [JsonPropertyName("agents")]
        public AgentMetadata[]? Agents { get; set; }

    }

    public record AgentMetadata
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("allowed_data_source_names")]
        public string[]? AllowedDataSourceNames { get; set; }

        [JsonPropertyName("language_model")]
        public LanguageModelMetadata? LanguageModel { get; set; }
    }

    public record LanguageModelMetadata
    {
        [JsonPropertyName("model_type")]
        public LanguageModelType? ModelType { get; set; }

        [JsonPropertyName("provider")]
        public LanguageModelProvider? Provider { get; set; }

        [JsonPropertyName("temperature")]
        public float? Temperature { get; set; }

        [JsonPropertyName("use_chat")]
        public bool? UseChat { get; set; }
    }

    public enum LanguageModelType
    {
        MICROSOFT,
        OPENAI
    }

    public enum LanguageModelProvider
    {
        OPENAI
    }
}