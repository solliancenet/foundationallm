using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Core.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Core.Services
{
    public class GatekeeperAPIService : IGatekeeperAPIService
    {
        private readonly IHttpClientFactoryService _httpClientFactoryService;
        readonly JsonSerializerSettings _jsonSerializerSettings;

        public GatekeeperAPIService(IHttpClientFactoryService httpClientFactoryService, IUserIdentityContext userIdentityContext)
        {
            _httpClientFactoryService = httpClientFactoryService;
            _jsonSerializerSettings = CommonJsonSerializerSettings.GetJsonSerializerSettings();
        }

        public async Task<CompletionResponse> GetCompletion(CompletionRequest completionRequest)
        {
            // TODO: Call RefinementService to refine userPrompt
            // await _refinementService.RefineUserPrompt(completionRequest);

            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.GatekeeperAPI);
                       
            var responseMessage = await client.PostAsync("orchestration/completion",
            new StringContent(
                    JsonConvert.SerializeObject(completionRequest, _jsonSerializerSettings),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var completionResponse = JsonConvert.DeserializeObject<CompletionResponse>(responseContent);

                return completionResponse;
            }

            return new CompletionResponse
            {
                Completion = "A problem on my side prevented me from responding.",
                UserPrompt = completionRequest.UserPrompt,
                PromptTokens = 0,
                CompletionTokens = 0,
                UserPromptEmbedding = new float[] { 0 }
            };
        }

        public async Task<string> GetSummary(string content)
        {
            // TODO: Call RefinementService to refine userPrompt
            // await _refinementService.RefineUserPrompt(content);

            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.GatekeeperAPI);

            var responseMessage = await client.PostAsync("orchestration/summarize",
                new StringContent(
                    JsonConvert.SerializeObject(new SummaryRequest { Prompt = content }, _jsonSerializerSettings),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var summarizeResponse = JsonConvert.DeserializeObject<SummaryResponse>(responseContent);

                return summarizeResponse?.Info;
            }
            else
                return "A problem on my side prevented me from responding.";
        }

        public async Task<bool> SetLLMOrchestrationPreference(string orchestrationService)
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.GatekeeperAPI);

            var responseMessage = await client.PostAsync("orchestration/preference",
                new StringContent(orchestrationService));

            if (responseMessage.IsSuccessStatusCode)
            {
                // The response value should be a boolean indicating whether the orchestration service was set successfully.
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var orchestrationServiceSet = JsonConvert.DeserializeObject<bool>(responseContent);
                return orchestrationServiceSet;
            }
            
            return false;
        }

        public Task AddMemory(object item, string itemName, Action<object, float[]> vectorizer)
        {
            throw new NotImplementedException();
        }

        public Task RemoveMemory(object item)
        {
            throw new NotImplementedException();
        }
    }
}
