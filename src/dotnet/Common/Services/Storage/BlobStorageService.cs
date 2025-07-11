﻿using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Common.Services.Storage
{
    /// <summary>
    /// Provides access to Azure blob storage.
    /// </summary>
    /// <remarks>
    ///  Initializes a new instance of the <see cref="BlobStorageService"/> with the specified options and logger.
    /// </remarks>
    /// <param name="storageOptions">The options object containing the <see cref="BlobStorageServiceSettings"/> object with the settings.</param>
    /// <param name="logger">The logger used for logging.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class BlobStorageService(
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        IOptions<BlobStorageServiceSettings> storageOptions,
        ILogger<BlobStorageService> logger) : StorageServiceBase(storageOptions, logger), IStorageService
    {
        private BlobServiceClient _blobServiceClient = null!;

        /// <inheritdoc/>
        public async Task<BinaryData> ReadFileAsync(
            string containerName,
            string filePath,
            CancellationToken cancellationToken = default)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(filePath);

            try
            {
                Response<BlobDownloadResult>? content = await blobClient.DownloadContentAsync(cancellationToken).ConfigureAwait(false);

                if (content != null && content.HasValue)
                {
                    return content.Value.Content;
                }

                throw new ContentException($"Cannot read file {filePath} from container {containerName}.");
            }
            catch (RequestFailedException e) when (e.Status == 404)
            {
                _logger.LogWarning("File not found: {FilePath}", filePath);
                throw new ContentException("File not found.", e);
            }
        }

        /// <inheritdoc/>
        public BinaryData ReadFile(
            string containerName,
            string filePath)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(filePath);

            try
            {
                Response<BlobDownloadResult>? content = blobClient.DownloadContent();

                if (content != null && content.HasValue)
                {
                    return content.Value.Content;
                }

                throw new ContentException($"Cannot read file {filePath} from container {containerName}.");
            }
            catch (RequestFailedException e) when (e.Status == 404)
            {
                _logger.LogWarning("File not found: {FilePath}", filePath);
                throw new ContentException("File not found.", e);
            }
        }

        /// <inheritdoc/>
        public async Task WriteFileAsync(
            string containerName,
            string filePath,
            Stream fileContent,
            string? contentType,
            CancellationToken cancellationToken = default)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(filePath);
            var blobLeaseClient = blobClient.GetBlobLeaseClient();

            // We are using pessimistic concurrency by default.
            // For more details, see https://learn.microsoft.com/en-us/azure/storage/blobs/concurrency-manage.

            BlobLease? blobLease = default;

            try
            {
                if (await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false))
                {
                    // We only need to get a lease for already existing blobs that are being updated.
                    blobLease = await blobLeaseClient.AcquireAsyncWithWait(TimeSpan.FromSeconds(60), cancellationToken: cancellationToken);
                    if (blobLease == null)
                    {
                        _logger.LogError("Could not get a lease for the blob {FilePath} from container {ContainerName}. Reason: unkown.", filePath, containerName);
                        throw new StorageException($"Could not get a lease for the blob {filePath} from container {containerName}. Reason: unknown.");
                    }
                }

                fileContent.Seek(0, SeekOrigin.Begin);

                BlobUploadOptions options = new()
                {
                    HttpHeaders = new BlobHttpHeaders()
                    {
                        ContentType = string.IsNullOrWhiteSpace(contentType)
                            ? "application/json"
                            : contentType
                    },
                    Conditions = (blobLease != null)
                    ? new BlobRequestConditions()
                        {
                            LeaseId = blobLease!.LeaseId
                        }
                    : default
                };

                await blobClient.UploadAsync(fileContent, options, cancellationToken).ConfigureAwait(false);
            }
            catch (RequestFailedException ex)
            {
                if (ex.Status == (int)HttpStatusCode.Conflict
                        && ex.ErrorCode == "LeaseAlreadyPresent")
                {
                    _logger.LogError(ex, "Could not get a lease for the blob {FilePath} from container {ContainerName}. " +
                        "Reason: an existing lease is preventing acquiring a new lease.",
                        filePath, containerName);
                    throw new StorageException($"Could not get a lease for the blob {filePath} from container {containerName}. " +
                        "Reason: an existing lease is preventing acquiring a new lease.", ex);
                }

                throw new StorageException($"Could not get a lease for the blob {filePath} from container {containerName}. Reason: unknown.", ex);
            }
            finally
            {
                if (blobLease != null)
                    await blobLeaseClient.ReleaseAsync(cancellationToken: cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task WriteFileAsync(
            string containerName,
            string filePath,
            string fileContent,
            string? contentType,
            CancellationToken cancellationToken = default) =>
            await WriteFileAsync(
                containerName,
                filePath,
                new MemoryStream(Encoding.UTF8.GetBytes(fileContent)),
                contentType,
                cancellationToken).ConfigureAwait(false);

        /// <inheritdoc/>
        public async Task UpdateJSONFileAsync<T>(
            string containerName,
            string filePath,
            Func<T, T> objectTransformer,
            CancellationToken cancellationToken) where T : class
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(filePath);
            var blobLeaseClient = blobClient.GetBlobLeaseClient();

            // We are using pessimistic concurrency by default.
            // For more details, see https://learn.microsoft.com/en-us/azure/storage/blobs/concurrency-manage.

            BlobLease? blobLease = default;

            try
            {
                // Always acquire a lease for the blob to ensure that we have exclusive access to it.
                blobLease = await blobLeaseClient.AcquireAsyncWithWait(TimeSpan.FromSeconds(60), cancellationToken: cancellationToken);
                if (blobLease == null)
                {
                    _logger.LogError("Could not get a lease for the blob {FilePath} from container {ContainerName}. Reason: unkown.", filePath, containerName);
                    throw new StorageException($"Could not get a lease for the blob {filePath} from container {containerName}. Reason: unknown.");
                }

                var contentResult = await blobClient.DownloadContentAsync(cancellationToken).ConfigureAwait(false);

                var fileContent = default(T);
                if (contentResult != null && contentResult.HasValue)
                {
                    if (JsonSerializer.Deserialize(contentResult.Value.Content, typeof(T)) is T content)
                    {
                        fileContent = objectTransformer(content);
                    }
                    else
                    {
                        _logger.LogError("Could not deserialize the content of the blob {FilePath} from container {ContainerName} to type {TypeName}.",
                            filePath, containerName, nameof(T));
                        throw new StorageException(
                            $"Could not deserialize the content of the blob {filePath} from container {containerName} to type {nameof(T)}.");
                    }
                }
                else
                    throw new StorageException($"Cannot read file {filePath} from container {containerName}.");

                BlobUploadOptions options = new()
                {
                    HttpHeaders = new BlobHttpHeaders()
                    {
                        ContentType = "application/json"
                    },
                    Conditions = (blobLease != null)
                    ? new BlobRequestConditions()
                    {
                        LeaseId = blobLease!.LeaseId
                    }
                    : default
                };

                await blobClient.UploadAsync(
                    new BinaryData(JsonSerializer.Serialize(fileContent)),
                    options,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (StorageException)
            {
                throw;
            }
            catch (RequestFailedException ex) when (ex.Status == (int)HttpStatusCode.NotFound)
            {
                _logger.LogError("File not found: {FilePath}", filePath);
                throw new StorageException("File not found.", ex);
            }
            catch (RequestFailedException ex) when (
                ex.Status == (int)HttpStatusCode.Conflict
                && ex.ErrorCode == "LeaseAlreadyPresent")
            {
                _logger.LogError(ex, "Could not get a lease for the blob {FilePath} from container {ContainerName}. " +
                    "Reason: an existing lease is preventing acquiring a new lease.",
                    filePath, containerName);
                throw new StorageException($"Could not get a lease for the blob {filePath} from container {containerName}. " +
                    "Reason: an existing lease is preventing acquiring a new lease.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the JSON file {FilePath} in container {ContainerName}.",
                    filePath,
                    containerName);
                throw new StorageException($"An error occurred while updating the JSON file {filePath} in container {containerName}.", ex);
            }
            finally
            {
                if (blobLease != null)
                    await blobLeaseClient.ReleaseAsync(cancellationToken: cancellationToken);
            }
        }

        /// <inheritdoc/>
        public async Task DeleteFileAsync(string containerName, string filePath,
            CancellationToken cancellationToken = default)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(filePath);

            try
            {
                await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            catch (RequestFailedException e) when (e.Status == 404)
            {
                _logger.LogWarning("File not found: {FilePath}", filePath);
                throw new ContentException("File not found.", e);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> FileExistsAsync(
            string containerName,
            string filePath,
            CancellationToken cancellationToken = default)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(filePath);

            return await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override void CreateClientFromAccountKey(string accountName, string accountKey) =>
            _blobServiceClient = new BlobServiceClient(
                new Uri($"https://{accountName}.blob.core.windows.net"),
                new StorageSharedKeyCredential(accountName, accountKey));

        /// <inheritdoc/>
        protected override void CreateClientFromConnectionString(string connectionString) =>
            _blobServiceClient = new BlobServiceClient(connectionString);

        /// <inheritdoc/>
        protected override void CreateClientFromIdentity(string accountName) =>
            _blobServiceClient = new BlobServiceClient(
                new Uri($"https://{accountName}.blob.core.windows.net"),
                ServiceContext.AzureCredential);

        /// <inheritdoc/>
        public async Task<List<string>> GetFilePathsAsync(string containerName, string? directoryPath = null, bool recursive = true, CancellationToken cancellationToken = default)
        {
            var fullListing = new List<string>(); // Full listing of directory and file paths  
            var filePaths = new List<string>(); // List to store only file paths  
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            if (recursive)
            {
                // Flat listing (recursive)  
                await foreach (var blob in containerClient.GetBlobsAsync(prefix: directoryPath, cancellationToken: cancellationToken))
                {
                    if (!blob.Name.Equals(directoryPath))
                    {
                        fullListing.Add(blob.Name);
                    }                    
                }
                // Filter out subpaths, note that empty folders will not be filtered out
                // there is no way of knowing if an empty folder is a blob or a virtual directory
                filePaths = FilterSubpaths(fullListing);
            }
            else
            {
                // Hierarchical listing (non-recursive)  
                var prefix = string.IsNullOrEmpty(directoryPath) ? null : directoryPath.TrimEnd('/');

                if (!directoryPath.Contains("requests"))
                    directoryPath += "/";

                await foreach (var blobHierarchyItem in containerClient.GetBlobsByHierarchyAsync(delimiter: "/", prefix: prefix, cancellationToken: cancellationToken))
                {
                    if (blobHierarchyItem.IsBlob)
                    {
                        filePaths.Add(blobHierarchyItem.Blob.Name);
                    }
                    // Do not add if the item is a prefix (which represents a virtual directory)  
                }
            }

            return filePaths;
        }

        /// <inheritdoc/>
        public async Task<List<string>> GetMatchingFilePathsAsync(
            string containerName,
            string filePathPattern,
            CancellationToken cancellationToken = default)
        {
            var fullListing = new List<string>(); // Full listing of directory and file paths  
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            _logger.LogDebug("Getting matching file paths in container {ContainerName} with pattern {FilePathPattern}.", containerName, filePathPattern);

            // Flat listing (recursive)  
            await foreach (var blob in containerClient.GetBlobsAsync(prefix: filePathPattern, cancellationToken: cancellationToken))
            {
                fullListing.Add(blob.Name);
            }

            _logger.LogDebug("Found {Count} matching file paths in container {ContainerName} with pattern {FilePathPattern}.", fullListing.Count, containerName, filePathPattern);

            return fullListing;
        }

        /// <inheritdoc/>
        public async Task CopyFileAsync(
            string containerName,
            string sourceFilePath,
            string destinationFilePath,
            CancellationToken cancellationToken = default)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var sourceBlobClient = containerClient.GetBlobClient(sourceFilePath);
            var destinationBlobClient = containerClient.GetBlobClient(destinationFilePath);

            // Start the copy operation
            var copyOperation = await destinationBlobClient.StartCopyFromUriAsync(
                sourceBlobClient.Uri,
                cancellationToken: cancellationToken
            ).ConfigureAwait(false);

            // Wait for the copy to complete
            BlobProperties destProperties;
            do
            {
                await Task.Delay(500, cancellationToken).ConfigureAwait(false);
                destProperties = await destinationBlobClient.GetPropertiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            while (destProperties.CopyStatus == CopyStatus.Pending);

            if (destProperties.CopyStatus != CopyStatus.Success)
            {
                _logger.LogError("Failed to copy blob from {Source} to {Destination}. Status: {Status}, Description: {Description}",
                    sourceFilePath, destinationFilePath, destProperties.CopyStatus, destProperties.CopyStatusDescription);
                throw new StorageException($"Failed to copy blob from {sourceFilePath} to {destinationFilePath}. Status: {destProperties.CopyStatus}, Description: {destProperties.CopyStatusDescription}");
            }

        }

        /// <summary>
        /// Removes subpaths (directories) from the list of paths.
        /// </summary>
        /// <param name="paths"></param>
        /// <returns>List of file paths.</returns>
        private List<string> FilterSubpaths(List<string> paths)
        {          
            List<string> filteredPaths = new List<string>(paths);

            // Sort the list by length in descending order to ensure we always keep the longest strings  
            filteredPaths.Sort((a, b) => b.Length.CompareTo(a.Length));

            // Compare each path with all others to see if it's contained within any other path (indicative of directory)
            for (int i = 0; i < filteredPaths.Count; i++)
            {
                for (int j = i + 1; j < filteredPaths.Count; j++)
                {                    
                    if (filteredPaths[i].Contains(filteredPaths[j]))
                    {
                        filteredPaths.RemoveAt(j);                         
                        j--;
                    }
                }
            }
            return filteredPaths;
        }
    }
}
