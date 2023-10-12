namespace FoundationaLLM.AgentFactory.Models.ConfigurationOptions
{
    public record AgentHubSettings
    {
        public string? APIUrl { get; init; }
        public string? APIKey { get; init; }
    }
}
