﻿using FoundationaLLM.Common.Interfaces;

namespace FoundationaLLM.Common.Services.Storage
{
    /// <summary>
    /// No-op implementation of the storage service.
    /// </summary>
    /// <remarks>
    /// This implementation should be used by resource providers that are using a different storage mechanism than blob storage.
    /// </remarks>
    public class NullStorageService : IStorageService
    {
        /// <inheritdoc/>
        public string? InstanceName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <inheritdoc/>
        public string? StorageAccountName => throw new NotImplementedException();

        /// <inheritdoc/>
        public string? StorageContainerName => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task DeleteFileAsync(string containerName, string filePath, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task<bool> FileExistsAsync(string containerName, string filePath, CancellationToken cancellationToken) => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task<List<string>> GetFilePathsAsync(string containerName, string? directoryPath = null, bool recursive = true, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <inheritdoc/>
        public BinaryData ReadFile(string containerName, string filePath) => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task<BinaryData> ReadFileAsync(string containerName, string filePath, CancellationToken cancellationToken) => throw new NotImplementedException();
        
        /// <inheritdoc/>
        public Task WriteFileAsync(string containerName, string filePath, Stream fileContent, string? contentType, CancellationToken cancellationToken) => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task WriteFileAsync(string containerName, string filePath, string fileContent, string? contentType, CancellationToken cancellationToken) => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task UpdateJSONFileAsync<T>(string containerName, string filePath, Func<T, T> objectTransformer, CancellationToken cancellationToken) where T : class => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task<List<string>> GetMatchingFilePathsAsync(string containerName, string filePathPattern, CancellationToken cancellationToken = default) => throw new NotImplementedException();

        /// <inheritdoc/>
        public Task CopyFileAsync(string containerName, string sourceFilePath, string destinationFilePath, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
