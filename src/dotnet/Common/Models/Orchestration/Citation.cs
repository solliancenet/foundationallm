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
    /// The content of the source information.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// The location of the source information.
    /// </summary>
    [JsonPropertyName("filepath")]
    public string? Filepath { get; set; }

    /// <summary>
    /// The url of the source information.
    /// </summary>
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    /// <summary>
    /// Any extra metadata associated with the source information.
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; }

    /// <summary>
    /// The chunk identifier of the source information.
    /// </summary>
    [JsonPropertyName("chunk_id")]
    public string? ChunkId { get; set; }

}
