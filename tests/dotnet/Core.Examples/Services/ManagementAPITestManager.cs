using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Core.Examples.Catalogs;
using FoundationaLLM.Core.Examples.Exceptions;
using FoundationaLLM.Core.Examples.Interfaces;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Core.Examples.Setup;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using FoundationaLLM.Core.Examples.Resources;

namespace FoundationaLLM.Core.Examples.Services
{
    /// <inheritdoc/>
    public class ManagementAPITestManager(
        IHttpClientManager httpClientManager,
        IOptions<InstanceSettings> instanceSettings) : IManagementAPITestManager
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();

        public async Task CreateAppConfiguration(AppConfigurationKeyValue appConfigurationKeyValue)
        {
            var managementClient = await httpClientManager.GetHttpClientAsync(HttpClients.ManagementAPI);
            var serializedRequest = JsonSerializer.Serialize(appConfigurationKeyValue, _jsonSerializerOptions);

            var response = await managementClient.PostAsync($"instances/{instanceSettings.Value.Id}/providers/{ResourceProviderNames.FoundationaLLM_Configuration}/appConfigurations",
                                              new StringContent(serializedRequest, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var resources = JsonSerializer.Deserialize<object>(responseContent, _jsonSerializerOptions);
                return;
            }

            throw new FoundationaLLMException($"Failed to create app configuration. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }

        public async Task CreateDataSource(string dataSourceName)
        {
            var item = DataSourceCatalog.GetDataSources().FirstOrDefault(a => a.Name == dataSourceName);

            if (item == null)
            {
                   throw new InvalidOperationException($"The data source {dataSourceName} was not found.");
            }

            //upload the dune file...
            var azureDataLakeDataSourceObjectId = await UpsertResourceAsync(
                instanceSettings.Value.Id,
                ResourceProviderNames.FoundationaLLM_DataSource,
                $"dataSources/{dataSourceName}",
                item);
        }

        public async Task CreateIndexingProfile(string indexingProfileName)
        {
            var indexingProfile = IndexingProfilesCatalog.GetIndexingProfiles().FirstOrDefault(a => a.Name == indexingProfileName);

            if (indexingProfile == null)
            {
                   throw new InvalidOperationException($"The indexing profile {indexingProfileName} was not found.");
            }

            var indexingProfileObjectId = await UpsertResourceAsync(
                instanceSettings.Value.Id,
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"indexingProfiles/{indexingProfileName}",
                indexingProfile);
        }

        public async Task CreateTextEmbeddingProfile(string textEmbeddingProfileName)
        {
            var textEmbeddingProfile = TextEmbeddingProfileCatalog.GetTextEmbeddingProfiles().FirstOrDefault(a => a.Name == textEmbeddingProfileName);

            if (textEmbeddingProfile == null)
            {
                   throw new InvalidOperationException($"The text embedding profile {textEmbeddingProfileName} was not found.");
            }

            var textEmbeddingProfileObjectId = await UpsertResourceAsync(
                instanceSettings.Value.Id,
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"textEmbeddingProfiles/{textEmbeddingProfileName}",
                textEmbeddingProfile);
        }

        public async Task CreateTextPartitioningProfile(string textPartitioningProfileName)
        {
            var textPartitioningProfile = TextPartitioningProfileCatalog.GetTextPartitioningProfiles().FirstOrDefault(a => a.Name == textPartitioningProfileName);

            if (textPartitioningProfile == null)
            {
                   throw new InvalidOperationException($"The text partitioning profile {textPartitioningProfileName} was not found.");
            }

            var textPartitioningProfileObjectId = await UpsertResourceAsync(
                instanceSettings.Value.Id,
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"textPartitioningProfiles/{textPartitioningProfileName}",
                textPartitioningProfile);
        }

        public async Task<VectorizationRequest> GetVectorizationRequest(VectorizationRequest vectorizationRequest)
        {
            return GetResourcesAsync<VectorizationRequest>(instanceSettings.Value.Id, ResourceProviderNames.FoundationaLLM_Vectorization, $"{vectorizationRequest.ObjectId}").Result;
        }

        public async Task<string> CreateVectorizationRequest(VectorizationRequest vectorizationRequest)
        {
            return await UpsertResourceAsync(
                instanceSettings.Value.Id,
                ResourceProviderNames.FoundationaLLM_Vectorization,
                vectorizationRequest.ObjectId!,
                vectorizationRequest);
        }

        /// <inheritdoc/>
        public async Task<VectorizationResult> ProcessVectorizationRequestAsync(VectorizationRequest vectorizationRequest)
        {
            var resourceId = vectorizationRequest.ObjectId!.Split("/").Last();
            var fullPath = $"instances/{instanceSettings.Value.Id}/providers/{ResourceProviderNames.FoundationaLLM_Vectorization}/{VectorizationResourceTypeNames.VectorizationRequests}/{resourceId}/process";

            var coreClient = await httpClientManager.GetHttpClientAsync(HttpClients.ManagementAPI);            
            var response = await coreClient.PostAsync(fullPath, new StringContent("{}", Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var processResult = JsonSerializer.Deserialize<VectorizationResult>(responseContent, _jsonSerializerOptions);
                return processResult!;
            }
            throw new FoundationaLLMException($"Failed to process vectorization request. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }

        public async Task DeleteVectorizationRequest(VectorizationRequest vectorizationRequest)
        {
            await DeleteResourceAsync(instanceSettings.Value.Id, ResourceProviderNames.FoundationaLLM_Vectorization, $"vectorizationRequests/{vectorizationRequest.ObjectId}");
        }

        public async Task DeleteDataSource(string profileName)
        {
            await DeleteResourceAsync(
                instanceSettings.Value.Id,
                ResourceProviderNames.FoundationaLLM_DataSource,
                $"dataSources/{profileName}");
        }

        public async Task DeleteTextPartitioningProfile(string profileName)
        {
            await DeleteResourceAsync(
                instanceSettings.Value.Id,
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"textPartitioningProfiles/{profileName}");
        }

        public async Task DeleteIndexingProfile(string profileName)
        {
            await DeleteResourceAsync(
                instanceSettings.Value.Id,
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"indexingProfiles/{profileName}");
        }

        public async Task DeleteTextEmbeddingProfile(string profileName)
        {
            await DeleteResourceAsync(
                instanceSettings.Value.Id,
                ResourceProviderNames.FoundationaLLM_Vectorization,
                $"textEmbeddingProfiles/{profileName}");
        }

        /// <inheritdoc/>
        public async Task<AgentBase> CreateAgent(string agentName, string? indexingProfileName, 
                string? textEmbeddingProfileName, string? textPartitioningProfileName)
        {
            // All test agents should have a corresponding prompt in the catalog.
            // Retrieve the agent and prompt from the test catalog.
            var agent = AgentCatalog.GetAllAgents().FirstOrDefault(a => a.Name == agentName);
            
            if (agent == null)
            {
                throw new InvalidOperationException($"The agent {agentName} was not found.");
            }
            

            // Resolve App Config values for the endpoint configuration as necessary.
            // Note: This is a temporary workaround until we have the Models and Endpoints resource provider in place.
            if (agent.OrchestrationSettings is {EndpointConfiguration: not null})
            {
                foreach (var (key, value) in agent.OrchestrationSettings.EndpointConfiguration)
                {
                    if (key.ToLower() == "api_key") continue;
                    if (value is not string stringValue || !stringValue.StartsWith("FoundationaLLM:")) continue;
                    var appConfigValue = await TestConfiguration.GetAppConfigValueAsync(value.ToString()!);
                    agent.OrchestrationSettings.EndpointConfiguration[key] = appConfigValue;
                }
            }

            var agentPrommpt = await CreatePrompt(agentName);
            // Add the prompt ObjectId to the agent.
            agent.PromptObjectId = agentPrommpt.ObjectId;

            // TODO: Create any other dependencies for the agent here.

            bool inlineContext = ((KnowledgeManagementAgent)agent).InlineContext;
            if (!inlineContext)
            {
                if(!string.IsNullOrEmpty(indexingProfileName))
                    ((KnowledgeManagementAgent)agent).Vectorization.IndexingProfileObjectIds = [(await GetIndexingProfile(indexingProfileName)).ObjectId];

                if (!string.IsNullOrEmpty(textEmbeddingProfileName))
                    ((KnowledgeManagementAgent)agent).Vectorization.TextEmbeddingProfileObjectId = (await GetTextEmbeddingProfile(textEmbeddingProfileName)).ObjectId;

                if (!string.IsNullOrEmpty(textPartitioningProfileName))
                    ((KnowledgeManagementAgent)agent).Vectorization.TextPartitioningProfileObjectId = (await GetTextPartitioningProfile(textPartitioningProfileName)).ObjectId;
            }

            // Create the agent.
            var agentObjectId = await UpsertResourceAsync(
                instanceSettings.Value.Id,
                ResourceProviderNames.FoundationaLLM_Agent,
                $"agents/{agentName}",
                agent);

            agent.ObjectId = agentObjectId;

            return agent;
        }

        /// <inheritdoc/>
        public async Task DeleteAgent(string agentName)
        {
            // Delete the agent and its dependencies.

            // Delete the agent's prompt.
            await DeletePrompt(agentName);

            // TODO: Delete other dependencies for the agent here.

            // Delete the agent.
            await DeleteResourceAsync(
                instanceSettings.Value.Id,
                ResourceProviderNames.FoundationaLLM_Agent,
                $"agents/{agentName}");
        }

        /// <inheritdoc/>
        public async Task<PromptBase> CreatePrompt(string promptName)
        {
            var prompt = PromptCatalog.GetAllPrompts().FirstOrDefault(p => p.Name == promptName);
            if (prompt == null)
            {
                throw new InvalidOperationException($"The prompt {promptName} was not found.");
            }
            
            prompt.ObjectId = await UpsertResourceAsync(
                instanceSettings.Value.Id,
                ResourceProviderNames.FoundationaLLM_Prompt,
                $"prompts/{promptName}",
                prompt);

            return prompt;
        }

        /// <inheritdoc/>
        public async Task DeletePrompt(string promptName)
        {
            await DeleteResourceAsync(
                  instanceSettings.Value.Id,
                  ResourceProviderNames.FoundationaLLM_Prompt,
                  $"prompts/{promptName}");
        }

        /// <inheritdoc/>
        public async Task<T?> GetResourcesAsync<T>(string instanceId, string resourceProvider, string resourcePath)
        {
            var coreClient = await httpClientManager.GetHttpClientAsync(HttpClients.ManagementAPI);
            var response = await coreClient.GetAsync($"instances/{instanceId}/providers/{resourceProvider}/{resourcePath}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic obj = JsonSerializer.Deserialize<dynamic>(responseContent, _jsonSerializerOptions);
                var resources = JsonSerializer.Deserialize<T>(obj[0], _jsonSerializerOptions);
                return resources;
            }

            throw new FoundationaLLMException($"Failed to get resources. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }

        /// <inheritdoc/>
        public async Task<string> UpsertResourceAsync(string instanceId, string resourceProvider, string resourcePath,
            object resource)
        {
            var coreClient = await httpClientManager.GetHttpClientAsync(HttpClients.ManagementAPI);
            var serializedRequest = JsonSerializer.Serialize(resource, _jsonSerializerOptions);

            var response = await coreClient.PostAsync($"instances/{instanceId}/providers/{resourceProvider}/{resourcePath}",
                               new StringContent(serializedRequest, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var upsertResult = JsonSerializer.Deserialize<ResourceProviderUpsertResult>(responseContent, _jsonSerializerOptions);
                if (upsertResult != null)
                    return upsertResult.ObjectId ??
                           throw new InvalidOperationException("The returned object ID is invalid.");
            }

            throw new FoundationaLLMException($"Failed to upsert resource. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }

      
        /// <inheritdoc/>
        public async Task DeleteResourceAsync(string instanceId, string resourceProvider, string resourcePath)
        {
            var coreClient = await httpClientManager.GetHttpClientAsync(HttpClients.ManagementAPI);
            var response = await coreClient.DeleteAsync($"instances/{instanceId}/providers/{resourceProvider}/{resourcePath}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return;

            if (response.IsSuccessStatusCode)
            {
                // Resource was deleted successfully. Now purge the resource, so we can reuse the name.
                await coreClient.PostAsync($"instances/{instanceId}/providers/{resourceProvider}/{resourcePath}/purge",
                    new StringContent("{}", Encoding.UTF8, "application/json"));
                if (!response.IsSuccessStatusCode)
                {
                    throw new FoundationaLLMException($"Successfully deleted the resource, but failed to purge it. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
                }
                return;
            }

            throw new FoundationaLLMException($"Failed to delete resource. Status code: {response.StatusCode}. Reason: {response.ReasonPhrase}");
        }

        async public Task<IndexingProfile> GetIndexingProfile(string name)
        {
           
            return (await GetResourcesAsync<ResourceProviderGetResult<IndexingProfile>>(instanceSettings.Value.Id, ResourceProviderNames.FoundationaLLM_Vectorization, $"indexingProfiles/{name}")).Resource;

        }

        async public Task<TextEmbeddingProfile> GetTextEmbeddingProfile(string name)
        {
            return (await GetResourcesAsync<ResourceProviderGetResult<TextEmbeddingProfile>>(instanceSettings.Value.Id, ResourceProviderNames.FoundationaLLM_Vectorization, $"textEmbeddingProfiles/{name}")).Resource;
        }

        async public Task<TextPartitioningProfile> GetTextPartitioningProfile(string name)
        {
            return (await GetResourcesAsync<ResourceProviderGetResult<TextPartitioningProfile>>(instanceSettings.Value.Id, ResourceProviderNames.FoundationaLLM_Vectorization, $"textPartitioningProfiles/{name}")).Resource;
        }
    }
}
