namespace FoundationaLLM.Common.Models.Configuration.Authorization
{
    public record AuthorizationServiceClientSettings
    {
        public required string APIUrl { get; set; }

        public required string APIScope { get; set; }
    }
}
