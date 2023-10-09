using FoundationaLLM.Common.Interfaces;
using Microsoft.Identity.Abstractions;

namespace FoundationaLLM.Chat.Helpers
{
    /// <summary>
    /// A helper class for producing authenticated HttpClient instances utilizing Entra authentication.
    /// </summary>
    public class EntraAuthenticatedHttpClientFactory : IAuthenticatedHttpClientFactory
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAuthorizationHeaderProvider _authorizationHeaderProvider;

        public EntraAuthenticatedHttpClientFactory(
            IHttpClientFactory httpClientFactory,
            IAuthorizationHeaderProvider authorizationHeaderProvider)
        {
            _httpClientFactory = httpClientFactory;
            _authorizationHeaderProvider = authorizationHeaderProvider;
        }

        /// <summary>
        /// Creates a new <see cref="HttpClient"/> instance from <see cref="IHttpClientFactory"/> with an
        /// authorization header for the current user.
        /// </summary>
        /// <param name="clientName">The named <see cref="HttpClient"/> client configuration.</param>
        /// <param name="scopes">List of permissions to request from the service.</param>
        /// <returns></returns>
        public async Task<HttpClient> CreateClientAsync(string clientName, string scopes)
        {
            var client = _httpClientFactory.CreateClient(clientName);
            string accessToken = await _authorizationHeaderProvider.CreateAuthorizationHeaderForUserAsync(new [] { scopes });
            client.DefaultRequestHeaders.Add("Authorization", accessToken);
            return client;
        }
    }
}
