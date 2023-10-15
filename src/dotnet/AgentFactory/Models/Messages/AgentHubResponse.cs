using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Messages
{
    public record AgentHubResponse
    {
        //[JsonObject]
        [JsonProperty("agent")]
        public AgentMetadata Agent { get; set; }

    }

    public record AgentMetadata
    {
        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("allowed_data_source_names")]
        public List<string>? AllowedDataSourceNames { get; set; }

        [JsonProperty("language_model")]
        public LanguageModelMetadata? LanguageModel { get; set; }
    }

    public record LanguageModelMetadata
    {
        [JsonProperty("model_type")]
        public string? ModelType { get; set; }

        [JsonProperty("provider")]
        public string? Provider { get; set; }

        [JsonProperty("temperature")]
        public float? Temperature { get; set; }

        [JsonProperty("use_chat")]
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