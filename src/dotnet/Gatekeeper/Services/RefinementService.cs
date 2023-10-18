using FoundationaLLM.Gatekeeper.Core.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Gatekeeper.Core.Services;

/// <summary>
/// Implements the <see cref="IRefinementService"/> interface.
/// </summary>
public class RefinementService : IRefinementService
{
    private readonly RefinementServiceSettings _settings;
    private readonly ILogger _logger;

    /// <summary>
    /// Constructor for the Refinement service.
    /// </summary>
    /// <param name="options">The configuration options for the Refinement service.</param>
    /// <param name="logger"></param>
    public RefinementService(
        IOptions<RefinementServiceSettings> options,
        ILogger<RefinementService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Refines the user prompt text.
    /// </summary>
    /// <param name="userPrompt">The user prompt text.</param>
    /// <returns>The refined user prompt text.</returns>
    public Task<string> RefineUserPrompt(string userPrompt)
    {
        throw new NotImplementedException();
    }
}
