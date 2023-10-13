using FoundationaLLM.Gatekeeper.Core.Models.ContentSafety;

namespace FoundationaLLM.Gatekeeper.Core.Interfaces;

public interface IContentSafetyService
{
    Task<AnalyzeTextFilterResult> AnalyzeText(string content);
}
