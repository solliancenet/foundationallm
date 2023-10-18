using FoundationaLLM.Common.Models.Configuration.Branding;

namespace FoundationaLLM.Chat.Helpers;

public interface IBrandingService
{
    /// <summary>
    /// The current configuration populated during initialization.
    /// </summary>
    ClientBrandingConfiguration? CurrentConfig { get; }
    /// <summary>
    /// Initializes the branding service by retrieving the brand settings through
    /// <see cref="IBrandManager"/> and setting the current configuration for this instance.
    /// </summary>
    Task InitializeAsync();
}