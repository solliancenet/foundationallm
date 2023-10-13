namespace FoundationaLLM.AgentFactory.Models.ConfigurationOptions
{
    public record PromptHubSettings
    {
        public string? APIUrl { get; init; }
        public string? APIKey { get; init; }
    }
}
