namespace FoundationaLLM.Common.Authentication
{
    /// <summary>
    /// Represents settings for API key validation.
    /// </summary>
    public record APIKeyValidationSettings
    {
        /// <summary>
        /// The name of the secret that contains the API key.
        /// </summary>
        public string APIKeySecretName { get; init; }
    }
}
