using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.SemanticKernel.Core.Interfaces;

public interface ISemanticKernelService
{
    Task<CompletionResponseBase> Complete(string userPrompt, List<MessageHistoryItem> messageHistory);

    Task<string> Summarize(string content);

    Task AddMemory(object item, string itemName, Action<object, float[]> vectorizer);

    Task RemoveMemory(object item);
}
