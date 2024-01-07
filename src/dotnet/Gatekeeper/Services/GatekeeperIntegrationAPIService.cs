using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Models.Integration;
using Newtonsoft.Json;
using System.Text;

namespace FoundationaLLM.Gatekeeper.Core.Services
{
    /// <summary>
    /// Contains methods for interacting with the Gatekeeper API.
    /// </summary>
    public class GatekeeperIntegrationAPIService : IGatekeeperIntegrationAPIService
    {
        private readonly IHttpClientFactoryService _httpClientFactoryService;
        readonly JsonSerializerSettings _jsonSerializerSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="GatekeeperIntegrationAPIService"/> class.
        /// </summary>
        /// <param name="httpClientFactoryService">The <see cref="IHttpClientFactoryService"/>
        /// used to retrieve an <see cref="HttpClient"/> instance that contains required
        /// headers for Gatekeeper Integration API requests.</param>
        public GatekeeperIntegrationAPIService(IHttpClientFactoryService httpClientFactoryService)
        {
            _httpClientFactoryService = httpClientFactoryService;
            _jsonSerializerSettings = CommonJsonSerializerSettings.GetJsonSerializerSettings();
        }

        /// <inheritdoc/>
        public async Task<List<PIIResult>> AnalyzeText(string text)
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.GatekeeperIntegrationAPI);

            var content = JsonConvert.SerializeObject(new AnalyzeRequest() { Content = text, Anonymize = false, Language = "en" });

            var responseMessage = await client.PostAsync("analyze", new StringContent(content, Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var analysisResults = JsonConvert.DeserializeObject<AnalyzeResponse>(responseContent);

                return analysisResults!.Results;
            }
            else
                return [];
        }

        /// <inheritdoc/>
        public async Task<string> AnonymizeText(string text)
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.GatekeeperIntegrationAPI);

            var content = JsonConvert.SerializeObject(new AnalyzeRequest() { Content = text, Anonymize = true, Language = "en" });

            var responseMessage = await client.PostAsync("analyze", new StringContent(content, Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var analysisResults = JsonConvert.DeserializeObject<AnonymizeResponse>(responseContent);

                return analysisResults!.Content;
            }
            else
                return "A problem on my side prevented me from responding.";
        }
    }
}
