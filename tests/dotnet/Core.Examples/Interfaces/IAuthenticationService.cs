namespace FoundationaLLM.Core.Examples.Interfaces;

/// <summary>
/// Responsible for providing authentication-related services.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Retrieves an authentication token for the specified API.
    /// </summary>>
    Task<string> GetAuthToken(string apiType);
}