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
        private readonly ICallContext _callContext;
        private readonly IDownstreamAPISettings _apiSettings;

        /// <summary>
        /// Creates a new instance of the <see cref="HttpClientFactoryService"/> class.
        /// </summary>
        /// <param name="httpClientFactory">A fully configured <see cref="IHttpClientFactory"/>
        /// that allows access to <see cref="HttpClient"/> instances by name.</param>
        /// <param name="callContext">Stores a <see cref="UnifiedUserIdentity"/> object resolved from
        /// one or more services, and the agent hint value extracted from the request header,
        /// if any. If the agent hint value is not empty or null, the service adds its contents
        /// to the agent hint header for the returned <see cref="HttpClient"/> instance.</param>
        /// <param name="apiSettings">A <see cref="DownstreamAPISettings"/> class that
        /// contains the configured path to the desired API key.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public HttpClientFactoryService(IHttpClientFactory httpClientFactory,
            ICallContext callContext,
            IDownstreamAPISettings apiSettings)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _callContext = callContext ?? throw new ArgumentNullException(nameof(callContext));
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
                httpClient.DefaultRequestHeaders.Add(Constants.HttpHeaders.APIKey, settings.APIKey);
            }

            // Optionally add the user identity header.
            if (_callContext.CurrentUserIdentity != null)
            {
                var serializedIdentity = JsonConvert.SerializeObject(_callContext.CurrentUserIdentity);
                httpClient.DefaultRequestHeaders.Add(Constants.HttpHeaders.UserIdentity, serializedIdentity);

                // Add the bearer token header if present.
                if (_callContext.Token != null)
                {
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _callContext.Token);
                }
            }

            // Add the agent hint header if present.
            if (_callContext.AgentHint != null)
            {
                var serializedAgentHint = JsonConvert.SerializeObject(_callContext.AgentHint);
                httpClient.DefaultRequestHeaders.Add(Constants.HttpHeaders.AgentHint, serializedAgentHint);
            }

            return httpClient;
        }
    }
}
