using Azure;
using Azure.Core.Serialization;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using FoundationaLLM.Common.Models.Search;
using FoundationaLLM.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Reflection;
using System.Text.Json;

namespace FoundationaLLM.Core.Services
{
    /// <summary>
    /// This service is provided as an example, it is not used by the main RAG flow.
    /// </summary>
    public class CognitiveSearchService : IVectorDatabaseServiceManagement, IVectorDatabaseServiceQueries
    {
        private const int ModelDimensions = 1536;
        private const string VectorFieldName = "vector";
        private readonly int _maxVectorSearchResults = default;
        private readonly ILogger _logger;
        private readonly SearchClient _searchClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="CognitiveSearchService"/> class.
        /// </summary>
        /// <param name="azureSearchAdminKey">The Azure Cognitive Search admin key.</param>
        /// <param name="azureSearchServiceEndpoint">The Azure Cognitive Search endpoint.</param>
        /// <param name="azureSearchIndexName">The name of the Azure Cognitive Search vector index.</param>
        /// <param name="maxVectorSearchResults">The maximum number of Azure Cognitive Search vector
        /// search results to return.</param>
        /// <param name="logger">The configured logging interface.</param>
        /// <param name="createIndexIfNotExists">If this setting is true, the service will create the
        /// Azure Cognitive Search vector index upon initialization if it does not exist.</param>
        public CognitiveSearchService(string azureSearchAdminKey, string azureSearchServiceEndpoint,
            string azureSearchIndexName, string maxVectorSearchResults, ILogger logger, bool createIndexIfNotExists = false)
        {
            _maxVectorSearchResults = int.TryParse(maxVectorSearchResults, out _maxVectorSearchResults) ? _maxVectorSearchResults : 10;
            _logger = logger;

            // Define client options to use camelCase when serializing property names.
            var options = new SearchClientOptions()
            {
                Serializer = new JsonObjectSerializer(
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    })
            };

            var searchCredential = new AzureKeyCredential(azureSearchAdminKey);
            var indexClient = new SearchIndexClient(new Uri(azureSearchServiceEndpoint), searchCredential, options);
            _searchClient = indexClient.GetSearchClient(azureSearchIndexName);

            if (!createIndexIfNotExists) return;
            // If the Azure Cognitive Search index does not exists, create the index.
            try
            {
                CreateIndexAsync(indexClient, azureSearchIndexName, true).Wait();
            }
            catch (Exception ex)
            {
                _logger.LogError("Azure Cognitive Search index creation failure: " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Inserts an entity into the Azure Cognitive Search vector index.
        /// </summary>
        /// <param name="document">The entity to add to the vector index.</param>
        /// <returns></returns>
        public async Task InsertVector(object document)
        {
            await InsertVectors(new[] { document });
        }


        /// <summary>
        /// Inserts a collection of entities into the Azure Cognitive Search vector index.
        /// </summary>
        /// <param name="documents">The entities to add to the vector index.</param>
        /// <returns></returns>
        public async Task InsertVectors(IEnumerable<object> documents)
        {
            try
            {
                await _searchClient.IndexDocumentsAsync(IndexDocumentsBatch.Upload(documents));
                _logger.LogInformation("Inserted new vectors into Cognitive Search");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: InsertVectors(): {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Deletes an entity from the Azure Cognitive Search vector index.
        /// </summary>
        /// <param name="document">The entity to remove from the vector index.</param>
        /// <returns></returns>
        public async Task DeleteVector(object document)
        {
            try
            {
                var objectType = document.GetType();
                var properties = objectType.GetProperties();

                foreach (var property in properties)
                {
                    var searchableAttribute = property.GetCustomAttribute<SearchableFieldAttribute>();
                    if (searchableAttribute != null && searchableAttribute.IsKey)
                    {
                        var propertyName = property.Name;
                        var propertyValue = property.GetValue(document);

                        Console.WriteLine($"Found key property: {propertyName}, Value: {propertyValue}");
                        await _searchClient.DeleteDocumentsAsync(propertyName, new[] { propertyValue?.ToString() });

                        _logger.LogInformation("Deleted vector from Cognitive Search");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: DeleteVector(): {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Performs a vector similarity search against the Azure Cognitive Search vector index.
        /// </summary>
        /// <param name="embeddings">The vector used in the index search.</param>
        /// <returns></returns>
        public async Task<string> VectorSearchAsync(float[] embeddings)
        {
            var retDocs = new List<string>();

            var resultDocuments = string.Empty;

            try
            {
                // Perform the vector similarity search.
                var vector = new SearchQueryVector { K = _maxVectorSearchResults, Fields = VectorFieldName, Value = embeddings };
                var searchOptions = new SearchOptions
                {
                    Vector = vector,
                    Size = _maxVectorSearchResults
                };

                SearchResults<SearchDocument> response = await _searchClient.SearchAsync<SearchDocument>(null, searchOptions);

                var count = 0;
                var serializerOptions = new JsonSerializerOptions
                {
                    WriteIndented = false
                };
                await foreach (var result in response.GetResultsAsync())
                {
                    count++;
                    var filteredDocument = new SearchDocument();
                    var searchDocument = result.Document;
                    foreach (var property in searchDocument)
                    {
                        // Exclude null properties, empty arrays/lists, and the "vector" property.
                        // This helps minimize the amount of data and also eliminates fields that may only relate to other document types.
                        if (property.Value != null && property.Key != VectorFieldName && !IsEmptyArrayOrList(property.Value))
                        {
                            filteredDocument[property.Key] = property.Value;
                        }
                    }

                    retDocs.Add(JsonSerializer.Serialize(filteredDocument, serializerOptions));
                }
                resultDocuments = string.Join(Environment.NewLine + "-", retDocs);
                _logger.LogInformation($"Total Results: {count}");

            }
            catch (Exception ex)
            {
                _logger.LogError($"There was an error conducting a vector search: {ex.Message}");
            }

            return resultDocuments;
        }

        // Helper method to check if a value is an empty array or list.
        private bool IsEmptyArrayOrList(object value)
        {
            if (value is Array array)
            {
                return array.Length == 0;
            }

            if (value is IList list)
            {
                return list.Count == 0;
            }

            return false;
        }

        internal async Task CreateIndexAsync(SearchIndexClient indexClient, string indexName,
            bool onlyCreateIfNotExists)
        {
            try
            {
                if (onlyCreateIfNotExists)
                {
                    if (await indexClient.GetIndexAsync(indexName) != null)
                    {
                        _logger.LogInformation($"The {indexName} index already exists; skipping index creation.");
                        return;
                    }
                }

                var vectorSearchConfigName = "vector-config";
                
                var fieldBuilder = new FieldBuilder();
                var customerFields = fieldBuilder.Build(typeof(Customer));
                var productFields = fieldBuilder.Build(typeof(Product));
                var salesOrderFields = fieldBuilder.Build(typeof(SalesOrder));

                // Combine the three search fields, eliminating duplicate names:
                var allFields = customerFields
                    .Concat(productFields)
                    .Concat(salesOrderFields)
                    .GroupBy(field => field.Name)
                    .Select(group => group.First())
                    .ToList();
                allFields.Add(
                    new SearchField(VectorFieldName, SearchFieldDataType.Collection(SearchFieldDataType.Single))
                    {
                        IsSearchable = true,
                        Dimensions = ModelDimensions,
                        VectorSearchConfiguration = vectorSearchConfigName
                    });

                SearchIndex searchIndex = new(indexName)
                {
                    VectorSearch = new()
                    {
                        AlgorithmConfigurations =
                        {
                            new VectorSearchAlgorithmConfiguration(vectorSearchConfigName, "hnsw")
                        }
                    },
                    Fields = allFields
                };

                await indexClient.CreateIndexAsync(searchIndex);
                _logger.LogInformation($"Created the {indexName} index.");
            }
            catch (Exception e)
            {
                _logger.LogError($"An error occurred while trying to build the {indexName} index: {e}");
                throw;
            }
        }
    }
}
