using System.Text;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.API;
using FoundationaLLM.Common.Models.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace FoundationaLLM.Common.Services.API;

/// <summary>
/// Class for the PromptHub API Service
/// </summary>
public class PromptHubAPIService : APIServiceBase, IPromptHubAPIService
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
            IHttpClientFactoryService httpClientFactoryService) :
        base(Common.Constants.HttpClients.PromptHubAPI, httpClientFactoryService, logger)
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
                return JsonConvert.DeserializeObject<string>(responseContent)!;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting prompt hub status.");
            throw;
        }

        return "Error";
    }


    /// <inheritdoc/>
    public async Task<PromptHubResponse> ResolveRequest(string promptContainer, string sessionId, string promptName = "default")
    {
        try
        {
            var request = new PromptHubRequest { PromptContainer = promptContainer, PromptName = promptName, SessionId = sessionId };
            
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.PromptHubAPI);
            var body = JsonConvert.SerializeObject(request, _jsonSerializerSettings);
            var responseMessage = await client.PostAsync("resolve", new StringContent(
                    body,
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<PromptHubResponse>(responseContent, _jsonSerializerSettings);
                return response!;
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
