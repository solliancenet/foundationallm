using FoundationaLLM.Common.Models.TextEmbedding;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Vectorization.Interfaces;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Vectorization.Services.ContentSources
{
    /// <summary>
    /// Implements a vectorization content source for content residing in blob storage.
    /// </summary>
    public class DataLakeContentSourceService : ContentSourceServiceBase, IContentSourceService
    {
        private readonly BlobStorageServiceSettings _storageSettings;
        private readonly ILogger<DataLakeContentSourceService> _logger;
        private readonly DataLakeStorageService _dataLakeStorageService;

        /// <summary>
        /// Creates a new instance of the vectorization content source.
        /// </summary>
        public DataLakeContentSourceService(
            BlobStorageServiceSettings storageSettings,
            ILoggerFactory loggerFactory)
        {
            _storageSettings = storageSettings;
            _logger = loggerFactory.CreateLogger<DataLakeContentSourceService>();
            _dataLakeStorageService = new DataLakeStorageService(
                _storageSettings,
                loggerFactory.CreateLogger<DataLakeStorageService>());
        }

        /// <inheritdoc/>
        /// <remarks>
        /// contentId[0] = the URL of the storage account.
        /// contentId[1] = the container name.
        /// contentId[2] = path of the file relative to the container name.
        /// </remarks>
        public async Task<string> ExtractTextFromFileAsync(ContentIdentifier contentId, CancellationToken cancellationToken)
        {   
            contentId.ValidateMultipartId(3);

            var binaryContent = await _dataLakeStorageService.ReadFileAsync(
                contentId[1],
                contentId[2],
                cancellationToken);

            return await ExtractTextFromFileAsync(contentId.FileName, binaryContent);
        }
    }
}
