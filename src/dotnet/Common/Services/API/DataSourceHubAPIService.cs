using System.Text;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.API;
using FoundationaLLM.Common.Models.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace FoundationaLLM.Common.Services.API;

/// <summary>
/// Class for the Data Source Hub API Service
/// </summary>
public class DataSourceHubAPIService : APIServiceBase, IDataSourceHubAPIService
{
    readonly DataSourceHubSettings _settings;
    readonly ILogger<DataSourceHubAPIService> _logger;
    private readonly IHttpClientFactoryService _httpClientFactoryService;
    readonly JsonSerializerSettings _jsonSerializerSettings;

    /// <summary>
    /// Constructor of the DataSource Hub API Service
    /// </summary>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    /// <param name="httpClientFactoryService"></param>
    public DataSourceHubAPIService(
            IOptions<DataSourceHubSettings> options,
            ILogger<DataSourceHubAPIService> logger,
            IHttpClientFactoryService httpClientFactoryService) :
        base(Common.Constants.HttpClients.DataSourceHubAPI, httpClientFactoryService, logger)
    {
        _settings = options.Value;
        _logger = logger;
        _httpClientFactoryService = httpClientFactoryService;
        _jsonSerializerSettings = Common.Settings.CommonJsonSerializerSettings.GetJsonSerializerSettings();
    }


    /// <summary>
    /// Gets the status of the DataSource Hub API
    /// </summary>
    /// <returns></returns>
    public async Task<string> Status()
    {
        try
        {
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.DataSourceHubAPI);

            var responseMessage = await client.GetAsync("status");

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<string>(responseContent)!;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting data hub status.");
            throw;
        }


        return "Error";
    }

    /// <summary>
    /// Gets a list of DataSources from the DataSource Hub
    /// </summary>
    /// <param name="sources">The data sources to resolve.</param>
    /// <param name="sessionId">The session ID.</param>
    /// <returns></returns>
    public async Task<DataSourceHubResponse> ResolveRequest(List<string> sources, string sessionId)
    {
        try
        {
            var request = new DataSourceHubRequest { DataSources =  sources, SessionId = sessionId };
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.DataSourceHubAPI);
            
            var responseMessage = await client.PostAsync("resolve", new StringContent(
                    JsonConvert.SerializeObject(request, _jsonSerializerSettings),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var response = JsonConvert.DeserializeObject<DataSourceHubResponse>(responseContent, _jsonSerializerSettings);
                
                return response!;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error resolving request for data source hub.");
            throw;
        }

        return new DataSourceHubResponse();
    }

}
