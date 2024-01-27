namespace FoundationaLLM.Common.Models.Vectorization
{
    /// <summary>
    /// Basic properties for vectorization profiles.
    /// </summary>
    public class VectorizationProfileBase
    {
        /// <summary>
        /// The name of the vectorization profile.
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// The configuration associated with the vectorization profile.
        /// </summary>
        public Dictionary<string, string>? Settings { get; set; } = [];
    }
}
