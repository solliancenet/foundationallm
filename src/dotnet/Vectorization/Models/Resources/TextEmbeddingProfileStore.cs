namespace FoundationaLLM.Vectorization.Models.Resources
{
    /// <summary>
    /// Models the content of the text embedding profiles store managed by the FoundationaLLM.Vectorization resource provider.
    /// </summary>
    public class TextEmbeddingProfileStore
    {
        /// <summary>
        /// The list of all embedding profiles that are registered for use by the vectorization pipelines.
        /// </summary>
        public required List<TextEmbeddingProfile> TextEmbeddingProfiles { get; set; }
    }
}
