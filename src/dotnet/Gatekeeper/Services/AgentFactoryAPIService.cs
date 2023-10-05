using FoundationaLLM.Common.Models.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FoundationaLLM.Common;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using FoundationaLLM.Common.Models.Orchestration.SemanticKernel;
using Newtonsoft.Json;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Common.Models.Orchestration;

namespace FoundationaLLM.Gatekeeper.Core.Services
{
    public class AgentFactoryAPIService : IAgentFactoryAPIService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IRefinementService _refinementService;
        readonly JsonSerializerSettings _jsonSerializerSettings;

        public AgentFactoryAPIService(IHttpClientFactory httpClientFactory,
            IRefinementService refinementService)
        {
            _httpClientFactory = httpClientFactory;
            _refinementService = refinementService;
            _jsonSerializerSettings = CommonJsonSerializerSettings.GetJsonSerializerSettings();
        }

        public async Task<CompletionResponseBase> GetCompletion(CompletionRequestBase completionRequest)
        {
            // TODO: Call RefinementService to refine userPrompt
            // await _refinementService.RefineUserPrompt(completionRequest);

            var client = _httpClientFactory.CreateClient(Common.Constants.HttpClients.AgentFactoryAPIClient);

            var responseMessage = await client.PostAsync("orchestration/completion",
            new StringContent(
                    JsonConvert.SerializeObject(completionRequest, _jsonSerializerSettings),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var completionResponse = JsonConvert.DeserializeObject<CompletionResponseBase>(responseContent);

                return completionResponse;
            }

            return new CompletionResponseBase
            {
                Completion = "A problem on my side prevented me from responding.",
                UserPrompt = completionRequest.Prompt,
                UserPromptTokens = 0,
                ResponseTokens = 0,
                UserPromptEmbedding = new float[] { 0 }
            };
        }

        public async Task<string> GetSummary(string content)
        {
            // TODO: Call RefinementService to refine userPrompt
            // await _refinementService.RefineUserPrompt(content);

            var client = _httpClientFactory.CreateClient(Common.Constants.HttpClients.AgentFactoryAPIClient);

            var responseMessage = await client.PostAsync("orchestration/summarize",
                new StringContent(
                    JsonConvert.SerializeObject(new SummarizeRequestBase { Prompt = content }, _jsonSerializerSettings),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var summarizeResponse = JsonConvert.DeserializeObject<SummarizeResponseBase>(responseContent);

                return summarizeResponse?.Info;
            }
            else
                return "A problem on my side prevented me from responding.";
        }
    }
}
