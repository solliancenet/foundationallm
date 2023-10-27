using FoundationaLLM.Common.Models.TextEmbedding;

namespace FoundationaLLM.Common.Models.Search
{
    /// <summary>
    /// The short term memory object.
    /// </summary>
    public class ShortTermMemory : EmbeddedEntity
    {
        /// <summary>
        /// The memory string associated with the short-term memory.
        /// </summary>
        [EmbeddingField]
        public string? memory__ { get; set; }
    }
}
