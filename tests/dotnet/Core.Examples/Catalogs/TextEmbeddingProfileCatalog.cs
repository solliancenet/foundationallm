using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Core.Examples.Constants;

namespace FoundationaLLM.Core.Examples.Catalogs
{
    public static class TextEmbeddingProfileCatalog
    {
        public static readonly List<TextEmbeddingProfile> Items =
        [
            new TextEmbeddingProfile { Name = "text_embedding_profile_gateway", Settings = new Dictionary<string, string> { { "model_name", "text-embedding-ada-002"} },  TextEmbedding = TextEmbeddingType.GatewayTextEmbedding },
            new TextEmbeddingProfile { Name = "text_embedding_profile_generic", TextEmbedding = TextEmbeddingType.SemanticKernelTextEmbedding, EmbeddingAIModelObjectId=TestAIModelNames.Embeddings_Default }
        ];

        public static List<TextEmbeddingProfile> GetTextEmbeddingProfiles()
        {
            var items = new List<TextEmbeddingProfile>();
            items.AddRange(Items);
            return items;
        }
    }
}
