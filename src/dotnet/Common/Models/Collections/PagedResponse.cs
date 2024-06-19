using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Collections
{
    /// <summary>
    /// Represents a paged response that includes a list of items and a flag indicating if there are more items to retrieve.
    /// </summary>
    /// <typeparam name="T">The type of items to return.</typeparam>
    public class PagedResponse<T>
    {
        /// <summary>
        /// The list of items to return.
        /// </summary>
        [JsonPropertyName("items")]
        public IEnumerable<T>? Items { get; set; }

        /// <summary>
        /// The total number of items available. This is the total count, not necessarily the number of items returned in the response.
        /// Please note that certain APIs only return a count on the first page of results.
        /// </summary>
        [JsonPropertyName("total_items")]
        public long? TotalItems { get; set; }

        /// <summary>
        /// Indicates if there are more items to retrieve.
        /// </summary>
        [JsonPropertyName("has_next_page")]
        public bool HasNextPage { get; set; }
    }
}
