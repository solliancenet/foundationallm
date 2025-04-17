using FoundationaLLM.Common.Constants.Context;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Context.Interfaces;
using FoundationaLLM.Context.Models.Configuration;
using Microsoft.Extensions.Logging;

namespace FoundationaLLM.Context.Services
{
    /// <summary>
    /// Provides the implementation for the FoundationaLLM File service.
    /// </summary>
    /// <param name="cosmosDBService">The Azure Cosmos DB service providing database services.</param>
    /// <param name="storageService">The <see cref="IStorageService"/> providing storage services.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class FileService(
        IAzureCosmosDBFileService cosmosDBService,
        IStorageService storageService,
        FileServiceSettings settings,
        ILogger<FileService> logger) : IFileService
    {
        private readonly IAzureCosmosDBFileService _cosmosDBService = cosmosDBService;
        private readonly IStorageService _storageService = storageService;
        private readonly FileServiceSettings _settings = settings;
        private readonly HashSet<string> _knowledgeSearchFileTypes = [.. settings
            .KnowledgeSearchFileExtensions
            .Split(",")
            .Select(s => s.Trim().ToLower())];
        private readonly ILogger<FileService> _logger = logger;

        /// <inheritdoc/>
        public async Task<ContextFileRecord> CreateFile(
            string instanceId,
            string origin,
            string conversationId,
            string fileName,
            string contentType,
            Stream content,
            UnifiedUserIdentity userIdentity,
            Dictionary<string, string>? metadata)
        {
            var fileRecord = new ContextFileRecord(
                instanceId,
                origin,
                conversationId,
                fileName,
                contentType,
                content.Length,
                origin switch
                {
                    ContextRecordOrigins.CodeSession => FileProcessingTypes.None,
                    ContextRecordOrigins.UserUpload => _knowledgeSearchFileTypes
                        .Contains(Path.GetExtension(fileName).Replace(".", string.Empty).ToLower())
                            ? FileProcessingTypes.ConversationDataPipeline
                            : FileProcessingTypes.None,
                    _ => FileProcessingTypes.None
                },
                userIdentity,
                metadata);

            await _cosmosDBService.UpsertFileRecord(fileRecord);

            await _storageService.WriteFileAsync(
                instanceId,
                fileRecord.FilePath,
                content,
                contentType,
                CancellationToken.None);

            return fileRecord;
        }

        /// <inheritdoc/>
        public async Task<ContextFileContent?> GetFileContent(
            string instanceId,
            string conversationId,
            string fileName,
            UnifiedUserIdentity userIdentity)
        {
            var fileRecords = await _cosmosDBService.GetFileRecords(
                instanceId,
                conversationId,
                fileName,
                userIdentity.UPN!);

            if (fileRecords.Count == 0)
                return null;

            var fileRecord = fileRecords.First();

            var fileContent = await _storageService.ReadFileAsync(
                instanceId,
                fileRecord.FilePath,
                default);

            return new ContextFileContent
            {
                FileName = fileRecord.FileName,
                ContentType = fileRecord.ContentType,
                FileContent = fileContent.ToStream()
            };
        }

        /// <inheritdoc/>
        public async Task<ContextFileContent?> GetFileContent(
            string instanceId,
            string fileId,
            UnifiedUserIdentity userIdentity)
        {
            var fileRecord = await _cosmosDBService.GetFileRecord(
                instanceId,
                fileId,
                userIdentity.UPN!);

            var fileContent = await _storageService.ReadFileAsync(
                instanceId,
                fileRecord.FilePath,
                default);

            return new ContextFileContent
            {
                FileName = fileRecord.FileName,
                ContentType = fileRecord.ContentType,
                FileContent = fileContent.ToStream()
            };
        }

        /// <inheritdoc/>
        public async Task<ContextFileRecord?> GetFileRecord(
            string instanceId,
            string fileId,
            UnifiedUserIdentity userIdentity)
        {
            var fileRecord = await _cosmosDBService.GetFileRecord(
                instanceId,
                fileId,
                userIdentity.UPN!);

            return fileRecord;
        }
    }
}
