namespace FoundationaLLM.Common.Models.ResourceProviders.Vectorization
{
    /// <summary>
    /// The category for api endpoint class.
    /// </summary>
    public enum APIEndpointCategory
    {
        /// <summary>
        /// Endpoints related to orchestration.
        /// </summary>
        Orchestration,

        /// <summary>
        /// Endpoints related to LLM.
        /// </summary>
        LLM,

        /// <summary>
        ///  Endpoints related to Gatekeeper.
        /// </summary>
        Gatekeeper,

        /// <summary>
        /// Endpoints for direct interactions with Microsoft Azure AI services.
        /// </summary>
        AzureAIDirect,

        /// <summary>
        /// Endpoints for direct interactions with Microsoft Azure open AI services.
        /// </summary>
        AzureOpenAIDirect,

        /// <summary>
        /// General endpoints.
        /// </summary>
        General
    }
}
