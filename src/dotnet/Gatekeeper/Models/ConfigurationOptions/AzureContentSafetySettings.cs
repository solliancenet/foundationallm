namespace FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions;

public record AzureContentSafetySettings
{
    public string APIUrl { get; init; }
    public string APIKey { get; init; }
    public int HateSeverity { get; init; }
    public int ViolenceSeverity { get; init; }
    public int SelfHarmSeverity { get; init; }
    public int SexualSeverity { get; init; }
}
