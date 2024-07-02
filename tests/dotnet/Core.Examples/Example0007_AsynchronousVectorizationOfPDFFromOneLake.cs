using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Core.Examples.Interfaces;
using FoundationaLLM.Core.Examples.Models;
using FoundationaLLM.Core.Examples.Setup;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples
{
    /// <summary>
    /// Example class for running asynchronous vectorization over a PDF file in a OneLake Lakehouse.
    /// Expects the following configuration values:
    ///     FoundationaLLM:DataSources:onelake_foundationallm:AuthenticationType
    ///     FoundationaLLM:DataSources:onelake_foundationallm:AccountName
    /// Expects the following document in the Files of the OneLake Files:
    ///     SDZWA-Journal-January-2024.pdf
    /// References:
    ///     PDF public source: https://sandiegozoowildlifealliance.org/Journal/january-2024
    /// </summary>
    public class Example0007_AsynchronousVectorizationOfPDFFromOneLake : BaseTest, IClassFixture<TestFixture>
    {
        private readonly IVectorizationTestService _vectorizationTestService;
        private InstanceSettings _instanceSettings;
        private string workspaceName = "FoundationaLLM";
        private string lakehouseFilePath = "FoundationaLLM.Lakehouse/Files";
        private string blobName = "SDZWA-Journal-January-2024.pdf";
        private string dataSourceName = "onelake_fllm";
        private string dataSourceObjectId = String.Empty;
        private string textPartitioningProfileName = "text_partition_profile";
        private string textEmbeddingProfileName = "text_embedding_profile_generic";
        private string indexingProfileName = "indexing_profile_pdf";
        private string searchString = "Kurt and Ollie";
        private string id = String.Empty;       

        public Example0007_AsynchronousVectorizationOfPDFFromOneLake(ITestOutputHelper output, TestFixture fixture)
            : base(output, fixture.ServiceProvider)
        {
            _vectorizationTestService = GetService<IVectorizationTestService>();
            _instanceSettings = _vectorizationTestService.InstanceSettings;
            dataSourceObjectId = $"/instances/{_instanceSettings.Id}/providers/FoundationaLLM.DataSource/dataSources/{dataSourceName}";
            id = Guid.NewGuid().ToString();
        }

        [Fact]
        public async Task RunAsync()
        {
            WriteLine("============ Asynchronous Vectorization of a PDF from OneLake ============");
            await RunExampleAsync();
        }

        private async Task RunExampleAsync()
        {
            try
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

                ContentIdentifier ci = new ContentIdentifier
                {
                    DataSourceObjectId = dataSourceObjectId,
                    MultipartId = new List<string>
                {
                    $"onelake.dfs.fabric.microsoft.com",
                    workspaceName,
                    lakehouseFilePath +"/"+ blobName
                },
                    CanonicalId = workspaceName + "/" + lakehouseFilePath + "/" + blobName.Substring(0, blobName.LastIndexOf('.'))
                };

                WriteLine($"Create the vectorization request: {id} via the Management API");
                List<VectorizationStep> steps =
                [
                    new VectorizationStep { Id = "extract", Parameters = new Dictionary<string, string>() },
                    new VectorizationStep { Id = "partition", Parameters = new Dictionary<string, string>() { { "text_partitioning_profile_name", textPartitioningProfileName } } },
                    new VectorizationStep { Id = "embed", Parameters = new Dictionary<string, string>() { { "text_embedding_profile_name", textEmbeddingProfileName } } },
                    new VectorizationStep { Id = "index", Parameters = new Dictionary<string, string>() { { "indexing_profile_name", indexingProfileName } } },
                ];
                var request = new VectorizationRequest
                {
                    RemainingSteps = new List<string> { "extract", "partition", "embed", "index" },
                    CompletedSteps = new List<string>(),
                    ProcessingType = VectorizationProcessingType.Asynchronous,
                    ContentIdentifier = ci,
                    Name = id,
                    Steps = steps,
                    ObjectId = $"{VectorizationResourceTypeNames.VectorizationRequests}/{id}"
                };
                //Create the vectorization request, re-assign the fully qualified object id if desired.
                request.ObjectId = await _vectorizationTestService.CreateVectorizationRequest(request);

                WriteLine($"Verify the vectorization request {id} was created by retrieving it from the Management API");
                var resource = await _vectorizationTestService.GetVectorizationRequest(request);
                if (resource == null)
                    throw new Exception("Vectorization request failed creation. Invalid result was returned.");

                WriteLine($"Issue the process action on the vectorization request: {id} via the Management API");
                var vectorizationResult = await _vectorizationTestService.ProcessVectorizationRequest(request);

                // Ensure the vectorization request was successful
                if (vectorizationResult == null)
                    throw new Exception("Vectorization request failed to complete successfully. Invalid result was returned.");

                if (vectorizationResult.IsSuccess == false)
                {
                    //retrieve more verbose error logging from resource....
                    resource = await _vectorizationTestService.GetVectorizationRequest(request);
                    throw new Exception($"Vectorization request failed to complete successfully. Message(s):\n{string.Join("\n", resource.ErrorMessages)}");
                }

                WriteLine($"Get the initial processing state for the vectorization request: {id} via the Management API");
                //As this is an asynchronous request, poll the status of the vectorization request until it has completed (or failed). Retrieve initial state.
                resource = await _vectorizationTestService.GetVectorizationRequest(request);

                // The finalized state of the vectorization request is either "Completed" or "Failed"
                // Give it a max of 10 minutes to complete, then exit loop and fail the test.
                WriteLine($"Polling the processing state of the async vectorization request: {id} by retrieving the request from the Management API");
                int timeRemainingMilliseconds = 600000;
                var pollDurationMilliseconds = 30000; //poll duration of 30 seconds
                while (resource.ProcessingState != VectorizationProcessingState.Completed && resource.ProcessingState != VectorizationProcessingState.Failed && timeRemainingMilliseconds > 0)
                {
                    Thread.Sleep(pollDurationMilliseconds);
                    timeRemainingMilliseconds -= pollDurationMilliseconds;
                    resource = await _vectorizationTestService.GetVectorizationRequest(request);
                }

                if (resource.ProcessingState == VectorizationProcessingState.Failed)
                    throw new Exception($"Vectorization request failed to complete successfully. Error Messages:\n{string.Join("\n", resource.ErrorMessages)}");

                if (timeRemainingMilliseconds <= 0)
                    throw new Exception("Vectorization request failed to complete successfully. Timeout exceeded.");

                WriteLine($"Vectorization request: {id} completed successfully.");

                //perform a search - this PDF yields 27 documents
                WriteLine($"Verify a search yields 27 documents.");
                TestSearchResult result = await _vectorizationTestService.QueryIndex(indexingProfileName, textEmbeddingProfileName, searchString);
                if (result.QueryResult.TotalCount != 27)
                    throw new Exception($"Query did not return the expected number of query results. Expected: 27, Retrieved: {result.QueryResult.TotalCount}");
                if (result.VectorResults.TotalCount != 27)
                    throw new Exception($"Query did not return the expected number of vector results. Expected: 27, Retrieved: {result.VectorResults.TotalCount}");
            }
            finally
            {
                WriteLine($"Delete the data source: {dataSourceName} via the Management API");
                await _vectorizationTestService.DeleteDataSource(dataSourceName);

                WriteLine($"Delete the vectorization text partitioning profile: {textPartitioningProfileName} via the Management API");
                await _vectorizationTestService.DeleteTextPartitioningProfile(textPartitioningProfileName);

                WriteLine($"Delete the vectorization text embedding profile: {textEmbeddingProfileName} via the Management API");
                await _vectorizationTestService.DeleteTextEmbeddingProfile(textEmbeddingProfileName);

                WriteLine($"Delete the vectorization indexing profile: {indexingProfileName} via the Management API and delete the created index");
                await _vectorizationTestService.DeleteIndexingProfile(indexingProfileName, true);
            }
        }
    }
}
