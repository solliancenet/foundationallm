using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Models.TextEmbedding
{
    /// <summary>
    /// Provides information about embedded content.
    /// </summary>
    public class EmbeddedContent
    {
        /// <summary>
        /// The canonical identifier of the content.
        /// </summary>
        public required ContentIdentifier ContentId { get; set; }

        /// <summary>
        /// The list of conent 
        /// </summary>
        public required List<EmbeddedContentPart> ContentParts { get; set; } = [];
    }
}
