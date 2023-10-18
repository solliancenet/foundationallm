using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.SemanticKernel.Core.Interfaces
{
    /// <summary>
    /// Interface for the Memory Source service.
    /// </summary>
    public interface IMemorySource
    {
        /// <summary>
        /// Gets a list of memories.
        /// </summary>
        /// <returns>The list of memories.</returns>
        Task<List<string>> GetMemories();
    }
}
