using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundationaLLM.SemanticKernel.TextEmbedding;

namespace FoundationaLLM.Core.Models.Search
{
    public class ShortTermMemory : EmbeddedEntity
    {
        [EmbeddingField]
        public string memory__ { get; set; }
    }
}
