using FoundationaLLM.Common.Models.Configuration.Branding;
using Microsoft.Identity.Web;

namespace FoundationaLLM.Chat.Helpers
{
    public class BrandingService : IBrandingService
    {
        /// <inheritdoc/>
        public ClientBrandingConfiguration? CurrentConfig { get; private set; }
        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;
        private readonly IBrandManager _brandManager;

        public BrandingService(IBrandManager brandManager,
            MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
        {
            _brandManager = brandManager;
            _consentHandler = consentHandler;
        }

        /// <inheritdoc/>
        public async Task InitializeAsync()
        {
            try
            {
                CurrentConfig = await _brandManager.GetBrandAsync();
            }
            catch (Exception e)
            {
                _consentHandler.HandleException(e);
                throw;
            }
        }
    }

}
