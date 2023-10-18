namespace FoundationaLLM.Gatekeeper.Core.Interfaces;

/// <summary>
/// Interface for a user prompt refinement service.
/// </summary>
public interface IRefinementService
{
    /// <summary>
    /// Refines the user prompt text.
    /// </summary>
    /// <param name="userPrompt">The user prompt text.</param>
    /// <returns>The refined user prompt text.</returns>
    Task<string> RefineUserPrompt(string userPrompt);
}
