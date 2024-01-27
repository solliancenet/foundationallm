namespace FoundationaLLM.Vectorization.Models.Resources
{
    /// <summary>
    /// Models the content of the indexing profiles store managed by the FoundationaLLM.Vectorization resource provider.
    /// </summary>
    public class IndexingProfileStore
    {
        /// <summary>
        /// The list of all indexing profiles that are registered for use by the vectorization pipelines.
        /// </summary>
        public required List<IndexingProfile> IndexingProfiles { get; set; }
    }
}
