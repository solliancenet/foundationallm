using FoundationaLLM.SemanticKernel.TextEmbedding;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AI.Embeddings;
using Microsoft.SemanticKernel.Memory;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace FoundationaLLM.SemanticKernel.Memory
{
    /// <summary>
    /// Short-term volatile memory store class.
    /// </summary>
    public class ShortTermVolatileMemoryStore : VolatileMemoryStore
    {
        private List<object>? _memories;

        private readonly ITextEmbeddingGeneration _textEmbedding;
        private readonly ILogger _logger;
        private readonly string _collectionName;
        private readonly SHA1 _hash;

        /// <summary>
        /// Constructor for the short-term volatile memory store.
        /// </summary>
        /// <param name="textEmbedding">The text embedding generation service.</param>
        /// <param name="logger">The logger for the short-term volatile memory store.</param>
        public ShortTermVolatileMemoryStore(
            ITextEmbeddingGeneration textEmbedding,
            ILogger logger)
        {
            _textEmbedding = textEmbedding;
            _logger = logger;
            _collectionName = "short-term";
            _hash = SHA1.Create();
        }

        /// <summary>
        /// Initializes the memories list.
        /// </summary>
        /// <param name="memories">The list of memories.</param>
        /// <returns></returns>
        public async Task Initialize(List<object> memories)
        {
            _memories = memories;

            if (!await DoesCollectionExistAsync(_collectionName))
                await CreateCollectionAsync(_collectionName);

            foreach (var memory in _memories)
            {
                var itemToEmbed = EmbeddingUtility.Transform(memory);
                var embbedding = await _textEmbedding.GenerateEmbeddingAsync(itemToEmbed.TextToEmbed);
                await UpsertAsync(
                    _collectionName,
                    new MemoryRecord(
                        new MemoryRecordMetadata(false, GetHash(itemToEmbed.TextToEmbed), null, null, null, JsonConvert.SerializeObject(memory)),
                        embbedding,
                        null)); ;
            }
        }

        /// <summary>
        /// Gets the Base64 hash string for an input text.
        /// </summary>
        /// <param name="s">The input string.</param>
        /// <returns>The computed hash string.</returns>
        private string GetHash(string s)
        {
            return Convert.ToBase64String(
                _hash.ComputeHash(
                    Encoding.UTF8.GetBytes(s)));
        }
    }
}
