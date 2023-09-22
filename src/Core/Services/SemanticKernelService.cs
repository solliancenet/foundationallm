using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Models.ConfigurationOptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Core.Services;

public class SemanticKernelService : ISemanticKernelService
{
    private readonly SemanticKernelServiceSettings _settings;
    private readonly ILogger _logger;

    public SemanticKernelService(
        IOptions<SemanticKernelServiceSettings> options,
        ILogger<SemanticKernelService> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task Test()
    {
        await Task.Run(() =>
        {
            return;
        });
    }
}

