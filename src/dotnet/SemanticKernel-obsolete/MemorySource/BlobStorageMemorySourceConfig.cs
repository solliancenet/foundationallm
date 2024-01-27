namespace FoundationaLLM.SemanticKernel.MemorySource
{
    /// <summary>
    /// The Blob Storage memory source configuration class.
    /// </summary>
    public class BlobStorageMemorySourceConfig
    {
        /// <summary>
        /// The maximum tokens for the text chunk.
        /// </summary>
        public int TextChunkMaxTokens { get; init; }

        /// <summary>
        /// The list of file memory sources.
        /// </summary>
        public required List<FileMemorySource> TextFileMemorySources { get; init; }
    }

    /// <summary>
    /// The FileMemorySource class.
    /// </summary>
    public class FileMemorySource
    {
        /// <summary>
        /// The name of the blob storage container.
        /// </summary>
        public required string ContainerName { get; init; }

        /// <summary>
        /// The list of file memory source files.
        /// </summary>
        public required List<FileMemorySourceFile> TextFiles { get; init; }
    }

    /// <summary>
    /// The FileMemorySourceFile class.
    /// </summary>
    public class FileMemorySourceFile
    {
        /// <summary>
        /// The name of the file.
        /// </summary>
        public required string FileName { get; init; }

        /// <summary>
        /// The flag representing if the file is split into chunks.
        /// </summary>
        public bool SplitIntoChunks { get; init; }
    }
}
