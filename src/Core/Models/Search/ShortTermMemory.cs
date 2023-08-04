using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Solliance.AICopilot.SemanticKernel.TextEmbedding;

namespace Solliance.AICopilot.Core.Models.Search
{
    public class ShortTermMemory : EmbeddedEntity
    {
        [EmbeddingField]
        public string memory__ { get; set; }
    }
}
