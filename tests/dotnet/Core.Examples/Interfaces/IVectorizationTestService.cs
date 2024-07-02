using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Core.Examples.Models;

namespace FoundationaLLM.Core.Examples.Interfaces;

public interface IVectorizationTestService
{
    public InstanceSettings InstanceSettings { get; set; }

    public Task CreateDataSource(string name);
    public Task CreateTextPartitioningProfile(string name);
    public Task CreateTextEmbeddingProfile(string name);
    public Task CreateIndexingProfile(string name);
    public Task<string> CreateVectorizationPipeline(string vectorizationPipelineName, string dataSourceName, string indexingProfileName,
                string textEmbeddingProfileName, string textPartitioningProfileName);
    public Task<VectorizationPipeline> GetVectorizationPipeline(string objectId);
    public Task<string> CreateVectorizationRequest(VectorizationRequest request);
    Task<VectorizationResult> ProcessVectorizationRequest(VectorizationRequest request);
    public Task<VectorizationRequest> GetVectorizationRequest(VectorizationRequest request);
    public Task<TestSearchResult> QueryIndex(string indexProfileName, string embedProfileName, string query);
    public Task<TestSearchResult> QueryIndex(IndexingProfile indexProfile, TextEmbeddingProfile embedProfile, string query);
    public Task DeleteIndexingProfile(string name, bool deleteIndex);
    public Task DeleteDataSource(string name);   
    public Task DeleteTextPartitioningProfile(string name);
    public Task DeleteTextEmbeddingProfile(string name);
    public Task DeleteVectorizationPipeline(string name);
}