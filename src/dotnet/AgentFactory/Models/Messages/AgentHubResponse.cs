using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;
using Newtonsoft.Json;
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
        //[JsonObject]
        [JsonPropertyName("agents")]
        public List<AgentMetadata>? Agents { get; set; }

    }

    public record AgentMetadata
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("allowed_data_source_names")]
        public List<string>? AllowedDataSourceNames { get; set; }

        [JsonPropertyName("language_model")]
        public LanguageModelMetadata? LanguageModel { get; set; }
    }

    public record LanguageModelMetadata
    {
        [JsonPropertyName("model_type")]
        public string? ModelType { get; set; }

        [JsonPropertyName("provider")]
        public string? Provider { get; set; }

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