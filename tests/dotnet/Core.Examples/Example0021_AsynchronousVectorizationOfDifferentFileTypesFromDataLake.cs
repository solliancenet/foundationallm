using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Core.Examples.Interfaces;
using FoundationaLLM.Core.Examples.Setup;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples
{
    /// <summary>
    /// Example class for running asynchronous vectorization over many files in the Azure Data Lake storage account.
    /// Expects the following configuration values:
    ///     FoundationaLLM:DataSources:datalake_different_file_types:AuthenticationType
    ///     FoundationaLLM:DataSources:datalake_different_file_types:AccountName
    /// Expects the following directory in the storage account:
    ///     /vectorization-input/fllm-org-data/
    /// </summary>
    public class Example0021_AsynchronousVectorizationOfDifferentFileTypesFromDataLake : BaseTest, IClassFixture<TestFixture>
    {
        private readonly IVectorizationTestService _vectorizationTestService;
        private InstanceSettings _instanceSettings;
        private string dataSourceName = "datalake_different_file_types";
        private string textPartitioningProfileName = "text_partition_profile";
        private string textEmbeddingProfileName = "text_embedding_profile_generic";
        private string indexingProfileName = "indexing_profile_different_file_types";
        private string vectorizationPipelineName = "vectorization_pipeline_different_file_types";
        // private string searchString = "FoundationaLLM";

        public Example0021_AsynchronousVectorizationOfDifferentFileTypesFromDataLake(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture.ServiceProvider)
        {
            _vectorizationTestService = GetService<IVectorizationTestService>();
            _instanceSettings = _vectorizationTestService.InstanceSettings;
        }

        [Fact]
        public async Task RunAsync()
        {
            WriteLine("============ Asynchronous Vectorization of many file types from Data Lake ============");
            await RunExampleAsync();
        }

        private async Task RunExampleAsync()
        {
            WriteLine($"Create the data source: {dataSourceName} via the Management API");
            await _vectorizationTestService.CreateDataSource(dataSourceName);

            Thread.Sleep(5000); // processing too quickly, pause after the creation of the data source

            WriteLine($"Create the vectorization text partitioning profile: {textPartitioningProfileName} via the Management API");
            await _vectorizationTestService.CreateTextPartitioningProfile(textPartitioningProfileName);
                        
            WriteLine($"Create the vectorization text embedding profile: {textEmbeddingProfileName} via the Management API");
            await _vectorizationTestService.CreateTextEmbeddingProfile(textEmbeddingProfileName);

            WriteLine($"Create the vectorization indexing profile: {indexingProfileName} via the Management API");
            await _vectorizationTestService.CreateIndexingProfile(indexingProfileName);

            WriteLine($"Create the vectorization pipeline: {vectorizationPipelineName} via the Management API");
            var pipelineObjectId = await _vectorizationTestService.CreateVectorizationPipeline(vectorizationPipelineName, dataSourceName, indexingProfileName, textEmbeddingProfileName, textPartitioningProfileName);
            
            WriteLine($"Verify the pipeline {pipelineObjectId} was created by retrieving it from the Management API");
            var resource = await _vectorizationTestService.GetVectorizationPipeline(pipelineObjectId);
            if (resource == null)
                throw new Exception("Vectorization pipeline failed creation. Invalid result was returned.");

            if (resource.Active == false)
            {
                throw new Exception("Vectorization pipeline failed to start.");
            }

            // The finalized state of the vectorization pipeline is Inactive
            // Give it a max of 10 minutes to complete, then exit loop and fail the test.
            WriteLine($"Polling the processing state of the vectorization pipeline: {pipelineObjectId}");
            int timeRemainingMilliseconds = 600000;
            var pollDurationMilliseconds = 30000; //poll duration of 30 seconds
            while (resource.Active && timeRemainingMilliseconds > 0)
            {
                Thread.Sleep(pollDurationMilliseconds);
                timeRemainingMilliseconds -= pollDurationMilliseconds;
                resource = await _vectorizationTestService.GetVectorizationPipeline(pipelineObjectId);
            }

            if (timeRemainingMilliseconds <= 0)
                throw new Exception("Vectorization pipeline failed to complete successfully. Timeout exceeded.");

            /*
            //perform a search - this PDF yields 27 documents
            WriteLine("Verify a search yields >= 500 documents.");
            TestSearchResult result = await _vectorizationTestService.QueryIndex(indexingProfileName, textEmbeddingProfileName, searchString);
            if(result.QueryResult.TotalCount >= 500)
                throw new Exception($"Query did not return the expected number of query results. Expected: 500+, Retrieved: {result.QueryResult.TotalCount}");
            if(result.VectorResults.TotalCount!=27)
                throw new Exception($"Query did not return the expected number of vector results. Expected: 27, Retrieved: {result.VectorResults.TotalCount}");
            */

            WriteLine($"Delete the data source: {dataSourceName} via the Management API");
            await _vectorizationTestService.DeleteDataSource(dataSourceName);

            WriteLine($"Delete the vectorization text partitioning profile: {textPartitioningProfileName} via the Management API");
            await _vectorizationTestService.DeleteTextPartitioningProfile(textPartitioningProfileName);

            WriteLine($"Delete the vectorization text embedding profile: {textEmbeddingProfileName} via the Management API");
            await _vectorizationTestService.DeleteTextEmbeddingProfile(textEmbeddingProfileName);

            WriteLine($"Delete the vectorization indexing profile: {indexingProfileName} via the Management API along with the index");
            await _vectorizationTestService.DeleteIndexingProfile(indexingProfileName, true);

            WriteLine($"Delete the vectorization pipeline: {vectorizationPipelineName} via the Management API");
            await _vectorizationTestService.DeleteVectorizationPipeline(vectorizationPipelineName);
        }
    }
}
