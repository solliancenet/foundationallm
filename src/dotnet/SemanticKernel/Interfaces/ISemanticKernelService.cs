using FoundationaLLM.Common.Models.Chat;

namespace FoundationaLLM.SemanticKernel.Core.Interfaces;

public interface ISemanticKernelService
{
    Task<string> GetCompletion(string userPrompt, List<MessageHistoryItem> messageHistory);

    Task<string> GetSummary(string content);

    Task AddMemory(object item, string itemName, Action<object, float[]> vectorizer);

    Task RemoveMemory(object item);
}
