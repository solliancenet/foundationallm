using FoundationaLLM.SemanticKernel.Core.Connectors.AzureML.Client;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Services;

namespace FoundationaLLM.SemanticKernel.Core.Connectors.AzureML.Services
{
    /// <summary>
    /// A chat completion service that targets AzureML deployed chat completion models.
    /// </summary>
    public class AzureMLChatCompletionService : IChatCompletionService
    {
        private readonly Dictionary<string, object?> _attributesInternal = [];
        private readonly AzureMLClient _chatCompletionClient;

        ///<inheritdoc/>
        public IReadOnlyDictionary<string, object?> Attributes => this._attributesInternal;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureMLChatCompletionService"/> class.
        /// </summary>
        /// <param name="enpdoint">The endpoint of the AzureML service.</param>
        /// <param name="apiKey">The API key of the AzureML service</param>
        /// <param name="deploymentName">The optional name of the deployment.</param>
        /// <param name="loggerFactory">The factory responsible for creating loggers.</param>
        /// <exception cref="ArgumentException"></exception>
        public AzureMLChatCompletionService(string enpdoint, string apiKey, string? deploymentName = null, ILoggerFactory? loggerFactory = null)
        {
            if (string.IsNullOrWhiteSpace(enpdoint))
                throw new ArgumentException("The endpoint is required.", nameof(enpdoint));
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("The API key is required.", nameof(apiKey));

            _chatCompletionClient = new AzureMLClient(enpdoint, apiKey, deploymentName, loggerFactory?.CreateLogger(this.GetType()));
            if(deploymentName is not null)
                _attributesInternal.Add(AIServiceExtensions.ModelIdKey, deploymentName);            
        }

        ///<inheritdoc/>
        public Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
            => this._chatCompletionClient.GenerateChatMessageAsync(chatHistory, executionSettings, kernel, cancellationToken);

        ///<inheritdoc/>
        public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}
