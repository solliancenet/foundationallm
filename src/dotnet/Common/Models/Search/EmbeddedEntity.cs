using Azure.Search.Documents.Indexes;
using FoundationaLLM.Common.Models.TextEmbedding;

namespace FoundationaLLM.Common.Models.Search
{
    public class EmbeddedEntity
    {
        [FieldBuilderIgnore]
        public float[]? vector { get; set; }

        [SearchableField(IsFilterable = true, IsFacetable = true)]
        [EmbeddingField(Label = "Entity (object) type")]
        public string entityType__ { get; set; }    // Since this applies to all business entities,  use a name that is unlikely to cause collisions with other properties
    }
}
