using FoundationaLLM.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Common.Authentication
{
    /// <summary>
    /// Implements the <see cref="IAPIKeyValidationService"/> interface.
    /// </summary>
    public class APIKeyValidationService : IAPIKeyValidationService
    {
        private readonly IConfigurationService _configurationService;
        private readonly APIKeyValidationSettings _settings;

        /// <summary>
        /// Creates a new instance of the <see cref="APIKeyValidationService"/> class.
        /// </summary>
        /// <param name="configurationService">Service for retrieving application secrets, settings or other variables from the configuration store.</param>
        /// <param name="options">otions for the deployed API key validation service.</param>
        public APIKeyValidationService(
            IConfigurationService configurationService,
            IOptions<APIKeyValidationSettings> options)
        {
            _configurationService = configurationService;
            _settings = options.Value;
        }

        /// <summary>
        /// Checks if an API key is valid or not.
        /// </summary>
        /// <param name="apiKey">The API key to be checked.</param>
        /// <returns>A boolean value representing the validity of the API key.</returns>
        public bool IsValid(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return false;

            string? validApiKey = _configurationService.GetValue<string>(_settings.APIKeySecretName);

            if (validApiKey == null || validApiKey != apiKey)
                return false;

            return true;
        }
    }
}
