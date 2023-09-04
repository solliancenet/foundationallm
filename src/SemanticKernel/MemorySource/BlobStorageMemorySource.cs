using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace FoundationaLLM.SemanticKernel.MemorySource
{
    public class BlobStorageMemorySource : IMemorySource
    {
        private readonly BlobStorageMemorySourceSettings _settings;
        private readonly ILogger _logger;

        private BlobStorageMemorySourceConfig _config;

        private readonly BlobServiceClient _blobServiceClient;
        private readonly Dictionary<string, BlobContainerClient> _containerClients;

        public BlobStorageMemorySource(
            IOptions<BlobStorageMemorySourceSettings> settings,
            ILogger<BlobStorageMemorySource> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            _blobServiceClient = new BlobServiceClient(_settings.ConfigBlobStorageConnection);
            _containerClients = new Dictionary<string, BlobContainerClient>();
        }

        public async Task<List<string>> GetMemories()
        {
            await EnsureConfig();

            var filesContent = await Task.WhenAll(_config.FileMemorySources
                .Select(tfms => tfms.Files.Select(tf => ReadTextFileContent(tfms.ContainerName, tf)))
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
                var configContent = await ReadConfigContent(_settings.ConfigBlobStorageContainer, _settings.ConfigFilePath);
                _config = JsonConvert.DeserializeObject<BlobStorageMemorySourceConfig>(configContent);
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

            switch (fileType)
            {
                case ".TXT":
                    return (await (new StreamReader(await blobClient.OpenReadAsync())).ReadToEndAsync(), file.SplitIntoChunks);
                case ".PDF":
                    return (await GetPdfText(blobClient), file.SplitIntoChunks);
                default:
                    return (string.Empty, false);
            }
        }

        private async Task<string> GetPdfText(BlobClient blobClient)
        {
            try
            {
                var ms = new MemoryStream();
                await blobClient.DownloadToAsync(ms);

                using (var pdfDocument = PdfDocument.Open(ms.ToArray()))
                {
                    var pages = pdfDocument.GetPages();
                    return string.Join(Environment.NewLine, pages.Select(p => p.Text).ToArray());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erorr parsing PDF document");
                return string.Empty;
            }
        }
    }
}
