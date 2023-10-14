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
        private readonly IUserIdentityContext _userIdentityContext;
        private readonly IDownstreamAPISettings _apiSettings;

        /// <summary>
        /// Creates a new instance of the <see cref="HttpClientFactoryService"/> class.
        /// </summary>
        /// <param name="httpClientFactory">A fully configured <see cref="IHttpClientFactory"/>
        /// that allows access to <see cref="HttpClient"/> instances by name.</param>
        /// <param name="configurationService">An <see cref="IConfigurationService"/> instance
        /// that standardizes accessing configuration values.</param>
        /// <param name="userIdentityContext">Stores a <see cref="UnifiedUserIdentity"/> object resolved from
        /// one or more services.</param>
        /// <param name="apiSettings">A <see cref="DownstreamAPISettings"/> class that
        /// contains the configured path to the desired API key.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public HttpClientFactoryService(IHttpClientFactory httpClientFactory, 
            IConfigurationService configurationService,
            IUserIdentityContext userIdentityContext,
            IDownstreamAPISettings apiSettings)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            _userIdentityContext = userIdentityContext ?? throw new ArgumentNullException(nameof(userIdentityContext));
            _apiSettings = apiSettings ?? throw new ArgumentNullException(nameof(apiSettings));
        }

        /// <inheritdoc/>
        public HttpClient CreateClient(string clientName)
        {
            var httpClient = _httpClientFactory.CreateClient(clientName);
            httpClient.Timeout = TimeSpan.FromSeconds(600);

            // Add the API key header.
            if (_apiSettings.DownstreamAPIs.TryGetValue(clientName, out var settings))
            {
                var apiKey = _configurationService.GetValue<string>(settings.APIKeySecretName);
                httpClient.DefaultRequestHeaders.Add(Constants.HttpHeaders.APIKey, apiKey);
            }

            // Optionally add the user identity header.
            if (_userIdentityContext.CurrentUserIdentity != null)
            {
                var serializedIdentity = JsonConvert.SerializeObject(_userIdentityContext.CurrentUserIdentity);
                httpClient.DefaultRequestHeaders.Add(Constants.HttpHeaders.UserIdentity, serializedIdentity);
            }

            return httpClient;
        }
    }
}
