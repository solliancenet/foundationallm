using System.Text;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.API;
using FoundationaLLM.Common.Models.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace FoundationaLLM.Common.Services.API;

/// <summary>
/// Class for the Agent Hub API Service.
/// </summary>
public class AgentHubAPIService : APIServiceBase, IAgentHubAPIService
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
            IHttpClientFactoryService httpClientFactoryService) :
        base(HttpClients.AgentHubAPI, httpClientFactoryService, logger)
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
                return JsonConvert.DeserializeObject<string>(responseContent)!;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting agent hub status.");
            throw;
        }

        return "Error";
    }

    /// <inheritdoc/>
    public async Task<AgentHubResponse> ResolveRequest(string userPrompt, string sessionId,
        string? agentHintOverride = null)
    {
        try
        {
            var request = new AgentHubRequest { UserPrompt = userPrompt, SessionId = sessionId };

            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.AgentHubAPI);

            if (!string.IsNullOrWhiteSpace(agentHintOverride))
            {
                var agentHint = JsonConvert.SerializeObject(
                    new FoundationaLLM.Common.Models.Metadata.Agent { Name = agentHintOverride },
                    _jsonSerializerSettings);
                client.DefaultRequestHeaders.Add(HttpHeaders.AgentHint, agentHint);
            }
                        
            var responseMessage = await client.PostAsync("resolve", new StringContent(
                    JsonConvert.SerializeObject(request, _jsonSerializerSettings),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<AgentHubResponse>(responseContent, _jsonSerializerSettings);
                return response!;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error resolving request for Agent Hub.");
            throw;
        }

        return new AgentHubResponse();
    }

    /// <inheritdoc/>
    public async Task<List<AgentMetadata>> ListAgents()
    {
        try
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.AgentHubAPI);

            var responseMessage = await client.GetAsync("list");

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<List<AgentMetadata>>(responseContent, _jsonSerializerSettings);
                return response!;
            }

            throw new Exception($"The Agent Hub API call returned with status {responseMessage.StatusCode}.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving list of agents from Agent Hub.");
            throw;
        }
    }
}
