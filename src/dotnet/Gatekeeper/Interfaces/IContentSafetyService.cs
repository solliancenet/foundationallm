using FoundationaLLM.Gatekeeper.Core.Models.ContentSafety;

namespace FoundationaLLM.Gatekeeper.Core.Interfaces;

/// <summary>
/// Interface for calling a content safety service.
/// </summary>
public interface IContentSafetyService
{
    /// <summary>
    /// Checks if a text is safe or not based on pre-configured content filters.
    /// </summary>
    /// <param name="content">The text content that needs to be analyzed.</param>
    /// <returns>The text analysis restult, which includes a boolean flag that represents if the content is considered safe. 
    /// In case the content is unsafe, also returns the reason.</returns>
    Task<AnalyzeTextFilterResult> AnalyzeText(string content);
}
