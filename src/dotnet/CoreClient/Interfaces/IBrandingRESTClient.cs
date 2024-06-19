using FoundationaLLM.Common.Models.Configuration.Branding;

namespace FoundationaLLM.Client.Core.Interfaces
{
    /// <summary>
    /// Provides methods to manage calls to the Core API's Branding endpoints.
    /// </summary>
    public interface IBrandingRESTClient
    {
        /// <summary>
        /// Retrieves the branding information for the client.
        /// </summary>
        /// <returns></returns>
        Task<ClientBrandingConfiguration> GetBrandingAsync();
    }
}
