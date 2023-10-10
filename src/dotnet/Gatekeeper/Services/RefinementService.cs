using FoundationaLLM.Gatekeeper.Core.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Gatekeeper.Core.Services;

public class RefinementService : IRefinementService
{
    private readonly RefinementServiceSettings _settings;
    private readonly ILogger _logger;

    public RefinementService(
        IOptions<RefinementServiceSettings> options,
        ILogger<RefinementService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<string> RefineUserPrompt(string userPrompt)
    {
        throw new NotImplementedException();
    }
}
