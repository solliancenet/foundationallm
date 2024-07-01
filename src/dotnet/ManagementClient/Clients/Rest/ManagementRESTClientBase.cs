using Azure.Core;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Settings;
using System.Text.Json;
using FoundationaLLM.Common.Constants.Authentication;

namespace FoundationaLLM.Client.Management.Clients.Rest
{
    internal class ManagementRESTClientBase(
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
        protected async Task<HttpClient> GetManagementClientAsync()
        {
            var managementClient = httpClientFactory.CreateClient(HttpClients.ManagementAPI);
            
            var token = await credential.GetTokenAsync(new TokenRequestContext([ScopeURIs.FoundationaLLM_Management]), default);
            managementClient.SetBearerToken(token.Token);
            return managementClient;
        }
    }
}
