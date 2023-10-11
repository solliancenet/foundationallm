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
                    prompt_template = "You are an agent designed to detect anomalies. Your job is analyze the '{input} record for possible anomalies in the values of its Price and Bottle Volume based on its Type.\n\nExecute the following queries using query_pandas_dataframe to retrieve dataset statistics to use for comparison:\n  - 'Describe the statistics for rums of the same Type as the input value and return the 25% and 75% values as the typical price range.'\n  - 'Get the most frequently occurring bottle volumes as the typical values.'\n  - Use bottle volumes that are not typical as examples of possible anomalies.\n\nAlso, convert the price and bottle volume values from the input record and compare them to each other. Flag the record as a probably anomaly due to a typo if those values are equal plus plus or minus 1 after being converted to integer values.\n\nReport any anomalies flagged.\n\nYour response must include explanations about why the record was flagged as an anomaly, the typical values, and recommendations for remediation steps.\n\nYou have access to the following tools:",
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
                        dialect = "msssql",
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
