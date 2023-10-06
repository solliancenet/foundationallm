namespace FoundationaLLM.Common.Models.Orchestration;

public class CompletionResponse
{
    public string Completion { get; set; }
    public string UserPrompt { get; set; }
    public int UserPromptTokens { get; set; }
    public int ResponseTokens { get; set; }
    public float[]? UserPromptEmbedding { get; set; }

    public CompletionResponse(string completion, string userPrompt, int userPromptTokens, int responseTokens,
        float[]? userPromptEmbedding)
    {
        Completion = completion;
        UserPrompt = userPrompt;
        UserPromptTokens = userPromptTokens;
        ResponseTokens = responseTokens;
        UserPromptEmbedding = userPromptEmbedding;
    }

    public CompletionResponse()
    {
    }
}
