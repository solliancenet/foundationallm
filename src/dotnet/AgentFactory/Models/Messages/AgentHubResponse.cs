using Newtonsoft.Json;

namespace FoundationaLLM.AgentFactory.Core.Models.Messages
{
    /// <summary>
    /// The response returned from the Agent Hub.
    /// </summary>
    public record AgentHubResponse
    {
        /// <summary>
        /// Information about a requested agent from the Agent Hub.
        /// </summary>
        //[JsonObject]
        [JsonProperty("agent")]
        public AgentMetadata? Agent { get; set; }

    }

    /// <summary>
    /// The information about an agent returned from the Agent Hub.
    /// </summary>
    public record AgentMetadata
    {
        /// <summary>
        /// The name of the agent.
        /// </summary>
        [JsonProperty("name")]
        public string? Name { get; set; }


        /// <summary>
        /// The description of the agent.
        /// </summary>
        [JsonProperty("description")]
        public string? Description { get; set; }

        /// <summary>
        /// The orchestration to execute.
        /// </summary>
        [JsonProperty("orchestrator")]
        public string? Orchestrator { get; set; }

        /// <summary>
        /// Datasources that are used or available to the agent.
        /// </summary>
        [JsonProperty("allowed_data_source_names")]
        public List<string>? AllowedDataSourceNames { get; set; }

        /// <summary>
        /// The lanauge model used by the agent.
        /// </summary>
        [JsonProperty("language_model")]
        public LanguageModelMetadata? LanguageModel { get; set; }
    }

    /// <summary>
    /// The language model used by the Agent.
    /// </summary>
    public record LanguageModelMetadata
    {
        /// <summary>
        /// The type of the language model
        /// </summary>
        [JsonProperty("model_type")]
        public string? ModelType { get; set; }

        /// <summary>
        /// The provider of the language model
        /// </summary>
        [JsonProperty("provider")]
        public string? Provider { get; set; }

        /// <summary>
        /// The temperature to use for the model request.
        /// </summary>
        [JsonProperty("temperature")]
        public float? Temperature { get; set; }

        /// <summary>
        /// Use the chat history in the request.
        /// </summary>
        [JsonProperty("use_chat")]
        public bool? UseChat { get; set; }
    }

    /// <summary>
    /// The supported language models.
    /// </summary>
    public enum LanguageModelType
    {
        /// <summary>
        /// MICROSOFT
        /// </summary>
        MICROSOFT,
        /// <summary>
        /// OPENAI
        /// </summary>
        OPENAI
    }

    /// <summary>
    /// The supported language model providers
    /// </summary>
    public enum LanguageModelProvider
    {
        /// <summary>
        /// OPENAI
        /// </summary>
        OPENAI
    }
}