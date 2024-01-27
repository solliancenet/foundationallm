using FoundationaLLM.SemanticKernel.Text;
using FoundationaLLM.SemanticKernel.Core.Interfaces;

namespace FoundationaLLM.SemanticKernel.Core.Services;

/// <summary>
/// Implements the <see cref="ISystemPromptService"/> interface.
/// </summary>
public class InMemorySystemPromptService : ISystemPromptService
{
    readonly Dictionary<string, string> _prompts = new()
    {
        { "RetailAssistant.Default", @"
You are an intelligent assistant for the Cosmic Works Bike Company. 
You are designed to provide helpful answers to user questions about 
product, product category, customer and sales order (salesOrder) information provided in JSON format below.

Instructions:
- Only answer questions related to the information provided below,
- Don't reference any product, customer, or salesOrder data not provided below.
- If you're unsure of an answer, you can say ""I don't know"" or ""I'm not sure"" and recommend users search themselves.

Text of relevant information:".NormalizeLineEndings()
        },
        {
            "RetailAssistant.Limited", @"
You are an AI assistant that helps people find information.
Provide concise answers that are polite and professional.".NormalizeLineEndings()
        },
        { "Summarizer.TwoWords", @"
Summarize this prompt in one or two words to use as a label in a button on a web page. Output words only.".NormalizeLineEndings()
        }
    };

    /// <summary>
    /// Gets the specified system prompt.
    /// </summary>
    /// <param name="promptName">The system prompt name.</param>
    /// <param name="forceRefresh">The flag that inform the System Prompt service to do a cache refresh.</param>
    /// <returns>The system prompt text.</returns>
    public async Task<string> GetPrompt(string promptName, bool forceRefresh = false)
    {
        if (!_prompts.ContainsKey(promptName))
            throw new ArgumentException($"The prompt {promptName} is not supported.");

        return await Task.FromResult(_prompts[promptName]);
    }
}
