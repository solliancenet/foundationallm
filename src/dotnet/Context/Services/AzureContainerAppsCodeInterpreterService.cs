using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.CodeExecution;
using FoundationaLLM.Context.Constants;
using FoundationaLLM.Context.Exceptions;
using FoundationaLLM.Context.Interfaces;
using FoundationaLLM.Context.Models;
using FoundationaLLM.Context.Models.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace FoundationaLLM.Context.Services
{
    /// <summary>
    /// Provides a code session service that uses Azure Container Apps Dynamic Sessions to execute code.
    /// </summary>
    /// <param name="httpClientFactory">The factory used to create <see cref="HttpClient"/> instances.</param>
    /// <param name="options">The options for the Azure Container Apps code execution service.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class AzureContainerAppsCodeInterpreterService(
        IHttpClientFactory httpClientFactory,
        IOptions<AzureContainerAppsCodeInterpreterServiceSettings> options,
        ILogger<AzureContainerAppsCodeInterpreterService> logger) :
        AzureContainerAppsServiceBase(httpClientFactory, logger), ICodeSessionProviderService
    {
        private readonly AzureContainerAppsCodeInterpreterServiceSettings _settings = options.Value;

        /// <inheritdoc />
        public string ProviderName => CodeSessionProviderNames.AzureContainerAppsCodeInterpreter;

        /// <inheritdoc />
        public async Task<CreateCodeSessionResponse> CreateCodeSession(
            string instanceId,
            string agentName,
            string conversationId,
            string context,
            string language,
            UnifiedUserIdentity userIdentity) =>
            _settings.Endpoints.TryGetValue(language, out var endpoints)
            && endpoints != null
            && endpoints.Count > 0
                ? await CreateCodeSessionInternal(
                    instanceId,
                    agentName,
                    conversationId,
                    context,
                    _settings.Endpoints[language].First(),
                    userIdentity)
                : throw new ContextServiceException(
                    $"Cound not find any endpoints for the [{language}] language.",
                    StatusCodes.Status400BadRequest);

        /// <inheritdoc />
        public async Task<bool> UploadFileToCodeSession(
            string codeSessionId,
            string endpoint,
            string fileName,
            Stream fileContent)
        {
            var httpClient = await CreateHttpClient();

            var multipartFormDataContent = new MultipartFormDataContent();
            var streamContent = new StreamContent(fileContent);
            streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "file",
                FileName = fileName
            };
            multipartFormDataContent.Add(streamContent);

            var responseMessage = await httpClient.PostAsync(
                $"{endpoint}/files?api-version=2024-10-02-preview&identifier={codeSessionId}",
                multipartFormDataContent);

            return responseMessage.IsSuccessStatusCode;
        }

        /// <inheritdoc />
        public async Task<List<CodeSessionFileStoreItem>> GetCodeSessionFileStoreItems(
            string codeSessionId,
            string endpoint)
        {
            var httpClient = await CreateHttpClient();

            var itemsToReturn = await GetCodeSessionFileStoreItems(
                codeSessionId,
                endpoint,
                httpClient);

            return itemsToReturn;
        }

        /// <inheritdoc/>
        public async Task DeleteCodeSessionFileStoreItems(
            string codeSessionId,
            string endpoint)
        {
            var httpClient = await CreateHttpClient();

            var itemsToDelete = await GetCodeSessionFileStoreItems(
                codeSessionId,
                endpoint,
                httpClient,
                includeFolders: true);

            foreach (var item in itemsToDelete)
            {
                var url = $"{endpoint}/files/{item.Name}?api-version=2024-10-02-preview&identifier={codeSessionId}&path={item.ParentPath}";
                var responseMessage = await httpClient.DeleteAsync(url);
                if (!responseMessage.IsSuccessStatusCode)
                    _logger.LogError("Unable to delete file {FileName} from code session {CodeSession}.",
                        item.Name, codeSessionId);
            }
        }

        /// <inheritdoc />
        public async Task<Stream?> DownloadFileFromCodeSession(
            string codeSessionId,
            string endpoint,
            string fileName,
            string filePath)
        {
            var httpClient = await CreateHttpClient();

            var responseMessage = await httpClient.GetAsync(
                $"{endpoint}/files/{fileName}/content?api-version=2024-10-02-preview&identifier={codeSessionId}&path={filePath}");

            if (responseMessage.IsSuccessStatusCode)
                return responseMessage.Content.ReadAsStream();
            else
                return null;
        }

        private async Task<List<CodeSessionFileStoreItem>> GetCodeSessionFileStoreItems(
            string codeSessionId,
            string endpoint,
            HttpClient httpClient,
            bool includeFolders = false,
            bool includeLocalPath = false)
        {
            var rootUrl = $"{endpoint}/files?api-version=2024-10-02-preview&identifier={codeSessionId}";
            var rootFileStore = await GetCodeSessionFileStore(
                httpClient,
                rootUrl,
                string.Empty);

            if (rootFileStore.Items.Count == 0)
                return [];

            var filesToReturn = rootFileStore.Items
                .Where(item => item.Type == "file")
                .ToList();

            var directoriesToReturn = new List<CodeSessionFileStoreItem>();

            var directoriesToProcess = rootFileStore.Items
                .Where(item => item.Type == "directory")
                .Select(x =>
                {
                    x.ParentPath = string.Empty;
                    return x;
                })
                .ToList();

            while (directoriesToProcess.Count > 0)
            {
                var directoryToProcess = directoriesToProcess.First();
                var fileStore = await GetCodeSessionFileStore(
                    httpClient,
                    rootUrl,
                    $"{directoryToProcess.ParentPath}/{directoryToProcess.Name}");

                if (includeFolders)
                    directoriesToReturn.Add(directoryToProcess);
                directoriesToProcess.RemoveAt(0);

                if (fileStore.Items.Count > 0)
                {
                    filesToReturn.AddRange(fileStore.Items.Where(item => item.Type == "file"));
                    directoriesToProcess.AddRange(fileStore.Items.Where(item => item.Type == "directory"));
                }
            }

            var result = includeFolders
                ? [.. filesToReturn, .. directoriesToReturn]
                : filesToReturn;

            return includeLocalPath
                ? [.. result.Select(x =>
                    {
                        x.ParentPath = $"/mnt/data{x.ParentPath}";
                        return x;
                    })]
                : result;
        }

        private async Task<CodeSessionFileStore> GetCodeSessionFileStore(
            HttpClient httpClient,
            string url,
            string path)
        {
            var urlWithPath = $"{url}{(string.IsNullOrWhiteSpace(path) ? string.Empty : $"&path={path}")}";
            var responseMessage = await httpClient.GetAsync(urlWithPath);

            if (!responseMessage.IsSuccessStatusCode)
            {
                return new();
            }
            else
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var fileStore = JsonSerializer.Deserialize<CodeSessionFileStore>(responseContent);
                foreach (var item in fileStore!.Items)
                    item.ParentPath = (string.IsNullOrWhiteSpace(path) && item.Type == "file")
                        ? "/"
                        : path;
                return fileStore!;
            }
        }
    }
}
