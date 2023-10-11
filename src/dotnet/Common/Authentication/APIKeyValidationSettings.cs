namespace FoundationaLLM.Common.Authentication
{
    public record APIKeyValidationSettings
    {
        public string APIKeySecretName { get; init; }
    }
}
