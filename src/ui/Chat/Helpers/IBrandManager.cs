using FoundationaLLM.Common.Models.Configuration.Branding;

namespace FoundationaLLM.Chat.Helpers;

public interface IBrandManager
{
    /// <summary>
    /// Retrieves the branding configuration for the UI.
    /// </summary>
    /// <returns>A populated <see cref="ClientBrandingConfiguration"/> object from the CoreAPI.</returns>
    Task<ClientBrandingConfiguration> GetBrandAsync();
}