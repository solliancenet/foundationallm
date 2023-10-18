using FoundationaLLM.SemanticKernel.Chat;

namespace FoundationaLLM.SemanticKernel.Core.Models.ConfigurationOptions;

/// <summary>
/// Provides configuration options for the Semantic Kernel service.
/// </summary>
public record SemanticKernelServiceSettings
{
    /// <summary>
    /// Provides configuration options for Open AI.
    /// </summary>
    public record OpenAISettings
    {
        /// <summary>
        /// The name of the completions deployment service.
        /// </summary>
        public required string CompletionsDeployment { get; set; }

        /// <summary>
        /// The maximum tokens for the completions deployment service.
        /// </summary>
        public required int CompletionsDeploymentMaxTokens { get; init; }

        /// <summary>
        /// The name of the embeddings deployment service.
        /// </summary>
        public required string EmbeddingsDeployment { get; init; }

        /// <summary>
        /// The maximum tokens for the embeddings deployment service.
        /// </summary>
        public required int EmbeddingsDeploymentMaxTokens { get; init; }

        /// <summary>
        /// The name of the chat completion prompt.
        /// </summary>
        public required string ChatCompletionPromptName { get; init; }

        /// <summary>
        /// The name of the short summary prompt.
        /// </summary>
        public required string ShortSummaryPromptName { get; init; }

        /// <summary>
        /// The configuration options for prompt optimization.
        /// </summary>
        public required PromptOptimizationSettings PromptOptimization { get; init; }

        /// <summary>
        /// The Azure Open AI endpoint.
        /// </summary>
        public required string Endpoint { get; init; }

        /// <summary>
        /// The Azure Open AI key.
        /// </summary>
        public required string Key { get; init; }
    }

    /// <summary>
    /// Provides configuration options for Cognitive Search.
    /// </summary>
    public record CognitiveSearchSettings
    {
        /// <summary>
        /// The index name.
        /// </summary>
        public required string IndexName { get; init; }

        /// <summary>
        /// The maximum vector search results.
        /// </summary>
        public required int MaxVectorSearchResults { get; init; }

        /// <summary>
        /// The Azure Cognitive Search endpoint.
        /// </summary>
        public required string Endpoint { get; init; }

        /// <summary>
        /// The Azure Cognitive Search key.
        /// </summary>
        public required string Key { get; init; }
    }

    /// <summary>
    /// Open AI settings object.
    /// </summary>
    public required OpenAISettings OpenAI { get; init; }

    /// <summary>
    /// Cognitive Search settings object.
    /// </summary>
    public required CognitiveSearchSettings CognitiveSearch { get; init; }
}