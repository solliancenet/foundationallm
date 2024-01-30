using Azure;
using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;

namespace FoundationaLLM.Common.Services
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
        private BlobServiceClient _blobServiceClient;

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
        public async Task WriteFileAsync(
            string containerName,
            string filePath,
            Stream fileContent,
            string? contentType,
            CancellationToken cancellationToken = default)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(filePath);

            fileContent.Seek(0, SeekOrigin.Begin);

            BlobUploadOptions options = new()
            {
                HttpHeaders = new BlobHttpHeaders()
                {
                    ContentType = string.IsNullOrWhiteSpace(contentType)
                        ? "application/json"
                        : contentType
                }
            };

            await blobClient.UploadAsync(fileContent, options, cancellationToken).ConfigureAwait(false);
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
                new Uri($"https://{accountName}.dfs.core.windows.net"),
                new StorageSharedKeyCredential(accountName, accountKey));

        /// <inheritdoc/>
        protected override void CreateClientFromConnectionString(string connectionString) =>
            _blobServiceClient = new BlobServiceClient(connectionString);

        /// <inheritdoc/>
        protected override void CreateClientFromIdentity(string accountName) =>
            _blobServiceClient = new BlobServiceClient(
                new Uri($"https://{accountName}.dfs.core.windows.net"),
                new DefaultAzureCredential());
    }
}
