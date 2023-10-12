using FoundationaLLM.AgentFactory.Core.Models.Orchestration;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;
using FoundationaLLM.Common.Models.Chat;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace FoundationaLLM.AgentFactory.Services
{
    public class LangChainOrchestrationService : ILangChainOrchestrationService
    {
        readonly LangChainOrchestrationServiceSettings _settings;
        readonly ILogger<LangChainOrchestrationService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        readonly JsonSerializerSettings _jsonSerializerSettings;

        public LangChainOrchestrationService(
            IOptions<LangChainOrchestrationServiceSettings> options,
            ILogger<LangChainOrchestrationService> logger,
            IHttpClientFactory httpClientFactory) 
        {
            _settings = options.Value;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _jsonSerializerSettings = CommonJsonSerializerSettings.GetJsonSerializerSettings();
        }

        public bool IsInitialized => GetServiceStatus();

        public async Task<CompletionResponse> GetResponse(string userPrompt, List<MessageHistoryItem> messageHistory)
        {
            var client = _httpClientFactory.CreateClient(Common.Constants.HttpClients.LangChainAPIClient);

            var request = new LangChainCompletionRequest()
            {
                user_prompt = userPrompt,
                agent = new LangChainAgent
                {
                    name = "anomaly-detector",
                    type = "anomaly",
                    description = "Useful for detecting anomalies in a dataset.",
                    prompt_template = "You are an anomaly detection agent tasked with inspecting an input product description for anomalies.\n\nYou should attempt to identify difference in the bottle volume and price by comparing the input text to similar records. Flag the record as an anomaly if its data points deviate significantly from these patterns.\nAdditionally, you should provide explanations for why certain data points are flagged as anomalies, allowing for further investigation and potential corrective action.\n\nHere are some statistics you can use to guide your detection of anomalies.\nYou are looking at descriptions of various rum products.\nRum bottles are typically 500ml and 700ml in volume. Volumes like 512ml and 523ml probably represent anomalies and might indicate a data entry error.\nBottle volumes not divisible by 10 are typically anomalies.\nThe price of a bottle of rum typically ranges between $25.00 and $50.00.\n\nYou can use the following tools to help answer questions:",
                    language_model = new LangChainLanguageModel
                    {
                        type = "openai",
                        subtype = "chat",
                        provider = "azure",
                        temperature = 0f
                    }
                },
                data_source = new LangChainDataSource
                {
                    name = "rumdb",
                    type = "sql",
                    description = "Azure SQL Database containing rum data.",
                    configuration = new LangChainSQLDataSourceConfiguration
                    {
                        dialect = "mssql",
                        host = "cocorahs-ai.database.windows.net",
                        port = 1433,
                        database_name = "cocorahsdb",
                        username = "coco-admin",
                        password_secret_name = "foundationallm-langchain-sqldb-testdb-database-password",
                        include_tables = new string[] {
                            "RumInventory"
                        },
                        few_shot_example_count = 2
                    }
                },
                message_history = new List<MessageHistoryItem>()
            };
            var body = JsonConvert.SerializeObject(request, _jsonSerializerSettings);
            var responseMessage = await client.PostAsync("/orchestration/completion",
                new StringContent(
                    body,
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var completionResponse = JsonConvert.DeserializeObject<LangChainCompletionResponse>(responseContent);

                return new CompletionResponse
                {
                    Completion = completionResponse.completion,
                    UserPrompt = userPrompt,
                    UserPromptTokens = 0,
                    ResponseTokens = 0,
                    UserPromptEmbedding = new float[0]
                };
            }

            return new CompletionResponse
            {
                Completion = "A problem on my side prevented me from responding.",
                UserPrompt = userPrompt,
                UserPromptTokens = 0,
                ResponseTokens = 0,
                UserPromptEmbedding = new float[0]
            };
        }

        public async Task<string> GetSummary(string content)
        {
            throw new NotImplementedException();
        }

        private bool GetServiceStatus()
        {
            var client = _httpClientFactory.CreateClient(Common.Constants.HttpClients.LangChainAPIClient);
            var responseMessage = client.Send(
                new HttpRequestMessage(HttpMethod.Get, "/status"));

            return responseMessage.Content.ToString() == "ready";
        }
    }
}
