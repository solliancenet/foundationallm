﻿using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Models.ResourceProviders.Configuration;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Core.Examples.Interfaces;
using FoundationaLLM.Core.Examples.Models;
using FoundationaLLM.Core.Examples.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;

namespace FoundationaLLM.Core.Examples
{
    /// <summary>
    /// Example class for running the default FoundationaLLM agent completions in both session and sessionless modes.
    /// Expects the following configuration values:
    ///     FoundationaLLM:DataSources:datalake_vectorization_input:AuthenticationType
    ///     FoundationaLLM:DataSources:datalake_vectorization_input:AccountName
    /// Expects the following document in the storage account:
    ///     /vectorization-input/really_big.pdf
    /// </summary>
    public class Example0010_AsynchronousVectorizationOfPDFFromDataLakeUsingGateway : TestBase, IClassFixture<TestFixture>
	{
		private readonly IVectorizationTestService _vectorizationTestService;        
        private readonly InstanceSettings _instanceSettings;
        private readonly string textPartitionProfileName = "text_partition_profile";
        private readonly string textEmbeddingProfileName = "text_embedding_profile_gateway";
        private readonly string indexingProfileName = "indexing_profile_really_big";
        private readonly string genericTextEmbeddingProfileName = "text_embedding_profile_generic";
        private readonly string dataSourceName = "datalake_vectorization_input";
        private readonly string containerName = "vectorization-input";
        private readonly string blobName = "really_big.pdf";
        private readonly string indexName = "reallybig";
        private readonly string dataSourceObjectId;
        private readonly string id = String.Empty;
        private readonly BlobStorageServiceSettings? _settings;
        private readonly string accountNameAppConfigKey;
        private readonly string authenticationTypeAppConfigKey;

        public Example0010_AsynchronousVectorizationOfPDFFromDataLakeUsingGateway(ITestOutputHelper output, TestFixture fixture)
			: base(1, output, fixture)
		{
            _vectorizationTestService = GetService<IVectorizationTestService>();
            _instanceSettings = _vectorizationTestService.InstanceSettings;
            dataSourceObjectId = $"/instances/{_instanceSettings.Id}/providers/FoundationaLLM.DataSource/dataSources/{dataSourceName}";
            id = Guid.NewGuid().ToString();
            _settings = GetService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Vectorization_Storage);
            accountNameAppConfigKey = $"FoundationaLLM:DataSources:{dataSourceName}:AccountName";
            authenticationTypeAppConfigKey = $"FoundationaLLM:DataSources:{dataSourceName}:AuthenticationType";
        }
        
        [Fact]
		public async Task RunAsync()
		{
            try
            {
                await PreExecute();

                await RunExampleAsync();
            }
            finally
            {
                await PostExecute();
            }
		}

        private async Task PreExecute()
        {
            WriteLine($"Create the App Configuration key {accountNameAppConfigKey}");
            await _vectorizationTestService.CreateAppConfiguration(
                new AppConfigurationKeyValue
                {
                    Name = accountNameAppConfigKey,
                    Key = accountNameAppConfigKey,
                    Value = _settings!.AccountName,
                    ContentType = ""
                }
            );

            WriteLine($"Create the App Configuration key {authenticationTypeAppConfigKey}");
            await _vectorizationTestService.CreateAppConfiguration(
                new AppConfigurationKeyValue
                {
                    Name = authenticationTypeAppConfigKey,
                    Key = authenticationTypeAppConfigKey,
                    Value = "AzureIdentity",
                    ContentType = ""
                }
            );

            WriteLine($"Setup: Create the data source: {dataSourceName} via the Management API");
            await _vectorizationTestService.CreateDataSource(dataSourceName);

            WriteLine($"Setup: Create the vectorization text partitioning profile: {textPartitionProfileName} via the Management API");
            await _vectorizationTestService.CreateTextPartitioningProfile(textPartitionProfileName);

            WriteLine($"Setup: Create the vectorization text embedding profile: {textEmbeddingProfileName} via the Management API");
            await _vectorizationTestService.CreateTextEmbeddingProfile(textEmbeddingProfileName);
            
            // The generic text embedding profile is used to embed the search text when testing the index retrieval.
            WriteLine($"Setup: Create a generic vectorization text embedding profile: {genericTextEmbeddingProfileName} via the Management API");
            await _vectorizationTestService.CreateTextEmbeddingProfile(genericTextEmbeddingProfileName);
           
            WriteLine($"Setup: Create the vectorization indexing profile: {indexingProfileName} via the Management API");
            await _vectorizationTestService.CreateIndexingProfile(indexingProfileName);
        }

        private async Task RunExampleAsync()
        {           
            ContentIdentifier ci = new()
            {
                DataSourceObjectId = dataSourceObjectId,                
                MultipartId =
                [
                    $"{_settings!.AccountName}.blob.core.windows.net",
                    containerName,
                    blobName
                ],
                CanonicalId = $"{containerName}/{(blobName[..blobName.LastIndexOf('.')])}"
            };

            WriteLine($"Create the vectorization request: {id} via the Management API");
            List<VectorizationStep> steps =
            [
                new VectorizationStep { Id = "extract", Parameters = [] },
                new VectorizationStep { Id = "partition", Parameters = new Dictionary<string, string>() { { "text_partitioning_profile_name", textPartitionProfileName } } },
                new VectorizationStep { Id = "embed", Parameters = new Dictionary<string, string>() { { "text_embedding_profile_name", textEmbeddingProfileName } } },
                new VectorizationStep { Id = "index", Parameters = new Dictionary<string, string>() { { "indexing_profile_name", indexingProfileName } } },
            ];            
            var request = new VectorizationRequest
            {
                RemainingSteps = ["extract", "partition", "embed", "index"],
                CompletedSteps = [],
                ProcessingType = VectorizationProcessingType.Asynchronous,
                ContentIdentifier = ci,
                Name = id,
                Steps = steps,
                ObjectId = $"{VectorizationResourceTypeNames.VectorizationRequests}/{id}"
            };            
            //Create the vectorization request, re-assign the fully qualified object id if desired.
            request.ObjectId = await _vectorizationTestService.CreateVectorizationRequest(request);

            WriteLine($"Issue the process action on the vectorization request: {id} via the Management API");
            //Issue the process action on the vectorization request
            var vectorizationResult = await _vectorizationTestService.ProcessVectorizationRequest(request)
                ?? throw new Exception("Vectorization request failed to complete successfully. Invalid result was returned.");
            if (vectorizationResult.IsSuccess == false)
                throw new Exception($"Vectorization request failed to complete successfully. Message: {vectorizationResult.ErrorMessage}");

            WriteLine($"Get the initial processing state for the vectorization request: {id} via the Management API");
            //As this is an asynchronous request, poll the status of the vectorization request until it has completed (or failed). Retrieve initial state.
            VectorizationRequest resource = await _vectorizationTestService.GetVectorizationRequest(request);

            // The finalized state of the vectorization request is either "Completed" or "Failed"
            // Give it a max of 15 minutes to complete, then exit loop and fail the test.
            WriteLine($"Polling the processing state of the async vectorization request: {id} by retrieving the request from the Management API");
            int timeRemainingMilliseconds = 900000;
            var pollDurationMilliseconds = 30000; //poll duration of 30 seconds
            while (resource.ProcessingState != VectorizationProcessingState.Completed && resource.ProcessingState != VectorizationProcessingState.Failed && timeRemainingMilliseconds > 0)
            {                
                Thread.Sleep(pollDurationMilliseconds);                
                timeRemainingMilliseconds -= pollDurationMilliseconds;
                resource = await _vectorizationTestService.GetVectorizationRequest(request);
            }

            if (resource.ProcessingState == VectorizationProcessingState.Failed)
                throw new Exception($"Vectorization request failed to complete successfully. Error Messages:\n{string.Join("\n",resource.ErrorMessages)}");

            if (timeRemainingMilliseconds <=0)
                throw new Exception("Vectorization request failed to complete successfully. Timeout exceeded.");           

            
            //verify artifacts
            //TODO

            //perform a search
            TestSearchResult result = await _vectorizationTestService.QueryIndex(indexingProfileName, genericTextEmbeddingProfileName, indexName);

            //verify expected results
            if (result.VectorResults.TotalCount != 50)
                throw new Exception("Expected 281 vector results, but got " + result.VectorResults.TotalCount);

            //vaidate chunks in index...
            if ( result.QueryResult.TotalCount != 0)
                throw new Exception("Expected 2883 search results, but got " + result.QueryResult.TotalCount);
            
        }

        private async Task PostExecute()
        {
            WriteLine($"Delete the App Configuration key {accountNameAppConfigKey}");
            await _vectorizationTestService.DeleteAppConfiguration(accountNameAppConfigKey);

            WriteLine($"Delete the App Configuration key {authenticationTypeAppConfigKey}");
            await _vectorizationTestService.DeleteAppConfiguration(authenticationTypeAppConfigKey);

            WriteLine($"Teardown: Delete data source {dataSourceName} via the Management API");
            await _vectorizationTestService.DeleteDataSource(dataSourceName);

            WriteLine($"Teardown: Delete text partitioning profile {textPartitionProfileName} via the Management API");
            await _vectorizationTestService.DeleteTextPartitioningProfile(textPartitionProfileName);

            WriteLine($"Teardown: Delete text embedding profile {textEmbeddingProfileName} via the Management API");
            await _vectorizationTestService.DeleteTextEmbeddingProfile(textEmbeddingProfileName);
            WriteLine($"Teardown: Delete text embedding profile {genericTextEmbeddingProfileName} via the Management API");
            await _vectorizationTestService.DeleteTextEmbeddingProfile(genericTextEmbeddingProfileName);

            //indexing profile
            //remove search index
            //remove indexing profile
            WriteLine($"Teardown: Delete indexing profile {indexingProfileName} via the Management API");
            await _vectorizationTestService.DeleteIndexingProfile(indexingProfileName, true);

            //indexing profile
            //remove search index
            //remove indexing profile           
            
        }
	}
}
