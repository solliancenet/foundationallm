using FoundationaLLM.GatekeeperAPI.Core.Interfaces;
using FoundationaLLM.GatekeeperAPI.Core.Models.ConfigurationOptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.GatekeeperAPI.Core.Services;

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

    public async Task RefineUserPrompt(string userPrompt)
    {
        throw new NotImplementedException();
    }
}
