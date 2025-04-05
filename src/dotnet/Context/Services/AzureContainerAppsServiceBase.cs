using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.CodeExecution;
using FoundationaLLM.Context.Exceptions;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace FoundationaLLM.Context.Services
{
    /// <summary>
    /// Base class for Azure Container Apps Dynamic Sessions services.
    /// </summary>
    /// <param name="httpClientFactory">The factory used to create <see cref="HttpClient"/> instances.</param>
    /// <param name="logger">The logger used for logging.</param>
    public class AzureContainerAppsServiceBase(
        IHttpClientFactory httpClientFactory,
        ILogger logger)
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        protected readonly ILogger _logger = logger;

        protected Task<CreateCodeSessionResponse> CreateCodeSessionInternal(
            string instanceId,
            string agentName,
            string conversationId,
            string context,
            string sessionEndpoint,
            UnifiedUserIdentity userIdentity)
        {
            ContextServiceException.ThrowIfNullOrWhiteSpace(instanceId, nameof(instanceId));
            ContextServiceException.ThrowIfNullOrWhiteSpace(agentName, nameof(agentName));
            ContextServiceException.ThrowIfNullOrWhiteSpace(conversationId, nameof(conversationId));
            ContextServiceException.ThrowIfNullOrWhiteSpace(context, nameof(context));
            ContextServiceException.ThrowIfNullOrWhiteSpace(userIdentity?.UPN, nameof(userIdentity));

            var newSessionId = $"code-{conversationId}-{context}";

            // Ensure the session identifier is no longer than 128 characters.
            if (newSessionId.Length > 128)
            {
                _logger.LogWarning("The generated code execution session identifier is longer than 128 characters. It will be truncated.");
                newSessionId = newSessionId[..128];
            }

            return Task.FromResult(new CreateCodeSessionResponse
            {
                SessionId = newSessionId,
                Endpoint = sessionEndpoint
            });
        }

        protected async Task<HttpClient> CreateHttpClient()
        {
            var httpClient = _httpClientFactory.CreateClient();

            var credentials = ServiceContext.AzureCredential;
            var tokenResult = await credentials!.GetTokenAsync(
                new(["https://dynamicsessions.io/.default"]),
                default);

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResult.Token);

            return httpClient;
        }
    }
}
