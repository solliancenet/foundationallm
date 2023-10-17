using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Configuration.Authentication;
using FoundationaLLM.Common.Models.Configuration.Branding;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Chat.Helpers
{
    public class BrandManager : IBrandManager
    {
        private readonly EntraSettings _entraSettings;
        private readonly IAuthenticatedHttpClientFactory _authenticatedHttpClientFactory;

        public BrandManager(IAuthenticatedHttpClientFactory authenticatedHttpClientFactory,
            IOptions<EntraSettings> entraSettings)
        {
            _authenticatedHttpClientFactory = authenticatedHttpClientFactory;
            _entraSettings = entraSettings.Value;
        }

        /// <inheritdoc/>
        public async Task<ClientBrandingConfiguration> GetBrandAsync()
        {
            var client = await _authenticatedHttpClientFactory.CreateClientAsync(HttpClients.CoreAPI, _entraSettings.Scopes);

            return await client.GetFromJsonAsync<ClientBrandingConfiguration>($"/branding") ?? 
                   throw new InvalidOperationException("Could not retrieve the site branding configuration. Please try again.");
        }
    }
}
