﻿using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.CodeExecution;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.Models.Context.Knowledge;
using FoundationaLLM.Common.Models.ResourceProviders.Context;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FoundationaLLM.Common.Clients
{
    /// <summary>
    /// Provides methods to call the FoundationaLLM Context API service.
    /// </summary>
    public class ContextServiceClient(
        IOrchestrationContext callContext,
        IHttpClientFactoryService httpClientFactoryService,
        ILogger<ContextServiceClient> logger) : IContextServiceClient
    {
        private readonly IOrchestrationContext _callContext = callContext;
        private readonly IHttpClientFactoryService _httpClientFactoryService = httpClientFactoryService;
        private readonly ILogger<ContextServiceClient> _logger = logger;

        /// <inheritdoc/>
        public async Task<ContextServiceResponse<ContextFileContent>> GetFileContent(
            string instanceId,
            string fileId)
        {
            try
            {
                var client = await _httpClientFactoryService.CreateClient(
                    HttpClientNames.ContextAPI,
                    _callContext.CurrentUserIdentity!);

                var responseMessage = await client.GetAsync($"instances/{instanceId}/files/{fileId}");

                if (responseMessage.IsSuccessStatusCode)
                {
                    return new ContextServiceResponse<ContextFileContent>
                    {
                        Success = true,
                        Result = new ContextFileContent
                        {
                            FileContent = await responseMessage.Content.ReadAsStreamAsync(),
                            FileName = responseMessage!.Content.Headers.ContentDisposition!.FileName!,
                            ContentType = responseMessage!.Content.Headers.ContentType!.MediaType!
                        }
                    };
                }

                _logger.LogError(
                    "An error occurred while retrieving the file content. Status code: {StatusCode}.",
                    responseMessage.StatusCode);

                return new ContextServiceResponse<ContextFileContent>
                {
                    Success = false,
                    ErrorMessage = "The service responded with an error status code."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the file content.");
                return new ContextServiceResponse<ContextFileContent>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while retrieving the file content."
                };
            }
        }

        /// <inheritdoc/>
        public async Task<ContextServiceResponse<ContextFileRecord>> GetFileRecord(
            string instanceId,
            string fileId)
        {
            try
            {
                var client = await _httpClientFactoryService.CreateClient(
                    HttpClientNames.ContextAPI,
                    _callContext.CurrentUserIdentity!);

                var responseMessage = await client.GetAsync($"instances/{instanceId}/fileRecords/{fileId}");

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<ContextFileRecord>(responseContent);

                    return response == null
                        ? new ContextServiceResponse<ContextFileRecord>
                        {
                            Success = false,
                            ErrorMessage = $"An error occurred deserializing the response from the service for file {fileId}."
                        }
                        : new ContextServiceResponse<ContextFileRecord>
                        {
                            Success = true,
                            Result = response
                        };
                }

                _logger.LogError(
                    "An error occurred while retrieving the file record for file {FileId}. Status code: {StatusCode}.",
                    fileId, responseMessage.StatusCode);

                return new ContextServiceResponse<ContextFileRecord>
                {
                    Success = false,
                    ErrorMessage = $"The service responded with an error status code for file {fileId}."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the file record for file {FileId}.", fileId);
                return new ContextServiceResponse<ContextFileRecord>
                {
                    Success = false,
                    ErrorMessage = $"An error occurred while retrieving the file record for file {fileId}."
                };
            }
        }

        /// <inheritdoc/>
        public async Task<ContextServiceResponse<ContextFileRecord>> CreateFile(
            string instanceId,
            string conversationId,
            string fileName,
            string fileContentType,
            Stream fileContent)
        {
            try
            {
                var client = await _httpClientFactoryService.CreateClient(
                    HttpClientNames.ContextAPI,
                    _callContext.CurrentUserIdentity!);

                var multipartFormDataContent = new MultipartFormDataContent();
                var streamContent = new StreamContent(fileContent);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(fileContentType);
                streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = "file",
                    FileName = fileName
                };
                multipartFormDataContent.Add(streamContent);

                var responseMessage = await client.PostAsync(
                    $"instances/{instanceId}/conversations/{conversationId}/files",
                    multipartFormDataContent);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<ContextFileRecord>(responseContent);

                    return response == null
                        ? new ContextServiceResponse<ContextFileRecord>
                        {
                            Success = false,
                            ErrorMessage = "An error occurred deserializing the response from the service."
                        }
                        : new ContextServiceResponse<ContextFileRecord>
                        {
                            Success = true,
                            Result = response
                        };
                }

                _logger.LogError(
                    "An error occurred while creating a file. Status code: {StatusCode}.",
                    responseMessage.StatusCode);

                return new ContextServiceResponse<ContextFileRecord>
                {
                    Success = false,
                    ErrorMessage = "The service responded with an error status code."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a file.");
                return new ContextServiceResponse<ContextFileRecord>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while creating a file."
                };
            }
        }

        /// <inheritdoc/>
        public async Task<ContextServiceResponse<CreateCodeSessionResponse>> CreateCodeSession(
            string instanceId,
            string agentName,
            string conversationId,
            string context,
            string endpointProvider,
            string language)
        {
            try
            {
                var client = await _httpClientFactoryService.CreateClient(
                    HttpClientNames.ContextAPI,
                    _callContext.CurrentUserIdentity!);

                var responseMessage = await client.PostAsync(
                    $"instances/{instanceId}/codeSessions",
                    JsonContent.Create(
                        new CreateCodeSessionRequest
                        {
                            AgentName = agentName,
                            ConversationId = conversationId,
                            Context = context,
                            EndpointProvider = endpointProvider,
                            Language = language
                        }));

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<CreateCodeSessionResponse>(responseContent);

                    return response == null
                        ? new ContextServiceResponse<CreateCodeSessionResponse>
                        {
                            Success = false,
                            ErrorMessage = "An error occurred deserializing the response from the service."
                        }
                        : new ContextServiceResponse<CreateCodeSessionResponse>
                        {
                            Success = true,
                            Result = response
                        };
                }

                _logger.LogError(
                    "An error occurred while creating a code session. Status code: {StatusCode}.",
                    responseMessage.StatusCode);

                return new ContextServiceResponse<CreateCodeSessionResponse>
                {
                    Success = false,
                    ErrorMessage = "The service responded with an error status code."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a code session.");
                return new ContextServiceResponse<CreateCodeSessionResponse>
                {
                    Success = false,
                    ErrorMessage = "An error occurred while creating a code session."
                };
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<KnowledgeSource>> GetKnowledgeSources(
            string instanceId,
            IEnumerable<string>? knowledgeSourceNames = null)
        {
            try
            {
                var client = await _httpClientFactoryService.CreateClient(
                    HttpClientNames.ContextAPI,
                    _callContext.CurrentUserIdentity!);
                var responseMessage = await client.PostAsJsonAsync(
                    $"instances/{instanceId}/knowledgeSources/list",
                    new ContextKnowledgeSourceListRequest
                    {
                        KnowledgeSourceNames = knowledgeSourceNames?.ToList()
                    });
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<IEnumerable<KnowledgeSource>>(responseContent);
                    return response ?? [];
                }
                _logger.LogError(
                    "An error occurred while retrieving the knowledge sources. Status code: {StatusCode}.",
                    responseMessage.StatusCode);
                return [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the knowledge sources.");
                return [];
            }
        }

        /// <inheritdoc/>
        public async Task<ContextServiceResponse> UpdateKnowledgeSource(
            string instanceId,
            string knowledgeSourceId,
            ContextKnowledgeSourceUpdateRequest updateRequest)
        {
            try
            {
                var client = await _httpClientFactoryService.CreateClient(
                    HttpClientNames.ContextAPI,
                    _callContext.CurrentUserIdentity!);

                var responseMessage = await client.PostAsJsonAsync(
                    $"instances/{instanceId}/knowledgeSources/{knowledgeSourceId}",
                    updateRequest);

                if (responseMessage.IsSuccessStatusCode)
                    return new ContextServiceResponse { Success = true };

                _logger.LogError(
                    "An error occurred while updating the knowledge source. Status code: {StatusCode}.",
                    responseMessage.StatusCode);

                return new ContextServiceResponse
                {
                    Success = false,
                    ErrorMessage = "The service responded with an error status code."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the knowledge source.");

                return new ContextServiceResponse
                {
                    Success = false,
                    ErrorMessage = "An error occurred while updating the knowledge source."
                };
            }
        }

        /// <inheritdoc/>
        public async Task<ContextKnowledgeSourceQueryResponse> QueryKnowledgeSource(
            string instanceId,
            string knowledgeSourceId,
            ContextKnowledgeSourceQueryRequest queryRequest)
        {
            try
            {
                var client = await _httpClientFactoryService.CreateClient(
                    HttpClientNames.ContextAPI,
                    _callContext.CurrentUserIdentity!);

                var responseMessage = await client.PostAsJsonAsync(
                    $"instances/{instanceId}/knowledgeSources/{knowledgeSourceId}/query",
                    queryRequest);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<ContextKnowledgeSourceQueryResponse>(responseContent);
                    return response!;
                }

                _logger.LogError(
                    "An error occurred while querying the knowledge source. Status code: {StatusCode}.",
                    responseMessage.StatusCode);

                return new ContextKnowledgeSourceQueryResponse
                {
                    Success = false,
                    ErrorMessage = "The service responded with an error status code."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while querying the knowledge source.");

                return new ContextKnowledgeSourceQueryResponse
                {
                    Success = false,
                    ErrorMessage = "An error occurred while querying the knowledge source."
                };
            }
        }

        /// <inheritdoc/>
        public async Task<ContextKnowledgeSourceRenderGraphResponse> RenderKnowledgeSourceGraph(
            string instanceId,
            string knowledgeSourceId,
            ContextKnowledgeSourceQueryRequest? queryRequest)
        {
            try
            {
                var client = await _httpClientFactoryService.CreateClient(
                    HttpClientNames.ContextAPI,
                    _callContext.CurrentUserIdentity!);

                var responseMessage = await client.PostAsJsonAsync(
                    $"instances/{instanceId}/knowledgeSources/{knowledgeSourceId}/render-graph",
                    queryRequest);

                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<ContextKnowledgeSourceRenderGraphResponse>(responseContent);
                    return response!;
                }

                _logger.LogError(
                    "An error occurred while rendering the knowledge source's graph. Status code: {StatusCode}.",
                    responseMessage.StatusCode);

                return new ContextKnowledgeSourceRenderGraphResponse
                {
                    Success = false,
                    ErrorMessage = "The service responded with an error status code."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while rendering the knowledge source's graph.");

                return new ContextKnowledgeSourceRenderGraphResponse
                {
                    Success = false,
                    ErrorMessage = "An error occurred while rendering the knowledge source's graph."
                };
            }
        }
    }
}
