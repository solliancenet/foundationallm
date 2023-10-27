using Azure.Search.Documents.Indexes;
using FoundationaLLM.Common.Models.TextEmbedding;

namespace FoundationaLLM.Common.Models.Search
{
    /// <summary>
    /// The embedded entity object.
    /// </summary>
    public class EmbeddedEntity
    {
        /// <summary>
        /// The vector associated with the embedded entity. 
        /// </summary>
        [FieldBuilderIgnore]
        public float[]? vector { get; set; }

        /// <summary>
        /// The type of the entity.
        /// </summary>
        [SearchableField(IsFilterable = true, IsFacetable = true)]
        [EmbeddingField(Label = "Entity (object) type")]
        public string? entityType__ { get; set; }    // Since this applies to all business entities,  use a name that is unlikely to cause collisions with other properties
    }
}
