using FoundationaLLM.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Common.Authentication
{
    public class APIKeyValidationService : IAPIKeyValidationService
    {
        private readonly IConfigurationService _configuration;
        private readonly APIKeyValidationSettings _settings;

        public APIKeyValidationService(
            IConfigurationService configuration, 
            IOptions<APIKeyValidationSettings> options)
        {
            _configuration = configuration;
            _settings = options.Value;
        }

        public bool IsValid(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return false;

            string? validApiKey = _configuration.GetValue<string>(_settings.APIKeySecretName);

            if (validApiKey == null || validApiKey != apiKey)
                return false;

            return true;
        }
    }
}
