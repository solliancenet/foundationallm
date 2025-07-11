﻿namespace FoundationaLLM.Common.Interfaces
{
    /// <summary>
    /// Provides services to interact with a storage.
    /// </summary>
    public interface IStorageService
    {
        /// <summary>
        /// The optional instance name of the storage service.
        /// </summary>
        string? InstanceName { get; set; }

        /// <summary>
        /// The name of the storage account.
        /// </summary>
        string? StorageAccountName { get; }

        /// <summary>
        /// The name of the storage container.
        /// </summary>
        string? StorageContainerName { get; }

        /// <summary>
        /// Reads the binary content of a specified file from the storage.
        /// </summary>
        /// <param name="containerName">The name of the container where the file is located.</param>
        /// <param name="filePath">The path of the file to read.</param>
        /// <param name="cancellationToken">The cancellation token that signals that operations should be cancelled.</param>
        /// <returns>The binary content of the file.</returns>
        Task<BinaryData> ReadFileAsync(string containerName, string filePath, CancellationToken cancellationToken);

        /// <summary>
        /// Reads the binary content of a specified file from the storage.
        /// </summary>
        /// <param name="containerName">The name of the container where the file is located.</param>
        /// <param name="filePath">The path of the file to read.</param>
        /// <returns>The binary content of the file.</returns>
        BinaryData ReadFile(string containerName, string filePath);

        /// <summary>
        /// Writes the binary content to a specified file from the storage.
        /// </summary>
        /// <param name="containerName">The name of the container where the file is located.</param>
        /// <param name="filePath">The path of the file to read.</param>
        /// <param name="fileContent">The binary content written to the file.</param>
        /// <param name="contentType">An optional content type.</param>
        /// <param name="cancellationToken">The cancellation token that signals that operations should be cancelled.</param>
        /// <returns></returns>
        Task WriteFileAsync(string containerName, string filePath, Stream fileContent, string? contentType, CancellationToken cancellationToken);

        /// <summary>
        /// Writes the string content to a specified file from the storage.
        /// </summary>
        /// <param name="containerName">The name of the container where the file is located.</param>
        /// <param name="filePath">The path of the file to read.</param>
        /// <param name="fileContent">The string content written to the file.</param>
        /// <param name="contentType">An optional content type.</param>
        /// <param name="cancellationToken">The cancellation token that signals that operations should be cancelled.</param>
        /// <returns></returns>
        Task WriteFileAsync(string containerName, string filePath, string fileContent, string? contentType, CancellationToken cancellationToken);

        /// <summary>
        /// Updates a specified JSON file in the storage.
        /// </summary>
        /// <typeparam name="T">The type of the object persisted in the JSON file.</typeparam>
        /// <param name="containerName">The name of the container where the file is located.</param>
        /// <param name="filePath">The path of the file to update.</param>
        /// <param name="objectTransformer">A function that updates the object persisted in the JSON file.</param>
        /// <param name="cancellationToken">The cancellation token that signals that operations should be cancelled.</param>
        /// <returns></returns>
        Task UpdateJSONFileAsync<T>(
            string containerName,
            string filePath,
            Func<T, T> objectTransformer,
            CancellationToken cancellationToken) where T : class;

        /// <summary>
        /// Deletes a file from storage.
        /// </summary>
        /// <param name="containerName">The name of the container where the file is located.</param>
        /// <param name="filePath">The path of the file to delete.</param>
        /// <param name="cancellationToken">The cancellation token that signals that operations should be cancelled.</param>
        /// <returns></returns>
        Task DeleteFileAsync(string containerName, string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a file exists on the storage.
        /// </summary>
        /// <param name="containerName">The name of the container where the file is located.</param>
        /// <param name="filePath">The path of the file to read.</param>
        /// <param name="cancellationToken">The cancellation token that signals that operations should be cancelled.</param>
        /// <returns></returns>
        Task<bool> FileExistsAsync(string containerName, string filePath, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a list file paths in the specific directory.
        /// </summary>
        /// <param name="containerName">Name of the container, file system or Workspace name.</param>
        /// <param name="directoryPath">Directory to list file contents</param>
        /// <param name="recursive">Recurse through child folders</param>
        /// <param name="cancellationToken">Determines if a request should be cancelled.</param>
        /// <returns>List of individual file paths located in the container.</returns>        
        Task<List<string>> GetFilePathsAsync(string containerName, string? directoryPath = null, bool recursive = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a list of file paths that match a specific pattern.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="filePathPattern">The file name pattern to match.</param>
        /// <param name="cancellationToken">The cancellation token that signals that operations should be cancelled.</param>
        /// <returns></returns>
        Task<List<string>> GetMatchingFilePathsAsync(string containerName, string filePathPattern, CancellationToken cancellationToken = default);

        /// <summary>
        /// Copies a file from one location to another within the specified storage container.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="sourceFilePath">The path of the file to copy.</param>
        /// <param name="destinationFilePath">The path of the destination.</param>
        /// <param name="cancellationToken">The cancellation token that signals that operations should be cancelled.</param>
        /// <returns></returns>
        Task CopyFileAsync(string containerName, string sourceFilePath, string destinationFilePath, CancellationToken cancellationToken = default);
    }
}
