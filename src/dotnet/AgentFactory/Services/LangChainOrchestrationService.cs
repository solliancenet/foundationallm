using FoundationaLLM.AgentFactory.Core.Models.Orchestration;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration.DataSourceConfigurations;
using FoundationaLLM.AgentFactory.Core.Models.Orchestration.Metadata;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using FoundationaLLM.Common.Interfaces;

namespace FoundationaLLM.AgentFactory.Services
{
    /// <summary>
    /// The LangChain orchestration service.
    /// </summary>
    public class LangChainOrchestrationService : ILangChainOrchestrationService
    {
        readonly LangChainOrchestrationServiceSettings _settings;
        readonly ILogger<LangChainOrchestrationService> _logger;
        private readonly IHttpClientFactoryService _httpClientFactoryService;
        readonly JsonSerializerSettings _jsonSerializerSettings;

        /// <summary>
        /// LangChain Orchestration Service
        /// </summary>
        public LangChainOrchestrationService(
            IOptions<LangChainOrchestrationServiceSettings> options,
            ILogger<LangChainOrchestrationService> logger,
            IHttpClientFactoryService httpClientFactoryService) 
        {
            _settings = options.Value;
            _logger = logger;
            _httpClientFactoryService = httpClientFactoryService;
            _jsonSerializerSettings = CommonJsonSerializerSettings.GetJsonSerializerSettings();
        }

        /// <summary>
        /// Flag indicating whether the orchestration service has been initialized.
        /// </summary>
        public bool IsInitialized => GetServiceStatus();

        /// <summary>
        /// Executes a completion request against the orchestration service.
        /// </summary>
        /// <param name="userPrompt">The user entered prompt.</param>
        /// <param name="messageHistory">List of previous user prompts.</param>
        /// <returns>Returns a completion response from the orchestration engine.</returns>
        public async Task<CompletionResponse> GetCompletion(string userPrompt, List<MessageHistoryItem> messageHistory)
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.LangChainAPI);

            // TODO: This should be created and then populated by calls to the Hub APIs.
            var request = new LLMOrchestrationCompletionRequest()
            {
                UserPrompt = userPrompt,
                Agent = new Agent
                {
                    Name = "anomaly-detector",
                    Type = "anomaly",
                    Description = "Useful for detecting anomalies in a dataset.",
                    PromptTemplate = "You are an anomaly detection agent tasked with inspecting an input product description for anomalies.\n\nYou should attempt to identify difference in the bottle volume and price by comparing the input text to similar records. Flag the record as an anomaly if its data points deviate significantly from these patterns.\nAdditionally, you should provide explanations for why certain data points are flagged as anomalies, allowing for further investigation and potential corrective action.\n\nHere are some statistics you can use to guide your detection of anomalies.\nYou are looking at descriptions of various rum products.\nRum bottles are typically 500ml and 700ml in volume. Volumes like 512ml and 523ml probably represent anomalies and might indicate a data entry error.\nBottle volumes not divisible by 10 are typically anomalies.\nThe price of a bottle of rum typically ranges between $25.00 and $50.00.\n\nYou can use the following tools to help answer questions:"
                },
                DataSource = new SQLDatabaseDataSource
                {
                    Name = "rumdb",
                    Type = "sql",
                    Description = "Azure SQL Database containing rum data.",
                    Configuration = new SQLDatabaseConfiguration
                    {
                        Dialect = "mssql",
                        Host = "cocorahs-ai.database.windows.net",
                        Port = 1433,
                        DatabaseName = "cocorahsdb",
                        Username = "coco-admin",
                        PasswordSecretName = "foundationallm-langchain-sqldb-testdb-database-password",
                        IncludeTables = new List<string> {
                            "RumInventory"
                        },
                        FewShotExampleCount = 2
                    }
                },
                LanguageModel = new LanguageModel
                {
                    Type = LanguageModelTypes.OPENAI,
                    Provider = LanguageModelProviders.MICROSOFT,
                    Temperature = 0f,
                    UseChat = true
                },
                MessageHistory = new List<MessageHistoryItem>()
            };

            var body = JsonConvert.SerializeObject(request, _jsonSerializerSettings);
            var responseMessage = await client.PostAsync("/orchestration/completion",
                new StringContent(
                    body,
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var completionResponse = JsonConvert.DeserializeObject<LLMOrchestrationCompletionResponse>(responseContent);

                return new CompletionResponse
                {
                    Completion = completionResponse.Completion,
                    UserPrompt = completionResponse.UserPrompt,
                    PromptTokens = completionResponse.PromptTokens,
                    CompletionTokens = completionResponse.CompletionTokens,
                    UserPromptEmbedding = null
                };
            }

            return new CompletionResponse
            {
                Completion = "A problem on my side prevented me from responding.",
                UserPrompt = userPrompt,
                PromptTokens = 0,
                CompletionTokens = 0,
                UserPromptEmbedding = Array.Empty<float>()
            };
        }

        /// <summary>
        /// Summarizes the input text.
        /// </summary>
        /// <param name="userPrompt">Text to summarize.</param>
        /// <returns>Returns a summary of the input text.</returns>
        public async Task<string> GetSummary(string userPrompt)
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.LangChainAPI);

            var request = new LLMOrchestrationCompletionRequest()
            {
                UserPrompt = userPrompt,
                Agent = new Agent
                {
                    Name = "summarizer",
                    Type = "summary",
                    Description = "Useful for summarizing input text based on a set of rules.",
                    PromptTemplate = "Write a concise two-word summary of the following:\n\"{text}\"\nCONCISE SUMMARY IN TWO WORDS:"
                },
                LanguageModel = new LanguageModel
                {
                    Type = LanguageModelTypes.OPENAI,
                    Provider = LanguageModelProviders.MICROSOFT,
                    Temperature = 0f,
                    UseChat = true
                }
            };

            var body = JsonConvert.SerializeObject(request, _jsonSerializerSettings);
            var responseMessage = await client.PostAsync("/orchestration/completion",
                new StringContent(
                    body,
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var summaryResponse = JsonConvert.DeserializeObject<LLMOrchestrationCompletionResponse>(responseContent);
                return summaryResponse.Completion;
            }

            return "A problem on my side prevented me from responding.";              
        }

        /// <summary>
        /// Retrieves the status of the orchestration service.
        /// </summary>
        /// <returns>True if the service is ready. Otherwise, returns false.</returns>
        private bool GetServiceStatus()
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.LangChainAPI);
            var responseMessage = client.Send(
                new HttpRequestMessage(HttpMethod.Get, "/status"));

            return responseMessage.Content.ToString() == "ready";
        }
    }
}
