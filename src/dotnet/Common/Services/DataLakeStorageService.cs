using Azure;
using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Files.DataLake;
using FoundationaLLM.Common.Exceptions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Common.Services
{
    /// <summary>
    /// Provides access to Azure Data Lake blob storage.
    /// </summary>
    public class DataLakeStorageService : StorageServiceBase, IStorageService
    {
        private DataLakeServiceClient _dataLakeClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataLakeStorageService"/> with the specified options and logger.
        /// </summary>
        /// <param name="options">The options object containing the <see cref="BlobStorageServiceSettings"/> object with the blob storage settings.</param>
        /// <param name="logger">The logger used for logging.</param>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public DataLakeStorageService(
            IOptions<BlobStorageServiceSettings> options,
            ILogger<DataLakeStorageService> logger) : base(options, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataLakeStorageService"/> with the specified options and logger.
        /// </summary>
        /// <param name="storageSettings">The <see cref="BlobStorageServiceSettings"/> object with the blob storage settings.</param>
        /// <param name="logger">The logger used for logging.</param>
        public DataLakeStorageService(
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
            BlobStorageServiceSettings storageSettings,
            ILogger<DataLakeStorageService> logger) : base(storageSettings, logger)
        {
        }

        /// <inheritdoc/>
        public async Task<BinaryData> ReadFileAsync(
            string containerName,
            string filePath,
            CancellationToken cancellationToken = default)
        {
            var fileSystemClient = _dataLakeClient.GetFileSystemClient(containerName);
            var fileClient = fileSystemClient.GetFileClient(filePath);

            try
            {
                var memoryStream = new MemoryStream();
                var result = await fileClient.ReadToAsync(memoryStream, null, cancellationToken).ConfigureAwait(false);

                if (result.IsError)
                    throw new ContentException($"Cannot read file {filePath} from file system {containerName}.");

                memoryStream.Seek(0, SeekOrigin.Begin);
                return BinaryData.FromStream(memoryStream);
            }
            catch (RequestFailedException e) when (e.Status == 404)
            {
                _logger.LogWarning("File not found: {FilePath}", filePath);
                throw new ContentException("File not found.", e);
            }
        }

        /// <inheritdoc/>
        public Task WriteFileAsync(
            string containerName,
            string filePath,
            Stream fileContent,
            string? contentType,
            CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        /// <inheritdoc/>
        public Task WriteFileAsync(
            string containerName,
            string filePath,
            string fileContent,
            string? contentType,
            CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        /// <inheritdoc/>
        public async Task<bool> FileExistsAsync(
            string containerName,
            string filePath,
            CancellationToken cancellationToken = default)
        {
            var fileSystemClient = _dataLakeClient.GetFileSystemClient(containerName);
            var fileClient = fileSystemClient.GetFileClient(filePath);

            return await fileClient.ExistsAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override void CreateClientFromAccountKey(string accountName, string accountKey) =>
            _dataLakeClient = new DataLakeServiceClient(
                new Uri($"https://{accountName}.dfs.core.windows.net"),
                new StorageSharedKeyCredential(accountName, accountKey));

        /// <inheritdoc/>
        protected override void CreateClientFromConnectionString(string connectionString) =>
            _dataLakeClient = new DataLakeServiceClient(connectionString);

        /// <inheritdoc/>
        protected override void CreateClientFromIdentity(string accountName) =>
            _dataLakeClient = new DataLakeServiceClient(
                new Uri($"https://{accountName}.dfs.core.windows.net"),
                new DefaultAzureCredential());
    }
}
