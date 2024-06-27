using FoundationaLLM.Common.Models.Vectorization;

namespace FoundationaLLM.Core.Examples.Catalogs
{
    public class VectorizationPipelineCatalog
    {
        public static readonly List<VectorizationPipeline> Items =
        [
            new VectorizationPipeline { Name = "vectorization_pipeline_different_file_types", Active = true, DataSourceObjectId = "", TextEmbeddingProfileObjectId = "", TextPartitioningProfileObjectId = "", IndexingProfileObjectId = "", TriggerType = VectorizationPipelineTriggerType.Event }
        ];

        public static List<VectorizationPipeline> GetVectorizationPipelines()
        {
            var items = new List<VectorizationPipeline>();
            items.AddRange(Items);
            return items;
        }
    }
}
