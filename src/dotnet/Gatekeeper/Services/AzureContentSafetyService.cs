using Azure;
using Azure.AI.ContentSafety;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions;
using FoundationaLLM.Gatekeeper.Core.Models.ContentSafety;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Gatekeeper.Core.Services
{
    public class AzureContentSafetyService : IContentSafetyService
    {
        private readonly ContentSafetyClient _client;
        private readonly AzureContentSafetySettings _settings;
        private readonly IConfigurationService _configurationService;
        private readonly ILogger _logger;

        public AzureContentSafetyService(
            IOptions<AzureContentSafetySettings> options,
            IConfigurationService configurationService,
            ILogger<AzureContentSafetyService> logger)
        {
            _settings = options.Value;
            _configurationService = configurationService;
            _logger = logger;

            _client = new ContentSafetyClient(new Uri(_settings.Endpoint), new AzureKeyCredential(_configurationService.GetValue<string>(_settings.APIKeySecretName)));
        }

        public async Task<AnalyzeTextFilterResult> AnalyzeText(string content)
        {
            var request = new AnalyzeTextOptions(content);

            Response<AnalyzeTextResult> response;
            try
            {
                response = await _client.AnalyzeTextAsync(request);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, $"Analyze prompt text failed with status code: {ex.Status}, error code: {ex.ErrorCode}, message: {ex.Message}");
                return new AnalyzeTextFilterResult { Safe = false, Reason = "The content safety service was unable to validate the prompt text due to an internal error." };
            }

            var safe = true;
            var reason = "The prompt text did not pass the content safety filter. Reason:";

            var hateSeverity = response.Value.HateResult?.Severity ?? 0;
            if (hateSeverity > _settings.HateSeverity)
            {
                reason += $" hate";
                safe = false;
            }

            var violenceSeverity = response.Value.ViolenceResult?.Severity ?? 0;
            if (violenceSeverity > _settings.ViolenceSeverity)
            {
                reason += $" violence";
                safe = false;
            }

            var selfHarmSeverity = response.Value.SelfHarmResult?.Severity ?? 0;
            if (selfHarmSeverity > _settings.SelfHarmSeverity)
            {
                reason += $" self-harm";
                safe = false;
            }

            var sexualSeverity = response.Value.SexualResult?.Severity ?? 0;
            if (sexualSeverity > _settings.SexualSeverity)
            {
                reason += $" sexual";
                safe = false;
            }

            return new AnalyzeTextFilterResult() { Safe = safe, Reason = safe ? string.Empty : reason };
        }
    }
}
