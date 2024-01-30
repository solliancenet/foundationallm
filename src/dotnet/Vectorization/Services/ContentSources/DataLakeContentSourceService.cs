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
        public async Task<string> ExtractTextFromFileAsync(List<string> multipartId, CancellationToken cancellationToken)
        {
            ValidateMultipartId(multipartId, 3);

            var binaryContent = await _dataLakeStorageService.ReadFileAsync(
                multipartId[1],
                multipartId[2],
                cancellationToken);

            return await ExtractTextFromFileAsync(multipartId[2], binaryContent);
        }
    }
}
