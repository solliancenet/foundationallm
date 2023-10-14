using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;
using FoundationaLLM.AgentFactory.Models.Orchestration;
using FoundationaLLM.AgentFactory.Services;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Settings;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http;
using System.Runtime;
using System.Text;
using FoundationaLLM.Common.Interfaces;

namespace FoundationaLLM.AgentFactory.Core.Services;

public class AgentHubAPIService : IAgentHubService
{
    readonly AgentHubSettings _settings;
    readonly ILogger<AgentHubAPIService> _logger;
    private readonly IHttpClientFactoryService _httpClientFactoryService;
    readonly JsonSerializerSettings _jsonSerializerSettings;

    public AgentHubAPIService(
            IOptions<AgentHubSettings> options,
            ILogger<AgentHubAPIService> logger,
            IHttpClientFactoryService httpClientFactoryService)
    {
        _settings = options.Value;
        _logger = logger;
        _httpClientFactoryService = httpClientFactoryService;
        _jsonSerializerSettings = Common.Settings.CommonJsonSerializerSettings.GetJsonSerializerSettings();
    }

    public async Task<string> Status()
    {
        try
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.AgentHubAPI);

            var responseMessage = await client.GetAsync("/status");

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                return responseContent;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting agent hub status.");
            throw ex;
        }

        return null;
    }

    public async Task<AgentHubResponse> ResolveRequest(string userPrompt, string userContext)
    {
        try
        {
            AgentHubMessage ahm = new AgentHubMessage { AgentName = "weather" };
            
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.AgentHubAPI);
                        
            var responseMessage = await client.PostAsync("/resolve_request", new StringContent(
                    JsonConvert.SerializeObject(ahm, _jsonSerializerSettings),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var ahr = JsonConvert.DeserializeObject<AgentHubResponse>(responseContent, _jsonSerializerSettings);
                
                /*
                dynamic obj = JsonConvert.DeserializeObject(responseContent);
                var agents = obj.agents;

                AgentHubResponse ahr = new AgentHubResponse();
                ahr.Agents = new List<AgentMetadata>();

                foreach ( var agent in agents)
                {
                    LanguageModelMetadata lmm = new LanguageModelMetadata {  ModelType = agent.language_model.model_type, Provider = agent.language_model.provider, Temperature = agent.language_model.temperature, UseChat = agent.language_model.use_chat };
                    AgentMetadata am = new AgentMetadata { Name = agent.name, AllowedDataSourceNames = agent.allowed_data_source_names.ToObject<List<string>>(), Description = agent.description, LanguageModel = lmm };
                    ahr.Agents.Add(am);
                }
                */
                

                return ahr;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error resolving request for Agent Hub.");
            throw ex;
        }

        return new AgentHubResponse();
    }    
}
