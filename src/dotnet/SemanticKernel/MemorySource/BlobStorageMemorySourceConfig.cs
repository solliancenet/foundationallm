using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.SemanticKernel.MemorySource
{
    public class BlobStorageMemorySourceConfig
    {
        public int TextChunkMaxTokens { get; init; }

        public List<FileMemorySource> FileMemorySources { get; init; }
    }

    public class FileMemorySource
    {
        public string ContainerName { get; init; }
        public List<FileMemorySourceFile> Files { get; init; }
    }

    public class FileMemorySourceFile
    {
        public string FileName { get; init; }
        public bool SplitIntoChunks { get; init; }
    }
}
