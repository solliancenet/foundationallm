using Newtonsoft.Json;
using System.Reflection.Metadata.Ecma335;

namespace FoundationaLLM.Common.Models.Orchestration;

/// <summary>
/// Response from a language model.
/// </summary>
public class CompletionResponse
{
    /// <summary>
    /// The completion response from the language model.
    /// </summary>
    [JsonProperty("completion")]
    public string Completion { get; set; }

    /// <summary>
    /// The user prompt the language model responded to.
    /// </summary>
    [JsonProperty("user_prompt")]
    public string UserPrompt { get; set; }

    /// <summary>
    /// The number of tokens in the prompt.
    /// </summary>
    [JsonProperty("prompt_tokens")]
    public int PromptTokens { get; set; } = 0;

    /// <summary>
    /// The number of tokens in the completion.
    /// </summary>
    [JsonProperty("completion_tokens")]
    public int CompletionTokens { get; set; } = 0;

    /// <summary>
    /// The total number of tokens.
    /// </summary>
    [JsonProperty("total_tokens")]
    public int TotalTokens => PromptTokens + CompletionTokens;

    /// <summary>
    /// The total cost of executing the completion operation.
    /// </summary>
    [JsonProperty("total_cost")]
    public float TotalCost { get; set; } = 0.0f;

    /// <summary>
    /// User prompt embedding.
    /// </summary>
    [JsonProperty("user_prompt_embedding")]
    public float[]? UserPromptEmbedding { get; set; }

    /// <summary>
    /// Initialize a completion response
    /// </summary>
    /// <param name="completion">The completion response from the language model.</param>
    /// <param name="userPrompt">The user prompt the language model responded to.</param>
    /// <param name="userPromptTokens">The number of tokens in the prompt.</param>
    /// <param name="responseTokens">The number of tokens in the completion.</param>
    /// <param name="userPromptEmbedding">User prompt embedding.</param>
    public CompletionResponse(string completion, string userPrompt, int userPromptTokens, int responseTokens,
        float[]? userPromptEmbedding)
    {
        Completion = completion;
        UserPrompt = userPrompt;
        PromptTokens = userPromptTokens;
        CompletionTokens = responseTokens;
        UserPromptEmbedding = userPromptEmbedding;
    }

    /// <summary>
    /// Initialize a completion response.
    /// </summary>
    public CompletionResponse()
    {
    }
}
