using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Core.Examples.Catalogs;
using FoundationaLLM.Core.Examples.Interfaces;
using FoundationaLLM.Core.Examples.Models;
using FoundationaLLM.Core.Examples.Setup;
using Microsoft.Extensions.Options;

#pragma warning disable SKEXP0001, SKEXP0020

namespace FoundationaLLM.Core.Examples.Services
{
    /// <summary>
    /// Service for running agent conversations using the Core API.
    /// </summary>
    /// <param name="managementAPITestManager">Interfacing code with the Management API responsible for resource management.</param>
    /// <param name="instanceSettings">Instance settings for the current environment.</param>
    public class VectorizationTestService(        
        IManagementAPITestManager managementAPITestManager,       
        IOptions<InstanceSettings> instanceSettings) : IVectorizationTestService
    {
        private IManagementAPITestManager _managementAPITestManager = managementAPITestManager;        
        private InstanceSettings _instanceSettings = instanceSettings.Value;

        InstanceSettings IVectorizationTestService.InstanceSettings { get { return _instanceSettings; } set { _instanceSettings = value; } }

        public async Task CreateDataSource(string name) 
        {
            await managementAPITestManager.CreateDataSource(name);
        }

        public Task CreateTextPartitioningProfile(string name)
        {
            return managementAPITestManager.CreateTextPartitioningProfile(name);
        }

        public Task CreateTextEmbeddingProfile(string name)
        {
            return managementAPITestManager.CreateTextEmbeddingProfile(name);
        }

        public Task CreateIndexingProfile(string name)
        {
            return managementAPITestManager.CreateIndexingProfile(name);
        }

        public Task<string> CreateVectorizationRequest(VectorizationRequest request)
        {
            return managementAPITestManager.CreateVectorizationRequest(request);
        }

        public Task<VectorizationResult> ProcessVectorizationRequest(VectorizationRequest request)
        {
            return managementAPITestManager.ProcessVectorizationRequestAsync(request);
        }

        public Task<VectorizationRequest> GetVectorizationRequest(VectorizationRequest request)
        {
            return managementAPITestManager.GetVectorizationRequest(request);

        }

        async public Task<TestSearchResult> QueryIndex(string indexProfileName, string embedProfileName, string query)
        {
            //get the indexing profile
            IndexingProfile indexingProfile = await managementAPITestManager.GetIndexingProfile(indexProfileName);

            TextEmbeddingProfile embeddingProfile = await managementAPITestManager.GetTextEmbeddingProfile(embedProfileName);

            return await QueryIndex(indexingProfile, embeddingProfile, query);
        }

        public async Task<ReadOnlyMemory<float>> GetVector(TextEmbeddingProfile embedProfile, string query)
        {
            //embed the query
            var embeddingModel = AIModelCatalog.EmbeddingAIModels.Where(x => x.ObjectId == embedProfile.EmbeddingAIModelObjectId).FirstOrDefault();
            var endpoint = APIEndpointConfigurationCatalog.APIEndpointConfigurations.Where(x => x.ObjectId == embeddingModel!.EndpointObjectId).FirstOrDefault();
            string oaiEndpoint = await TestConfiguration.GetAppConfigValueAsync(endpoint!.Url);
            string authType = endpoint.AuthenticationType.ToString();            
            OpenAIClient openAIClient;
            switch(authType)
            {
                case "AzureIdentity":
                    openAIClient = new OpenAIClient(new Uri(oaiEndpoint), new DefaultAzureCredential());
                    break;
                case "ApiKey":
                    openAIClient = new OpenAIClient(new Uri(oaiEndpoint), new AzureKeyCredential(await TestConfiguration.GetAppConfigValueAsync(endpoint.AuthenticationParameters["APIKey"].ToString()!)));
                    break;
                default:
                    throw new Exception("Invalid authentication type");
            }    

            EmbeddingsOptions embeddingOptions = new()
            {
                DeploymentName = embeddingModel!.DeploymentName,
                Input = { query },
            };

            var returnValue = openAIClient.GetEmbeddings(embeddingOptions);

            return returnValue.Value.Data[0].Embedding.ToArray();
        }

        async public Task<SearchIndexClient> GetIndexClient(IndexingProfile indexProfile)
        {
            var endpoint = APIEndpointConfigurationCatalog.APIEndpointConfigurations.Where(x => x.ObjectId == indexProfile.IndexingAPIEndpointConfigurationObjectId).FirstOrDefault();
            string searchServiceEndPoint = await TestConfiguration.GetAppConfigValueAsync(endpoint!.Url);
            string authType = endpoint.AuthenticationType.ToString();

            SearchIndexClient indexClient = null;

            switch (authType)
            {
                case "AzureIdentity":
                    indexClient = new SearchIndexClient(new Uri(searchServiceEndPoint), new DefaultAzureCredential());
                    break;
                case "ApiKey":
                    string adminApiKey = await TestConfiguration.GetAppConfigValueAsync(endpoint.AuthenticationParameters["ApiKey"].ToString()!);
                    indexClient = new SearchIndexClient(new Uri(searchServiceEndPoint), new AzureKeyCredential(adminApiKey));
                    break;

            }

            return indexClient;
        }

        async public Task<SearchClient> GetSearchClient(IndexingProfile indexProfile)
        {
            SearchIndexClient indexClient = await GetIndexClient(indexProfile);

            return indexClient.GetSearchClient(indexProfile.Settings["IndexName"]);
        }

        public async Task<SearchResults<object>> PerformQuerySearch(IndexingProfile indexProfile, string query)
        {
            SearchClient searchClient = await GetSearchClient(indexProfile);

            SearchOptions searchOptions = new SearchOptions
            {
                IncludeTotalCount = true
            };

            //Do basic search...
            SearchResults<object> sr = searchClient.Search<object>(query, searchOptions);

            return sr;
        }

        public async Task<SearchResults<object>> PerformVectorSearch(IndexingProfile indexProfile, TextEmbeddingProfile textEmbeddingProfile, string query, List<string> selectFields, List<string> embeddingFields, string filter = "", int k = 3)
        {
            SearchClient searchClient = await GetSearchClient(indexProfile);

            // Perform the vector similarity search  
            var searchOptions = new SearchOptions
            {
                Filter = filter,
                Size = k,
                //Select = select,
                IncludeTotalCount = true
            };

            searchOptions.VectorSearch = new VectorSearchOptions();
            //VectorizedQuery vectorizedQuery = new VectorizedQuery { KNearestNeighborsCount = k, Fields = { "vector" }, Exhaustive = true };
            
            ReadOnlyMemory<float> queryVector = await GetVector(textEmbeddingProfile, query);

            VectorizedQuery vectorizedQuery = new VectorizedQuery(queryVector);

            foreach(var field in embeddingFields)
                vectorizedQuery.Fields.Add(field);
            
            searchOptions.VectorSearch.Queries.Add(vectorizedQuery);

            SearchResults<object> response = await searchClient.SearchAsync<object>(query, searchOptions);

            return response;
        }


        public async Task<TestSearchResult> QueryIndex(IndexingProfile indexProfile, TextEmbeddingProfile embedProfile, string query)
        {
            TestSearchResult searchResult = new();

            searchResult.VectorResults = await PerformVectorSearch(indexProfile, embedProfile, query, new List<string> { "Name", "Text"}, new List<string> { "Embedding" });
            searchResult.QueryResult = await PerformQuerySearch(indexProfile, query);

            return searchResult;
        }

        public async Task DeleteDataSource(string name)
        {
            await managementAPITestManager.DeleteDataSource(name);

            return;
        }

        public Task DeleteTextPartitioningProfile(string name)
        {
            return managementAPITestManager.DeleteTextPartitioningProfile(name);
        }

        public Task DeleteTextEmbeddingProfile(string name)
        {
            return managementAPITestManager.DeleteTextEmbeddingProfile(name);
        }


        async public Task DeleteIndexingProfile(string name, bool deleteIndex = true)
        {
            IndexingProfile indexingProfile = await managementAPITestManager.GetIndexingProfile(name);

            if (indexingProfile != null)
            {
                if (deleteIndex)
                {
                    SearchIndexClient indexClient = await GetIndexClient(indexingProfile);
                    await indexClient.DeleteIndexAsync(indexingProfile.Settings["IndexName"]);
                }
            }

            await managementAPITestManager.DeleteIndexingProfile(name);
        }

        async public Task CreateAppConfiguration(AppConfigurationKeyValue appConfigurationKeyValue)
        {
            await managementAPITestManager.CreateAppConfiguration(appConfigurationKeyValue);
        }

        async public Task DeleteAppConfiguration(string key)
        {
            await managementAPITestManager.DeleteAppConfiguration(key);
        }
    }
}
