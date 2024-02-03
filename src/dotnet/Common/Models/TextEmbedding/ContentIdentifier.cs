using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.TextEmbedding;

/// <summary>
/// Represents the content associated with a vectorization request.
/// </summary>
public class ContentIdentifier
{
    /// <summary>
    /// The name of the vectorization content source profile that provides settings to connect to the content source.
    /// </summary>
    [JsonPropertyOrder(0)]
    [JsonPropertyName("content_source_profile_name")]
    public required string ContentSourceProfileName { get; set; }

    /// <summary>
    /// The multipart unique identifier of the the content (i.e. document) being vectorized.
    /// </summary>
    [JsonInclude]
    [JsonPropertyOrder(1)]
    [JsonPropertyName("multipart_id")]
    public required List<string> MultipartId { private get; set; }

    /// <summary>
    /// The canonical identifier of the content being vectorized.
    /// Vectorization state services use it to derive the location of the state in the underlying storage.
    /// </summary>
    [JsonPropertyOrder(2)]
    [JsonPropertyName("canonical_id")]
    public required string CanonicalId { get; set; }

    /// <summary>
    /// The unique identifier of the content (i.e., document) being vectorized.
    /// The identifier is determined by concatenating the parts from <see cref="MultipartId"/>.
    /// </summary>
    [JsonIgnore]
    public string UniqueId => string.Join("/", MultipartId);

    /// <summary>
    /// The file name part of the content identifier.
    /// </summary>
    [JsonIgnore]
    public string FileName => MultipartId.Last();

    /// <summary>
    /// Validates a multipart unique content identifier.
    /// </summary>
    /// <param name="expectedPartsCount">The expected number of parts in the multipart identifier.</param>
    /// <exception cref="ContentIdentifierException"></exception>
    public void ValidateMultipartId(int expectedPartsCount)
    {
        if (MultipartId == null
            || MultipartId.Count != expectedPartsCount
            || MultipartId.Any(t => string.IsNullOrWhiteSpace(t)))
            throw new ContentIdentifierException("Invalid multipart identifier.");
    }

    /// <summary>
    /// The indexer allowing to access the components of the multipart identifier using [] notation.
    /// </summary>
    /// <param name="i">The index of component being retrieved.</param>
    /// <returns></returns>
    [JsonIgnore]
    public string this[int i]
    {
        get
        {
            if (i < 0 || i >= MultipartId.Count)
                throw new ContentIdentifierException("The specified multipart content identifier index is invalid.");

            // The first component of the multipart content identifier is always tested for a known URL pattern.
            // Some content sources do not requre the first component to be a URL.
            // This case is handled gracefully by the FromKnownNeutralUrl extension.

            return MultipartId.Count > 1
                ? MultipartId[i].FromKnownNeutralUrl()
                : MultipartId[i];
        }
    }
}
