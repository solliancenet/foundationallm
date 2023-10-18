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
using FoundationaLLM.Common.Constants;

namespace FoundationaLLM.AgentFactory.Core.Services;

/// <summary>
/// Class for the Agent Hub API Service
/// </summary>
public class AgentHubAPIService : IAgentHubAPIService
{
    readonly AgentHubSettings _settings;
    readonly ILogger<AgentHubAPIService> _logger;
    private readonly IHttpClientFactoryService _httpClientFactoryService;
    readonly JsonSerializerSettings _jsonSerializerSettings;


    /// <summary>
    /// Constructor for the Agent Hub API Service
    /// </summary>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    /// <param name="httpClientFactoryService"></param>
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

    /// <summary>
    /// Gets the status of the Agent Hub API
    /// </summary>
    /// <returns></returns>
    public async Task<string> Status()
    {
        try
        {
            var client = _httpClientFactoryService.CreateClient(HttpClients.AgentHubAPI);

            var responseMessage = await client.GetAsync("status");

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                return responseContent;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting agent hub status.");
            throw;
        }

        return "Error";
    }

    /// <summary>
    /// Gets a set of agents from the Agent Hub based on the prompt and user context.
    /// </summary>
    /// <param name="userPrompt"></param>
    /// <param name="userContext"></param>
    /// <returns></returns>
    public async Task<AgentHubResponse> ResolveRequest(string userPrompt, string userContext)
    {
        try
        {
            AgentHubRequest ahm = new AgentHubRequest { UserPrompt = userPrompt };

            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.AgentHubAPI);
                        
            var responseMessage = await client.PostAsync("resolve", new StringContent(
                    JsonConvert.SerializeObject(ahm, _jsonSerializerSettings),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var ahr = JsonConvert.DeserializeObject<AgentHubResponse>(responseContent, _jsonSerializerSettings);
                return ahr!;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error resolving request for Agent Hub.");
            throw;
        }

        return new AgentHubResponse();
    }  
}
