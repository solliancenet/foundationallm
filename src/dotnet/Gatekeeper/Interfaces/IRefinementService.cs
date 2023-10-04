namespace FoundationaLLM.Gatekeeper.Core.Interfaces;

public interface IRefinementService
{
    Task RefineUserPrompt(string userPrompt);
}
