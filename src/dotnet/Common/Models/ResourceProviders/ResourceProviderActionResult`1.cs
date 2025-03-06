using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.ResourceProviders
{
    /// <summary>
    /// The result of an action executed by a resource provider.
    /// </summary>
    /// <param name="ObjectId">The object id to which the result refers to.</param>
    /// <param name="IsSuccessResult">Indicates whether the action executed successfully or not.</param>
    /// <param name="ErrorMessage">When IsSuccess is false, contains an error message with details.</param>
    public record ResourceProviderActionResult<T>(
        string ObjectId,
        bool IsSuccessResult,
        string? ErrorMessage = null) : ResourceProviderActionResult(ObjectId, IsSuccessResult, ErrorMessage)
        where T : ResourceBase
    {
        /// <summary>
        /// Gets or sets the resource resulting from the action.
        /// </summary>
        /// <remarks>
        /// Each resource provider will decide whether to return the resource in the action result or not.
        /// </remarks>
        [JsonPropertyName("resource")]
        public T? Resource { get; set; }
    }
}
