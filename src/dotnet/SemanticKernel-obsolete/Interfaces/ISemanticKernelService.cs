using FoundationaLLM.Common.Models.Chat;

namespace FoundationaLLM.SemanticKernel.Core.Interfaces;

/// <summary>
/// Interface for the Semantic Kernel service.
/// </summary>
public interface ISemanticKernelService
{
    /// <summary>
    /// Gets a completion from the Semantic Kernel service.
    /// </summary>
    /// <param name="userPrompt">The user prompt text.</param>
    /// <param name="messageHistory">A list of previous messages.</param>
    /// <returns>The completion text.</returns>
    Task<string> GetCompletion(string userPrompt, List<MessageHistoryItem> messageHistory);

    /// <summary>
    /// Gets a summary from the Semantic Kernel service.
    /// </summary>
    /// <param name="content">The user prompt text.</param>
    /// <returns>The prompt summary.</returns>
    Task<string> GetSummary(string content);

    /// <summary>
    /// Add an object instance and its associated vectorization to the underlying memory.
    /// </summary>
    /// <param name="item">The object instance to be added to the memory.</param>
    /// <param name="itemName">The name of the object instance.</param>
    /// <returns></returns>
    Task AddMemory(object item, string itemName);

    /// <summary>
    /// Removes an object instance and its associated vectorization from the underlying memory.
    /// </summary>
    /// <param name="item">The object instance to be removed from the memory.</param>
    /// <returns></returns>
    Task RemoveMemory(object item);
}
