using FoundationaLLM.Client.Management.Interfaces;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.Agent;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Common.Models.ResourceProviders.Prompt;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Core.Examples.Catalogs;
using FoundationaLLM.Core.Examples.Exceptions;
using FoundationaLLM.Core.Examples.Interfaces;
using FoundationaLLM.Core.Examples.Setup;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace FoundationaLLM.Core.Examples.Services
{
    /// <inheritdoc/>
    public class ManagementAPITestManager(
        IHttpClientManager httpClientManager,
        IManagementRESTClient managementRestClient,
        IManagementClient managementClient,
        IOptions<InstanceSettings> instanceSettings) : IManagementAPITestManager
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = CommonJsonSerializerOptions.GetJsonSerializerOptions();

        public async Task CreateAppConfiguration(AppConfigurationKeyValue appConfigurationKeyValue)
        {
            var response = await managementClient.Configuration.UpsertAppConfigurationAsync(appConfigurationKeyValue);

            if (!string.IsNullOrWhiteSpace(GetObjectId(response)))
            {
                return;
            }

            throw new FoundationaLLMException($"Failed to create app configuration.");
        }

        public async Task CreateDataSource(string dataSourceName)
        {
            var item = DataSourceCatalog.GetDataSources().FirstOrDefault(a => a.Name == dataSourceName);

            if (item == null)
            {
                throw new InvalidOperationException($"The data source {dataSourceName} was not found.");
            }

            var response = await managementClient.DataSources.UpsertDataSourceAsync(item);

            if (!string.IsNullOrWhiteSpace(GetObjectId(response)))
            {
                return;
            }

            throw new FoundationaLLMException($"Failed to create data source: {dataSourceName}.");
        }

        public async Task CreateIndexingProfile(string indexingProfileName)
        {
            var indexingProfile = IndexingProfilesCatalog.GetIndexingProfiles().FirstOrDefault(a => a.Name == indexingProfileName);

            if (indexingProfile == null)
            {
                throw new InvalidOperationException($"The indexing profile {indexingProfileName} was not found.");
            }

            var response = await managementClient.Vectorization.UpsertIndexingProfileAsync(indexingProfile);

            if (!string.IsNullOrWhiteSpace(GetObjectId(response)))
            {
                return;
            }

            throw new FoundationaLLMException($"Failed to create indexing profile: {indexingProfileName}.");
        }

        public async Task CreateTextEmbeddingProfile(string textEmbeddingProfileName)
        {
            var textEmbeddingProfile = TextEmbeddingProfileCatalog.GetTextEmbeddingProfiles().FirstOrDefault(a => a.Name == textEmbeddingProfileName);

            if (textEmbeddingProfile == null)
            {
                throw new InvalidOperationException($"The text embedding profile {textEmbeddingProfileName} was not found.");
            }

            var response = await managementClient.Vectorization.UpsertTextEmbeddingProfileAsync(textEmbeddingProfile);

            if (!string.IsNullOrWhiteSpace(GetObjectId(response)))
            {
                return;
            }

            throw new FoundationaLLMException($"Failed to create text embedding profile: {textEmbeddingProfileName}.");
        }

        public async Task CreateTextPartitioningProfile(string textPartitioningProfileName)
        {
            var textPartitioningProfile = TextPartitioningProfileCatalog.GetTextPartitioningProfiles().FirstOrDefault(a => a.Name == textPartitioningProfileName);

            if (textPartitioningProfile == null)
            {
                throw new InvalidOperationException($"The text partitioning profile {textPartitioningProfileName} was not found.");
            }

            var response = await managementClient.Vectorization.UpsertTextPartitioningProfileAsync(textPartitioningProfile);

            if (!string.IsNullOrWhiteSpace(GetObjectId(response)))
            {
                return;
            }

            throw new FoundationaLLMException($"Failed to create text partitioning profile: {textPartitioningProfileName}.");
        }

        public async Task<VectorizationRequest> GetVectorizationRequest(VectorizationRequest vectorizationRequest)
        {
            return await managementRestClient.Resources.GetResourcesAsync<VectorizationRequest>(
                    ResourceProviderNames.FoundationaLLM_Vectorization, vectorizationRequest.ObjectId!);
        }

        public async Task<string> CreateVectorizationRequest(VectorizationRequest vectorizationRequest)
        {
            var response = await managementRestClient.Resources.UpsertResourceAsync(
                ResourceProviderNames.FoundationaLLM_Vectorization,
                vectorizationRequest.ObjectId!, 
                vectorizationRequest);
            return GetObjectId(response);
        }

        /// <inheritdoc/>
        public async Task<VectorizationResult> ProcessVectorizationRequestAsync(VectorizationRequest vectorizationRequest)
        {
            var resourceId = vectorizationRequest.ObjectId!.Split("/").Last();
            var fullPath = $"instances/{instanceSettings.Value.Id}/providers/{ResourceProviderNames.FoundationaLLM_Vectorization}/{VectorizationResourceTypeNames.VectorizationRequests}/{resourceId}/process";

            var managementClient = await httpClientManager.GetHttpClientAsync(HttpClients.ManagementAPI);            
            var response = await managementClient.PostAsync(fullPath, new StringContent("{}", Encoding.UTF8, "application/json"));
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
            await managementClient.DataSources.DeleteDataSourceAsync(profileName);
            // Purge the data source so we can reuse the name.
            await managementClient.DataSources.PurgeDataSourceAsync(profileName);
        }

        public async Task DeleteTextPartitioningProfile(string profileName)
        {
            await managementClient.Vectorization.DeleteTextPartitioningProfileAsync(profileName);
            // Purge the text partitioning profile so we can reuse the name.
            await managementClient.Vectorization.PurgeTextPartitioningProfileAsync(profileName);
        }

        public async Task DeleteIndexingProfile(string profileName)
        {
            await managementClient.Vectorization.DeleteIndexingProfileAsync(profileName);
            // Purge the indexing profile so we can reuse the name.
            await managementClient.Vectorization.PurgeIndexingProfileAsync(profileName);
        }

        public async Task DeleteTextEmbeddingProfile(string profileName)
        {
            await managementClient.Vectorization.DeleteTextEmbeddingProfileAsync(profileName);
            // Purge the text embedding profile so we can reuse the name.
            await managementClient.Vectorization.PurgeTextEmbeddingProfileAsync(profileName);
        }

        /// <inheritdoc/>
        public async Task DeletePrompt(string promptName)
        {
            await managementClient.Prompts.DeletePromptAsync(promptName);
            // Purge the prompt so we can reuse the name.
            await managementClient.Prompts.PurgePromptAsync(promptName);
        }

        /// <inheritdoc/>
        public async Task DeleteAgent(string agentName)
        {
            // Delete the agent and its dependencies.

            // Delete the agent's prompt.
            await DeletePrompt(agentName);

            // TODO: Delete other dependencies for the agent here.

            // Delete the agent.
            await managementClient.Agents.DeleteAgentAsync(agentName);
            // Purge the agent so we can reuse the name.
            await managementClient.Agents.PurgeAgentAsync(agentName);
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

            var agentPrompt = await CreatePrompt(agentName);
            // Add the prompt ObjectId to the agent.
            agent.PromptObjectId = agentPrompt.ObjectId;

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
            var response = await managementClient.Agents.UpsertAgentAsync(agent);

            agent.ObjectId = GetObjectId(response);

            return agent;
        }

        /// <inheritdoc/>
        public async Task<PromptBase> CreatePrompt(string promptName)
        {
            var prompt = PromptCatalog.GetAllPrompts().FirstOrDefault(p => p.Name == promptName);
            if (prompt == null)
            {
                throw new InvalidOperationException($"The prompt {promptName} was not found.");
            }
            
            var response = await managementClient.Prompts.UpsertPromptAsync(prompt);
            prompt.ObjectId = GetObjectId(response);

            return prompt;
        }

        private string GetObjectId(ResourceProviderUpsertResult result)
        {
            return result.ObjectId ?? throw new InvalidOperationException("The returned object ID is invalid.");
        }
      
        /// <inheritdoc/>
        public async Task DeleteResourceAsync(string instanceId, string resourceProvider, string resourcePath)
        {
            try
            {
                await managementRestClient.Resources.DeleteResourceAsync(resourceProvider, resourcePath);
                // Resource was deleted successfully. Now purge the resource, so we can reuse the name.
                await managementRestClient.Resources
                    .ExecuteResourceActionAsync<ResourceProviderActionResult>(resourceProvider, $"{resourcePath}/purge", new { });
            }
            catch (Exception ex)
            {
                throw new FoundationaLLMException($"Failed to delete resource: {ex}");
            }
        }

        public async Task<IndexingProfile> GetIndexingProfile(string name)
        {
            var response = await managementClient.Vectorization.GetIndexingProfileAsync(name);
            return response.Resource;
        }

        public async Task<TextEmbeddingProfile> GetTextEmbeddingProfile(string name)
        {
            var response = await managementClient.Vectorization.GetTextEmbeddingProfileAsync(name);
            return response.Resource;
        }

        public async Task<TextPartitioningProfile> GetTextPartitioningProfile(string name)
        {
            var response = await managementClient.Vectorization.GetTextPartitioningProfileAsync(name);
            return response.Resource;
        }
    }
}
