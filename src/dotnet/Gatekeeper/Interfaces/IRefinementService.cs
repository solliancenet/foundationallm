namespace FoundationaLLM.Gatekeeper.Core.Interfaces;

public interface IRefinementService
{
    Task<string> RefineUserPrompt(string userPrompt);
}
