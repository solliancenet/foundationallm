namespace FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions;

/// <summary>
/// Provides configuration options for the Azure Content Safety service.
/// </summary>
public record AzureContentSafetySettings
{
    /// <summary>
    /// The Azure Content Safety endpoint.
    /// </summary>
    public required string APIUrl { get; init; }

    /// <summary>
    /// The Azure Content Safety key.
    /// </summary>
    public required string APIKey { get; init; }

    /// <summary>
    /// The threshold for filtering the contents of a text, based on the "Hate" category. Possible values are: 0 = Safe, 2 = Low, 4 = Medium, 6 = High.
    /// </summary>
    public int HateSeverity { get; init; }

    /// <summary>
    /// The threshold for filtering the contents of a text, based on the "Violence" category. Possible values are: 0 = Safe, 2 = Low, 4 = Medium, 6 = High.
    /// </summary>
    public int ViolenceSeverity { get; init; }

    /// <summary>
    /// The threshold for filtering the contents of a text, based on the "Self-Harm" category. Possible values are: 0 = Safe, 2 = Low, 4 = Medium, 6 = High.
    /// </summary>
    public int SelfHarmSeverity { get; init; }

    /// <summary>
    /// The threshold for filtering the contents of a text, based on the "Sexual" category. Possible values are: 0 = Safe, 2 = Low, 4 = Medium, 6 = High.
    /// </summary>
    public int SexualSeverity { get; init; }
}
