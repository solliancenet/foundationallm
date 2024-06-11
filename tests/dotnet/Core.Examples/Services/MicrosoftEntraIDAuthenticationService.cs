using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Core.Examples.Interfaces;
using FoundationaLLM.Core.Examples.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace FoundationaLLM.Core.Examples.Services
{
    /// <inheritdoc/>
    public class MicrosoftEntraIDAuthenticationService(
        IOptionsSnapshot<HttpClientOptions> httpClientOptions) : IAuthenticationService
    {
        /// <inheritdoc/>
        public async Task<string> GetAuthToken(string apiType)
        {
            var options = httpClientOptions.Get(apiType);

            var scope = options.Scope;
            if (scope == null) return string.Empty;
            // The scope needs to just be the base URI, not the full URI.
            scope = scope[..scope.LastIndexOf('/')];

            var credentials = DefaultAuthentication.AzureCredential;
            if (credentials == null) return string.Empty;
            var tokenResult = await credentials.GetTokenAsync(
                new([scope]),
                default);

            return tokenResult.Token;
        }
    }
}
