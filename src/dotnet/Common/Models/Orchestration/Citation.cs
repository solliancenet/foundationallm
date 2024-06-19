using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Orchestration;

/// <summary>
/// Encapsulates data about the sources used in building a completion response.
/// </summary>
public class Citation
{
    /// <summary>
    /// The index identifier of the document containing the source information.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>
    /// The title of the document containing the source information.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// The location of the source information.
    /// </summary>
    [JsonPropertyName("filepath")]
    public string? Filepath { get; set; }
}
