namespace FoundationaLLM.Common.Authentication
{
    /// <summary>
    /// Represents settings for API key validation.
    /// </summary>
    public record APIKeyValidationSettings
    {
        /// <summary>
        /// The API key.
        /// </summary>
        public string APIKey { get; init; }
    }
}
