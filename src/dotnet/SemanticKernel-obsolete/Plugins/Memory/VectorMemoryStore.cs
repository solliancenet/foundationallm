using Azure.Search.Documents.Indexes;
using FoundationaLLM.Common.Models.Search;
using FoundationaLLM.SemanticKernel.TextEmbedding;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AI.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Newtonsoft.Json;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace FoundationaLLM.SemanticKernel.Plugins.Memory
{
    /// <summary>
    /// Vector memory store plugin for Semantic Kernel.
    /// </summary>
    public class VectorMemoryStore
    {
        readonly string _collectionName;
        readonly IMemoryStore _memoryStore;
        readonly ITextEmbeddingGeneration _textEmbedding;
        readonly ILogger<VectorMemoryStore> _logger;
        readonly SHA1 _hash;

        /// <summary>
        /// The memory store.
        /// </summary>
        public IMemoryStore MemoryStore => _memoryStore;

        /// <summary>
        /// Constructor for the Vector Memory Store.
        /// </summary>
        /// <param name="collectionName">The name associated with a collection of embeddings.</param>
        /// <param name="memoryStore">The memory store.</param>
        /// <param name="textEmbedding">The text embedding generation service.</param>
        /// <param name="logger">The logger for the Vector Memory Store.</param>
        public VectorMemoryStore(
            string collectionName,
            IMemoryStore memoryStore,
            ITextEmbeddingGeneration textEmbedding,
            ILogger<VectorMemoryStore> logger)
        {
            _collectionName = collectionName;
            _memoryStore = memoryStore;
            _textEmbedding = textEmbedding;
            _logger = logger;
            _hash = SHA1.Create();
        }

        /// <summary>
        /// Add an object instance and its associated vectorization to the underlying memory.
        /// </summary>
        /// <param name="item">The object instance to be added to the memory.</param>
        /// <param name="itemName">The name of the object instance.</param>
        /// <returns></returns>
        public async Task AddMemory(object item, string itemName)
        {
            try
            {
                if (item is EmbeddedEntity entity)
                    entity.entityType__ = item.GetType().Name;
                else
                    throw new ArgumentException("Only objects derived from EmbeddedEntity can be added to memory.");

                // Prepare the object for embedding
                var itemToEmbed = EmbeddingUtility.Transform(item);

                // Get the embeddings from OpenAI: the ITextEmbeddingGeneration service is exposed by SemanticKernel
                // and is responsible for calling the text embedding endpoint to get the vectorized representation
                // of the incoming object.
                // Use by default the more elaborate text representation based on EmbeddingFieldAttribute
                // The purely text representation generated based on the EmbeddingFieldAttribute is well suited for 
                // embedding and it allows you to control precisely which attributes will be used as inputs in the process.
                // In general, it is recommended to avoid identifier attributes (e.g., GUIDs) as they do not provide
                // any meaningful context for the embedding process.
                // Exercise: Test also using the JSON text representation - itemToEmbed.ObjectToEmbed
                var embedding = await _textEmbedding.GenerateEmbeddingAsync(itemToEmbed.TextToEmbed);

                // This will send the vectorized object to the Azure Cognitive Search index.
                await _memoryStore.UpsertAsync(_collectionName, new MemoryRecord(
                    new MemoryRecordMetadata(
                        false,
                        itemToEmbed.ObjectToEmbed.ContainsKey("id")
                            ? itemToEmbed.ObjectToEmbed.Value<string>("id")!
                            : GetHash(itemToEmbed.TextToEmbed),
                        itemToEmbed.TextToEmbed,
                        string.Empty,
                        string.Empty,
                        JsonConvert.SerializeObject(item)),
                    embedding,
                    null));

                _logger.LogInformation($"Memorized vector for item: {itemName} of type {item.GetType().Name}");
            }
            catch (Exception x)
            {
                _logger.LogError($"Exception while generating vector for [{itemName} of type {item.GetType().Name}]: " + x.Message);
            }
        }

        /// <summary>
        /// Removes an object instance and its associated vectorization from the underlying memory.
        /// </summary>
        /// <param name="item">The object instance to be removed from the memory.</param>
        /// <returns></returns>
        public async Task RemoveMemory(object item)
        {
            try
            {
                var objectType = item.GetType();
                var properties = objectType.GetProperties();

                foreach (var property in properties)
                {
                    var searchableAttribute = property.GetCustomAttribute<SearchableFieldAttribute>();
                    if (searchableAttribute != null && searchableAttribute.IsKey)
                    {
                        var propertyName = property.Name;
                        var propertyValue = property.GetValue(item);

                        _logger.LogInformation($"Found key property: {propertyName}, Value: {propertyValue}");
                        await _memoryStore.RemoveAsync(_collectionName, propertyValue?.ToString()!);

                        _logger.LogInformation("Removed memory successfully.");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: RemoveMemory(): {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the nearest match to an embedding of type float. Does not guarantee that the collection exists.
        /// </summary>
        /// <param name="textToMatch">The text to compare the collection's embeddings with.</param>
        /// <param name="limit">The maximum number of similarity results to return.</param>
        /// <param name="minRelevanceScore">The minimum relevance threshold for returned results.</param>
        /// <returns>The metadata associated with the memory and the similarity score.</returns>
        public async IAsyncEnumerable<MemoryQueryResult> GetNearestMatches(string textToMatch, int limit, double minRelevanceScore = 0.7)
        {
            var embedding = await _textEmbedding.GenerateEmbeddingAsync(textToMatch);
            await foreach (var result in _memoryStore.GetNearestMatchesAsync(
                _collectionName,
                embedding,
                limit,
                minRelevanceScore))
            {
                yield return new MemoryQueryResult(result.Item1.Metadata, result.Item2, null);
            }
        }

        /// <summary>
        /// Gets the nearest matches to an embedding of type float. Does not guarantee that the collection exists.
        /// </summary>
        /// <param name="embeddingToMatch">The embedding to compare the collection's embeddings with.</param>
        /// <param name="limit">The maximum number of similarity results to return.</param>
        /// <param name="minRelevanceScore">The minimum cosine similarity threshold for returned results.</param>
        /// <returns>The metadata associated with the memory and the similarity score.</returns>
        public async IAsyncEnumerable<MemoryQueryResult> GetNearestMatches(ReadOnlyMemory<float> embeddingToMatch, int limit, double minRelevanceScore = 0.7)
        {
            await foreach (var result in _memoryStore.GetNearestMatchesAsync(
            _collectionName,
                embeddingToMatch,
                limit,
                minRelevanceScore))
            {
                yield return new MemoryQueryResult(result.Item1.Metadata, result.Item2, null);
            }
        }

        /// <summary>
        /// Generates an embedding from the given text.
        /// </summary>
        /// <param name="textToEmbed">The text to embed.</param>
        /// <returns>A list of embedding structs representing the input text.</returns>
        public async Task<ReadOnlyMemory<float>> GetEmbedding(string textToEmbed)
        {
            return await _textEmbedding.GenerateEmbeddingAsync(textToEmbed);
        }

        private string GetHash(string s)
        {
            return Convert.ToBase64String(
                _hash.ComputeHash(
                    Encoding.UTF8.GetBytes(s)));
        }
    }
}
