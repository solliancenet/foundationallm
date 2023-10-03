using FoundationaLLM.Common.Models.TextEmbedding;

namespace FoundationaLLM.Common.Models.Search
{
    public class ShortTermMemory : EmbeddedEntity
    {
        [EmbeddingField]
        public string memory__ { get; set; }
    }
}
