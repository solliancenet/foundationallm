using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Authentication;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FoundationaLLM.Common.Services.API
{
    /// <inheritdoc/>
    public class HttpClientFactoryService : IHttpClientFactoryService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(10);

        private IResourceProviderService? _configurationResourceProvider;

        /// <summary>
        /// Creates a new instance of the <see cref="HttpClientFactoryService"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> for the main DI container.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> used to retrieve app settings from configuration.</param>
        /// <param name="httpClientFactory">A fully configured <see cref="IHttpClientFactory"/>.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public HttpClientFactoryService(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        /// <inheritdoc/>
        public async Task<HttpClient> CreateClient(string clientName, UnifiedUserIdentity userIdentity, bool getStatusEndpoint = false)
        {
            var endpointConfiguration = await GetEndpoint(clientName, userIdentity);

            var client = await CreateClient(endpointConfiguration, userIdentity);

            if (getStatusEndpoint && !string.IsNullOrWhiteSpace(endpointConfiguration.StatusUrl))
            {
                client.BaseAddress = new Uri(endpointConfiguration.StatusUrl);
            }

            return client;
        }

        /// <inheritdoc/>
        public async Task<T> CreateClient<T>(
            string clientName,
            UnifiedUserIdentity userIdentity,
            Func<Dictionary<string, object>, T> clientBuilder,
            Dictionary<string, object>? clientBuilderParameters = null)
        {
            var endpointConfiguration = await GetEndpoint(clientName, userIdentity);

            if (clientBuilderParameters == null)
                clientBuilderParameters = [];

            clientBuilderParameters[HttpClientFactoryServiceKeyNames.Endpoint] =
                GetUrlExceptionForUserIdentity(endpointConfiguration, userIdentity)?.Url                
                ?? endpointConfiguration.Url;
            clientBuilderParameters[HttpClientFactoryServiceKeyNames.TimeoutSeconds] = endpointConfiguration.TimeoutSeconds;
            clientBuilderParameters[HttpClientFactoryServiceKeyNames.AuthenticationType] = endpointConfiguration.AuthenticationType;
            if (endpointConfiguration.AuthenticationType == AuthenticationTypes.APIKey)
            {
                if (!endpointConfiguration.AuthenticationParameters.TryGetValue(
                        AuthenticationParametersKeys.APIKeyConfigurationName, out var apiKeyConfigurationNameObj))
                    throw new Exception($"The {AuthenticationParametersKeys.APIKeyConfigurationName} key is missing from the endpoint's authentication parameters dictionary.");

                var apiKey = _configuration.GetValue<string>(apiKeyConfigurationNameObj.ToString()!);
                clientBuilderParameters[HttpClientFactoryServiceKeyNames.APIKey] = apiKey!;
            }

            return clientBuilder(clientBuilderParameters);
        }


        /// <inheritdoc/>
        public async Task<HttpClient> CreateClient(APIEndpointConfiguration endpointConfiguration, UnifiedUserIdentity? userIdentity)
        {
            var httpClient = _httpClientFactory.CreateClient(endpointConfiguration.Name);

            // Set the default timeout.
            httpClient.Timeout = TimeSpan.FromSeconds(endpointConfiguration.TimeoutSeconds);

            // Set the default URL.
            httpClient.BaseAddress = new Uri(endpointConfiguration.Url);

            // Override the default URL if an exception is provided.
            if (userIdentity != null)
            {
                var urlException = GetUrlExceptionForUserIdentity(endpointConfiguration, userIdentity);
                if (urlException != null)
                    httpClient.BaseAddress = new Uri(urlException.Url);

                // Add the user identity header.
                var serializedIdentity = JsonSerializer.Serialize(userIdentity);
                httpClient.DefaultRequestHeaders.Add(Constants.HttpHeaders.UserIdentity, serializedIdentity);
            }

            switch (endpointConfiguration.AuthenticationType)
            {
                case AuthenticationTypes.APIKey:
                    var apiKey = string.Empty;
                    var apiKeyHeaderName = string.Empty;
                    var apiKeyPrefix = string.Empty;

                    if (!endpointConfiguration.AuthenticationParameters.TryGetValue(
                        AuthenticationParametersKeys.APIKeyConfigurationName, out var apiKeyConfigurationNameObj))
                        throw new Exception($"The {AuthenticationParametersKeys.APIKeyConfigurationName} key is missing from the endpoint's authentication parameters dictionary.");

                    apiKey = _configuration.GetValue<string>(apiKeyConfigurationNameObj?.ToString()!)!;

                    if (!endpointConfiguration.AuthenticationParameters.TryGetValue(
                        AuthenticationParametersKeys.APIKeyHeaderName, out var apiKeyHeaderNameObj))
                        throw new Exception($"The {AuthenticationParametersKeys.APIKeyHeaderName} key is missing from the enpoint's authentication parameters dictionary.");

                    apiKeyHeaderName = apiKeyHeaderNameObj.ToString();

                    // APIKeyPrefix is optional, set to empty string if not found.
                    endpointConfiguration.AuthenticationParameters.TryGetValue(
                        AuthenticationParametersKeys.APIKeyPrefix, out var apiKeyPrefixObj);

                    apiKeyPrefix = apiKeyPrefixObj?.ToString() ?? string.Empty;


                    if (apiKeyHeaderName == HeaderNames.Authorization)
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                            apiKeyPrefix ?? string.Empty, apiKey
                        );
                    }
                    else
                    {
                        httpClient.DefaultRequestHeaders.Add(
                            apiKeyHeaderName,
                            string.IsNullOrWhiteSpace(apiKeyPrefix)
                                ? apiKey
                                : $"{apiKeyPrefix} {apiKey}");
                    }

                    break;

                case AuthenticationTypes.AzureIdentity:
                    if (!endpointConfiguration.AuthenticationParameters.TryGetValue(
                        AuthenticationParametersKeys.Scope, out var scopeObj))
                        throw new Exception($"The {AuthenticationParametersKeys.Scope} key is missing from the enpoint's authentication parameters dictionary.");

                    var scope = scopeObj as string;

                    if (string.IsNullOrEmpty(scope))
                        throw new Exception($"The {AuthenticationParametersKeys.Scope} key is missing from the enpoint's authentication parameters dictionary.");

                    var credentials = ServiceContext.AzureCredential;
                    var tokenResult = await credentials!.GetTokenAsync(
                        new([scope]),
                        default);

                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", tokenResult.Token);

                    break;

                default:
                    throw new Exception($"The authentication type {endpointConfiguration.AuthenticationType} is not supported.");
            }

            return httpClient;
        }

        /// <inheritdoc/>
        public HttpClient CreateUnregisteredClient(TimeSpan? timeout = null)
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = timeout ?? _defaultTimeout;
            return httpClient;
        }

        private async Task EnsureConfigurationResourceProvider()
        {
            if (_configurationResourceProvider != null)
                return;

            var resourceProviderServices = _serviceProvider.GetServices<IResourceProviderService>();
            _configurationResourceProvider = resourceProviderServices
                .SingleOrDefault(rps => rps.Name == ResourceProviderNames.FoundationaLLM_Configuration)
                ?? throw new ResourceProviderException($"The resource provider {ResourceProviderNames.FoundationaLLM_Configuration} was not loaded.");


            await _configurationResourceProvider.WaitForInitialization();
        }

        private async Task<APIEndpointConfiguration> GetEndpoint(string name, UnifiedUserIdentity userIdentity)
        {
            await EnsureConfigurationResourceProvider();

            var endpointConfiguration = await _configurationResourceProvider!.GetResourceAsync<APIEndpointConfiguration>(
                $"/{ConfigurationResourceTypeNames.APIEndpointConfigurations}/{name}",
                userIdentity)
                ?? throw new Exception($"The resource provider {ResourceProviderNames.FoundationaLLM_Configuration} did not load the {name} endpoint configuration.");

            return endpointConfiguration;
        }

        /// <summary>
        /// Retrieve an enabled URL Exception for the provided user identity if one exists. If not NULL is returned.
        /// </summary>
        /// <param name="endpointConfiguration">The endpoint configuration from which to retrieve the URL Exception.</param>
        /// <param name="userIdentity">The Identity of the user.</param>
        /// <returns>URLException or NULL</returns>
        private UrlException? GetUrlExceptionForUserIdentity(APIEndpointConfiguration endpointConfiguration, UnifiedUserIdentity userIdentity)
        => endpointConfiguration.UrlExceptions.SingleOrDefault(x => x.UserPrincipalName.ToLower() == userIdentity.UPN!.ToLower() && x.Enabled);
        
    }
}
