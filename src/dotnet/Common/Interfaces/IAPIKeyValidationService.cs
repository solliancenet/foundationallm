namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Common interface for X-API-Key header validation.
    /// </summary>
    public interface IAPIKeyValidationService
    {
        /// <summary>
        /// Checks if an API key is valid or not.
        /// </summary>
        /// <param name="apiKey">The API key to be checked.</param>
        /// <returns>A boolean representing the validity of the API key parameter.</returns>
        bool IsValid(string apiKey);
    }
}
