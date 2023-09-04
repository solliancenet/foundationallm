using Azure.Search.Documents.Indexes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundationaLLM.SemanticKernel.TextEmbedding;

namespace FoundationaLLM.Core.Models.Search
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
