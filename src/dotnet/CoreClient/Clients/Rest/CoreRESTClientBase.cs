using Azure.Core;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Settings;
using System.Text.Json;
using FoundationaLLM.Common.Constants.Authentication;

namespace FoundationaLLM.Client.Core.Clients.Rest
{
    internal class CoreRESTClientBase(
        IHttpClientFactory httpClientFactory,
        TokenCredential credential)
    {
        /// <summary>
        /// Sets standard JSON serializer options.
        /// </summary>
        protected JsonSerializerOptions SerializerOptions { get; } =
            CommonJsonSerializerOptions.GetJsonSerializerOptions();

        /// <summary>
        /// Returns a new HttpClient configured with an authentication header that uses the supplied token.
        /// </summary>
        /// <returns></returns>
        protected async Task<HttpClient> GetCoreClientAsync()
        {
            var coreClient = httpClientFactory.CreateClient(HttpClients.CoreAPI);
            
            var token = await credential.GetTokenAsync(new TokenRequestContext([ScopeURIs.FoundationaLLM_Core]), default);
            coreClient.SetBearerToken(token.Token);
            return coreClient;
        }
    }
}
