using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.TextEmbedding;

/// <summary>
/// Represents the content associated with a vectorization request.
/// </summary>
public class ContentIdentifier
{
    /// <summary>
    /// The multipart unique identifier of the the content (i.e. document) being vectorized.
    /// </summary>
    [JsonPropertyOrder(1)]
    [JsonPropertyName("multipart_id")]
    public required List<string> MultipartId { get; set; }

    /// <summary>
    /// The unique identifier of the content (i.e., document) being vectorized.
    /// The identifier is determined by concatenating the parts from <see cref="MultipartId"/>.
    /// </summary>
    [JsonIgnore]
    public string UniqueId => string.Join("/", MultipartId);

    /// <summary>
    /// The canonical identifier of the content being vectorized.
    /// Vectorization state services use it to derive the location of the state in the underlying storage.
    /// </summary>
    [JsonPropertyOrder(2)]
    [JsonPropertyName("canonical_id")]
    public required string CanonicalId { get; set; }
}
