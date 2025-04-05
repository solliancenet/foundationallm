namespace FoundationaLLM.Context.Models.Configuration
{
    /// <summary>
    /// Provides settings for an Azure Container Apps Dynamic Sessions service.
    /// </summary>
    public class AzureContainerAppsServiceSettings
    {
        /// <summary>
        /// Get or sets the list of Azure Container Apps Dynamic Sessions endpoints grouped by programming language.
        /// </summary>
        public Dictionary<string, List<string>> Endpoints { get; set; } = [];
    }
}
