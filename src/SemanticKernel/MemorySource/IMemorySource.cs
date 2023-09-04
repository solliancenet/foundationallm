using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.SemanticKernel.MemorySource
{
    public interface IMemorySource
    {
        Task<List<string>> GetMemories();
    }
}
