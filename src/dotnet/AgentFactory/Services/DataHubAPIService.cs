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
/// Class for the Data Source Hub API Service
/// </summary>
public class DataSourceHubAPIService : IDataSourceHubAPIService
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
            IHttpClientFactoryService httpClientFactoryService)
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
                return responseContent;
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
    /// <param name="sources"></param>
    /// <param name="userContext"></param>
    /// <returns></returns>
    public async Task<DataSourceHubResponse> ResolveRequest(List<string> sources, string userContext)
    {
        try
        {
            DataSourceHubRequest phm = new DataSourceHubRequest { DataSources =  sources };
            var client = _httpClientFactoryService.CreateClient(Common.Constants.HttpClients.DataSourceHubAPI);
            
            var responseMessage = await client.PostAsync("resolve", new StringContent(
                    JsonConvert.SerializeObject(phm, _jsonSerializerSettings),
                    Encoding.UTF8, "application/json"));

            if (responseMessage.IsSuccessStatusCode)
            {
                var responseContent = await responseMessage.Content.ReadAsStringAsync();
                var dshr = JsonConvert.DeserializeObject<DataSourceHubResponse>(responseContent, _jsonSerializerSettings);
                
                return dshr!;
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