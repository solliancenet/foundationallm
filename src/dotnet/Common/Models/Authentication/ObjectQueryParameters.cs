using System.Text.Json.Serialization;

namespace FoundationaLLM.Common.Models.Authentication
{
    /// <summary>
    /// Parameters for querying user and group accounts.
    /// </summary>
    public class ObjectQueryParameters
    {
        /// <summary>
        /// Account name.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// The IDs of the account objects to query.
        /// </summary>
        [JsonPropertyName("ids")]
        public required string[] Ids { get; set; }

        /// <summary>
        /// The current page number.
        /// </summary>
        [JsonPropertyName("page_number")]
        public int? PageNumber { get; set; }

        /// <summary>
        /// The number of items to return in each page.
        /// </summary>
        [JsonPropertyName("page_size")]
        public int? PageSize { get; set; }
    }
}
