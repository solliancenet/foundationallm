using Azure.AI.OpenAI;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Metadata;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.SemanticKernel.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FoundationaLLM.SemanticKernel.Core.Plugins
{
    public class KnowledgeManagementAgentPlugin : IKnowledgeManagementAgentPlugin
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public KnowledgeManagementAgentPlugin(
            ILogger<KnowledgeManagementAgentPlugin> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <inheritdoc/>
        public async Task<LLMCompletionResponse> GetCompletion(KnowledgeManagementCompletionRequest request)
        {
            var kernel = CreateKernel(request.Agent.LanguageModel!);

            ChatHistory history = [];

            var internalContext = true;
            var messageHistoryEnabled = false;

            if (request.Agent.IndexingProfileObjectIds != null
                && request.Agent.IndexingProfileObjectIds.Count > 0
                && request.Agent.TextEmbeddingProfileObjectId != null)
            {
                internalContext = false;
            }

            if (request.Agent.ConversationHistory != null)
            {
                if (request.Agent.ConversationHistory.Enabled)
                {
                    messageHistoryEnabled = true;
                }
            }

            //var agentPrompt = ResourceProviderService.GetAgentPrompt(request.Agent.Prompt);
            var agentPrompt = string.Empty;
            var context = string.Empty;
            var promptBuilder = $"{context}";

            if (!internalContext)
            {
                promptBuilder = agentPrompt;
                if (messageHistoryEnabled)
                {
                    promptBuilder = $"\n\nQuestion: {request.UserPrompt}\n\nContext: {context}\n\nAnswer:";
                }
            }

            if (messageHistoryEnabled)
                history.AddUserMessage(request.UserPrompt!);

            var modelVersion = _configuration.GetValue<string>(request.Agent.LanguageModel!.Version!);

            var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
            var result = await chatCompletionService.GetChatMessageContentAsync(promptBuilder, new PromptExecutionSettings() { ModelId = modelVersion });
            var usage = result.Metadata!["Usage"] as CompletionsUsage;

            if (messageHistoryEnabled)
                history.AddAssistantMessage(result.Content!);

            return new LLMCompletionResponse()
            {
                Completion = result.Content,
                UserPrompt = request.UserPrompt,
                FullPrompt = promptBuilder,
                PromptTemplate = "\n\nQuestion: {question}\n\nContext: {context}\n\nAnswer:",
                AgentName = request.Agent.Name,
                PromptTokens = usage!.PromptTokens,
                CompletionTokens = usage.CompletionTokens,
                TotalTokens = usage.TotalTokens,
                TotalCost = 0
            };
        }

        private Kernel CreateKernel(LanguageModel llm)
        {
            var deploymentName = _configuration.GetValue<string>(llm.Deployment!);
            var endpoint = _configuration.GetValue<string>(llm.ApiEndpoint!);
            var apiKey = _configuration.GetValue<string>(llm.ApiKey!);

            var builder = Kernel.CreateBuilder();

            if (llm.Provider == LanguageModelProviders.MICROSOFT)
            {
                if (llm.UseChat)
                    builder.AddAzureOpenAIChatCompletion(deploymentName!, endpoint!, apiKey!);
                else
                    builder.AddAzureOpenAITextGeneration(deploymentName!, endpoint!, apiKey!);
            }
            else
            {
                if (llm.UseChat)
                    builder.AddOpenAIChatCompletion(deploymentName!, endpoint!, apiKey!);
                else
                    builder.AddOpenAITextGeneration(deploymentName!, endpoint!, apiKey!);
            }

            return builder.Build();
        }
    }
}
