namespace FoundationaLLM.GatekeeperAPI.Core.Interfaces;

public interface IRefinementService
{
    Task RefineUserPrompt(string userPrompt);
}
