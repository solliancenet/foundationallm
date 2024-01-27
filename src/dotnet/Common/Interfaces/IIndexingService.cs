using FoundationaLLM.Common.Models.TextEmbedding;

namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Provides indexing capabilities for embedding vectors.
    /// </summary>
    public interface IIndexingService
    {
        /// <summary>
        /// Adds to a specified index the list of embeddings associated with a content.
        /// </summary>
        /// <param name="embeddedContent">The <see cref="EmbeddedContent"/> containind the embeddings to index.</param>
        /// <param name="indexName">The name of the index.</param>
        /// <returns></returns>
        Task<List<string>> IndexEmbeddingsAsync(EmbeddedContent embeddedContent, string indexName);
    }
}
