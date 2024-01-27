using Azure.Storage.Blobs;
using FoundationaLLM.SemanticKernel.Core.Interfaces;
using FoundationaLLM.SemanticKernel.Core.Models.ConfigurationOptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Text;
using Newtonsoft.Json;
using UglyToad.PdfPig;

namespace FoundationaLLM.SemanticKernel.MemorySource
{
    /// <summary>
    /// Azure Blob Storage memory source implementing <see cref="IMemorySource"/>.
    /// </summary>
    public class BlobStorageMemorySource : IMemorySource
    {
        private readonly BlobStorageMemorySourceSettings _settings;
        private readonly ILogger _logger;

        private BlobStorageMemorySourceConfig? _config;

        private readonly BlobServiceClient _blobServiceClient;
        private readonly Dictionary<string, BlobContainerClient> _containerClients;

        /// <summary>
        /// Constructor for the Azure Blob Storage memory source.
        /// </summary>
        /// <param name="settings">The configuration options for the Azure Blob Storage memory source.</param>
        /// <param name="logger">The logger for the Azure Blob Storage memory source.</param>
        public BlobStorageMemorySource(
            IOptions<BlobStorageMemorySourceSettings> settings,
            ILogger<BlobStorageMemorySource> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            _blobServiceClient = new BlobServiceClient(_settings.BlobStorageConnection);
            _containerClients = new Dictionary<string, BlobContainerClient>();
        }

        /// <summary>
        /// Gets a list of memories from the configured memory source.
        /// </summary>
        /// <returns>A list of memories.</returns>
        public async Task<List<string>> GetMemories()
        {
            await EnsureConfig();

            var filesContent = await Task.WhenAll(_config!.TextFileMemorySources
                .Select(tfms => tfms.TextFiles.Select(tf => ReadTextFileContent(tfms.ContainerName, tf)))
                .SelectMany(x => x));

            var chunkedFilesContent = filesContent
                .Where(x => !string.IsNullOrWhiteSpace(x.Content))
                .Select(txt => txt.SplitIntoChunks ? TextChunker.SplitPlainTextLines(txt.Content, _config.TextChunkMaxTokens) : new List<string>() { txt.Content })
                .SelectMany(x => x).ToList();

            return chunkedFilesContent;
        }

        private async Task EnsureConfig()
        {
            if (_config == null)
            {
                var configContent = await ReadConfigContent(_settings.BlobStorageContainer, _settings.ConfigFilePath);

                var config = JsonConvert.DeserializeObject<BlobStorageMemorySourceConfig>(configContent);

                if (config != null)
                    _config = config;
                else
                    throw new Exception("Could not ensure that the Blob Storage Memory Source config was loaded.");
            }
        }

        private BlobContainerClient GetBlobContainerClient(string containerName)
        {
            if (!_containerClients.ContainsKey(containerName))
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                _containerClients.Add(containerName, containerClient);
                return containerClient;
            }

            return _containerClients[containerName];
        }

        private async Task<string> ReadConfigContent(string containerName, string filePath)
        {
            var containerClient = GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(filePath);
            var reader = new StreamReader(await blobClient.OpenReadAsync());
            return await reader.ReadToEndAsync();
        }

        private async Task<(string Content, bool SplitIntoChunks)> ReadTextFileContent(string containerName, FileMemorySourceFile file)
        {
            var containerClient = GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(file.FileName);
            var fileType = Path.GetExtension(file.FileName).ToUpper();

            return fileType switch
            {
                ".TXT" => (await (new StreamReader(await blobClient.OpenReadAsync())).ReadToEndAsync(), file.SplitIntoChunks),
                ".PDF" => (await GetPdfText(blobClient), file.SplitIntoChunks),
                _ => (string.Empty, false),
            };
        }

        private async Task<string> GetPdfText(BlobClient blobClient)
        {
            try
            {
                var ms = new MemoryStream();
                await blobClient.DownloadToAsync(ms);

                using var pdfDocument = PdfDocument.Open(ms.ToArray());
                var pages = pdfDocument.GetPages();
                return string.Join(Environment.NewLine, pages.Select(p => p.Text).ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erorr parsing PDF document");
                return string.Empty;
            }
        }
    }
}
