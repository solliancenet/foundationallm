using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.AgentFactory.Core.Models.Messages;
using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using FoundationaLLM.Common.Interfaces;

namespace FoundationaLLM.AgentFactory.Core.Services;

/// <summary>
/// Class for the PromptHub API Service
/// </summary>
public class PromptHubAPIService : IPromptHubAPIService
{
    readonly PromptHubSettings _settings;
    readonly ILogger<PromptHubAPIService> _logger;
    private readonly IHttpClientFactoryService _httpClientFactoryService;
    readonly JsonSerializerSettings _jsonSerializerSettings;

    /// <summary>
    /// Constructor for the PromptHub API Service
    /// </summary>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    /// <param name="httpClientFactoryService"></param>
    public PromptHubAPIService(
            IOptions<PromptHubSettings> options,
            ILogger<PromptHubAPIService> logger,
            IHttpClientFactoryService httpClientFactoryService)
    {
        _settings = options.Value;
        _logger = logger;
        _httpClientFactoryService = httpClientFactoryService;
        _jsonSerializerSettings = Common.Settings.CommonJsonSerializerSettings.GetJsonSerializerSettings();
    }


    /// <summary>
    /// Gets the status of the Prompt Hub API
    /// </summary>
    /// <returns></returns>
    public async Task<string> Status()
    {
        try
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.PromptHubAPI);

            var responseMessage = await client.GetAsync("status");

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                return responseContent;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting prompt hub status.");
            throw;
        }

        return "Error";
    }


    /// <summary>
    /// Used to get prompts for a target agent and user context.
    /// </summary>
    /// <param name="agentName"></param>
    /// <param name="userContext"></param>
    /// <returns></returns>
    public async Task<PromptHubResponse> ResolveRequest(string agentName, string userContext)
    {
        try
        {
            PromptHubRequest phm = new PromptHubRequest { AgentName = agentName };
            
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.PromptHubAPI);
            var body = JsonConvert.SerializeObject(phm, _jsonSerializerSettings);
            var responseMessage = await client.PostAsync("resolve", new StringContent(
                    body,
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var phr = JsonConvert.DeserializeObject<PromptHubResponse>(responseContent, _jsonSerializerSettings);
                return phr!;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error resolving request for prompt Hub.");
            throw;
        }

        return new PromptHubResponse();
    }    
}
