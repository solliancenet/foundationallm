using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Services
{
    /// <inheritdoc/>
    public class HttpClientFactoryService : IHttpClientFactoryService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfigurationService _configurationService;
        private readonly APIKeyValidationSettings _settings;

        /// <summary>
        /// Creates a new instance of the <see cref="HttpClientFactoryService"/> class.
        /// </summary>
        /// <param name="httpClientFactory">A fully configured <see cref="IHttpClientFactory"/>
        /// that allows access to <see cref="HttpClient"/> instances by name.</param>
        /// <param name="configurationService">An <see cref="IConfigurationService"/> instance
        /// that standardizes accessing configuration values.</param>
        /// <param name="options">An <see cref="APIKeyValidationSettings"/> class that
        /// contains the configured path to the desired API key.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public HttpClientFactoryService(IHttpClientFactory httpClientFactory, 
            IConfigurationService configurationService,
            IOptions<APIKeyValidationSettings> options)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _settings = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc/>
        public HttpClient CreateClient(string clientName, UnifiedUserIdentity? userIdentity = null)
        {
            var httpClient = _httpClientFactory.CreateClient(clientName);

            // Add the API key header.
            var apiKey = _configurationService.GetValue<string>(_settings.APIKeyPath);
            httpClient.DefaultRequestHeaders.Add(Constants.HttpHeaders.APIKey, apiKey);

            // Optionally add the user identity header.
            if (userIdentity != null)
            {
                var serializedIdentity = JsonConvert.SerializeObject(userIdentity);
                httpClient.DefaultRequestHeaders.Add(Constants.HttpHeaders.UserIdentity, serializedIdentity);
            }

            return httpClient;
        }
    }
}
