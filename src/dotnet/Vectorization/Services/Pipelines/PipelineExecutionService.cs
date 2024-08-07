using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Constants.ResourceProviders;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Models.ResourceProviders;
using FoundationaLLM.Common.Models.ResourceProviders.DataSource;
using FoundationaLLM.Common.Models.ResourceProviders.Vectorization;
using FoundationaLLM.Common.Models.Vectorization;
using FoundationaLLM.Vectorization.Extensions;
using FoundationaLLM.Vectorization.Interfaces;
using FoundationaLLM.Vectorization.Models;
using FoundationaLLM.Vectorization.Services.DataSources;
using FoundationaLLM.Vectorization.ResourceProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Configuration;
using FoundationaLLM.Vectorization.Services.DataSources.Configuration.SQLDatabase;
using FoundationaLLM.Common.Constants.Authentication;

namespace FoundationaLLM.Vectorization.Services.Pipelines
{
    /// <summary>
    /// Executes active vectorization data pipelines.
    /// </summary>
    /// <param name="configuration">The global configuration provider.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> providing dependency injection services..</param>
    /// <param name="resourceProviderServices">The list of resurce providers registered with the main dependency injection container.</param>
    /// <param name="loggerFactory">Factory responsible for creating loggers.</param>
    /// <param name="logger">The <see cref="ILogger"/> used for logging.</param>
    public class PipelineExecutionService(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        IEnumerable<IResourceProviderService> resourceProviderServices,
        ILoggerFactory loggerFactory,
        ILogger<PipelineExecutionService> logger) : IPipelineExecutionService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<PipelineExecutionService> _logger = logger;
        private readonly ILoggerFactory _loggerFactory = loggerFactory;
        private readonly Dictionary<string, IResourceProviderService> _resourceProviderServices =
            resourceProviderServices.ToDictionary<IResourceProviderService, string>(
                rps => rps.Name);

        /// <inheritdoc/>
        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (!_resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_Vectorization, out var vectorizationResourceProviderService))
            {
                _logger.LogError($"Could not retrieve the {ResourceProviderNames.FoundationaLLM_Vectorization} resource provider.");
                return;
            }
            //cast for extension methods
            var vectorizationResourceProvider = (VectorizationResourceProviderService)vectorizationResourceProviderService;

            if (!_resourceProviderServices.TryGetValue(ResourceProviderNames.FoundationaLLM_DataSource, out var dataSourceResourceProvider))
            {
                _logger.LogError($"Could not retrieve the {ResourceProviderNames.FoundationaLLM_DataSource} resource provider.");
                return;
            }

            var stateService = _serviceProvider.GetRequiredService<IVectorizationStateService>();
            if (stateService is null)
            {
                _logger.LogError("Could not retrieve the vectorization state service.");
                return;
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if(!vectorizationResourceProvider.IsInitialized)
                    {
                        _logger.LogInformation("Vectorization resource provider has not finished initializing.");
                        await Task.Delay(TimeSpan.FromSeconds(5));
                        continue;
                    }
                    var activePipelines = await vectorizationResourceProvider.GetActivePipelines();

                    foreach (var activePipeline in activePipelines)
                    {
                        //deactivate the pipeline bing processed.
                        await vectorizationResourceProvider.TogglePipelineActivation(activePipeline.ObjectId!, false);

                        // initialize pipeline execution state
                        var pipelineExecutionId = Guid.NewGuid().ToString();
                        var pipelineName = activePipeline.Name;
                        var pipelineState = new VectorizationPipelineState
                        {
                            ExecutionId = pipelineExecutionId,
                            PipelineObjectId = activePipeline.ObjectId!,
                            ExecutionStart = DateTime.UtcNow,
                            ProcessingState = VectorizationProcessingState.InProgress
                        };

                        await stateService.SavePipelineState(pipelineState);

                        try
                        {
                            _logger.LogInformation($"Executing pipeline {pipelineName} with execution ID {pipelineExecutionId}.");

                            var dataSource = await GetResource<DataSourceBase>(
                                activePipeline.DataSourceObjectId,
                                DataSourceResourceTypeNames.DataSources,
                                dataSourceResourceProvider);
                            var textPartitioningProfile = await GetResource<TextPartitioningProfile>(
                                activePipeline.TextPartitioningProfileObjectId,
                                VectorizationResourceTypeNames.TextPartitioningProfiles,
                                vectorizationResourceProvider);
                            var textEmbeddingProfile = await GetResource<TextEmbeddingProfile>(
                                activePipeline.TextEmbeddingProfileObjectId,
                                VectorizationResourceTypeNames.TextEmbeddingProfiles,
                                vectorizationResourceProvider);
                            var indexingProfile = await GetResource<IndexingProfile>(
                                activePipeline.IndexingProfileObjectId,
                                VectorizationResourceTypeNames.IndexingProfiles,
                                vectorizationResourceProvider);

                            if (dataSource is null)
                            {
                                continue;
                            }
                            switch (dataSource.Type)
                            {
                                case DataSourceTypes.AzureDataLake:
                                    // resolve configuration references
                                    var blobStorageServiceSettings = new BlobStorageServiceSettings { AuthenticationType = AuthenticationTypes.Unknown };
                                    _configuration.Bind(
                                        $"{AppConfigurationKeySections.FoundationaLLM_DataSources}:{dataSource.Name}",
                                        blobStorageServiceSettings);

                                    AzureDataLakeDataSourceService svc = new AzureDataLakeDataSourceService(
                                                                        (AzureDataLakeDataSource)dataSource!,
                                                                        blobStorageServiceSettings,
                                                                        _loggerFactory);

                                    if (string.IsNullOrEmpty(blobStorageServiceSettings.AccountName))
                                    {
                                        // extract the account from the connection string
                                        var accountName = blobStorageServiceSettings.ConnectionString!.Split(';')
                                            .FirstOrDefault(s => s.StartsWith("AccountName="))?.Split('=')[1];
                                        blobStorageServiceSettings.AccountName = accountName;
                                    }

                                    var files = await svc.GetFilesListAsync();
                                    var firstMultipartToken = $"{blobStorageServiceSettings.AccountName}.dfs.core.windows.net";
                                    if (blobStorageServiceSettings.AccountName!.Equals("onelake"))
                                    {
                                        firstMultipartToken = $"{blobStorageServiceSettings.AccountName}.dfs.fabric.microsoft.com";
                                    }
                                    foreach (var file in files)
                                    {
                                        //first token is the container name
                                        var containerName = file.Split("/")[0];
                                        //remove the first token from the path
                                        var path = file.Substring(file.IndexOf('/') + 1);
                                        //path minus the file extension
                                        var canonical = path.Substring(0, path.LastIndexOf('.'));
                                        var vectorizationRequest = new VectorizationRequest()
                                        {
                                            Name = Guid.NewGuid().ToString(),
                                            PipelineExecutionId = pipelineExecutionId,
                                            PipelineObjectId = activePipeline.ObjectId!,
                                            PipelineName = activePipeline.Name,
                                            CostCenter = activePipeline.CostCenter,
                                            ContentIdentifier = new ContentIdentifier()
                                            {
                                                DataSourceObjectId = dataSource.ObjectId!,
                                                MultipartId = new List<string> { firstMultipartToken, containerName, path },
                                                CanonicalId = canonical
                                            },
                                            ProcessingType = VectorizationProcessingType.Asynchronous,
                                            ProcessingState = VectorizationProcessingState.New,
                                            Steps = new List<VectorizationStep>()
                                            {
                                                new VectorizationStep()
                                                {
                                                    Id = VectorizationSteps.Extract,
                                                    Parameters = new Dictionary<string, string>()
                                                },
                                                new VectorizationStep()
                                                {
                                                    Id = VectorizationSteps.Partition,
                                                    Parameters = new Dictionary<string, string>()
                                                    {
                                                        {"text_partitioning_profile_name", textPartitioningProfile.Name }
                                                    }
                                                },
                                                new VectorizationStep()
                                                {
                                                    Id = VectorizationSteps.Embed,
                                                    Parameters = new Dictionary<string, string>()
                                                    {
                                                        {"text_embedding_profile_name", textEmbeddingProfile.Name }
                                                    }
                                                },
                                                new VectorizationStep()
                                                {
                                                    Id = VectorizationSteps.Index,
                                                    Parameters = new Dictionary<string, string>()
                                                    {
                                                        {"indexing_profile_name", indexingProfile.Name }
                                                    }
                                                }
                                            },
                                            CompletedSteps = [],
                                            RemainingSteps = ["extract", "partition", "embed", "index"]
                                        };

                                        // submit the vectorization request, if an error occurs on a single file, record it and continue with the next file.
                                        // this does not result in the failure of the entire pipeline.
                                        try
                                        {
                                            //create the vectorization request
                                            await vectorizationRequest.UpdateVectorizationRequestResource(vectorizationResourceProvider);
                                            pipelineState.VectorizationRequestObjectIds.Add(vectorizationRequest.ObjectId!);
                                            //issue process action on the created vectorization request
                                            await vectorizationRequest.ProcessVectorizationRequest(vectorizationResourceProvider);
                                        }
                                        catch (Exception ex)
                                        {
                                            var errorMessage = $"An error was encountered while creating the vectorization request for file: {string.Join('/', vectorizationRequest.ContentIdentifier.MultipartId)}, exception: {ex.Message}";
                                            _logger.LogError(ex, errorMessage);                                            
                                            pipelineState.ErrorMessages.Add(errorMessage);
                                            
                                        }
                                        finally
                                        {
                                            await stateService.SavePipelineState(pipelineState);
                                        }

                                    }
                                    break;
                                case DataSourceTypes.AzureSQLDatabase:
                                    var sqlDataSourceServiceSettings = new SQLDatabaseServiceSettings { ConnectionString = String.Empty };
                                    _configuration.Bind(
                                        $"{AppConfigurationKeySections.FoundationaLLM_DataSources}:{dataSource.Name}",
                                        sqlDataSourceServiceSettings);
                                    AzureSQLDatabaseDataSourceService sqlSvc = new AzureSQLDatabaseDataSourceService(
                                        (AzureSQLDatabaseDataSource)dataSource!,
                                        sqlDataSourceServiceSettings,
                                        _loggerFactory);
                                    List<List<string>> multipartIds = new List<List<string>>();
                                    if (!String.IsNullOrWhiteSpace(sqlDataSourceServiceSettings.MultiPartQuery))
                                    {
                                        var delimitedMultipartIds = await sqlSvc.ExecuteMultipartQueryAsync(cancellationToken);
                                        foreach (var delimitedMultipartId in delimitedMultipartIds)
                                        {
                                            multipartIds.Add(delimitedMultipartId.Split('|').ToList());
                                        }
                                    }

                                    foreach (var multipartId in multipartIds)
                                    {
                                        var canonical = $"{dataSource.Name}/{string.Join('/', multipartId)}";
                                        var vectorizationRequest = new VectorizationRequest()
                                        {
                                            Name = Guid.NewGuid().ToString(),
                                            PipelineExecutionId = pipelineExecutionId,
                                            PipelineObjectId = activePipeline.ObjectId!,
                                            PipelineName = activePipeline.Name,
                                            CostCenter = activePipeline.CostCenter,
                                            ContentIdentifier = new ContentIdentifier()
                                            {
                                                DataSourceObjectId = dataSource.ObjectId!,
                                                MultipartId = multipartId,
                                                CanonicalId = canonical
                                            },
                                            ProcessingType = VectorizationProcessingType.Asynchronous,
                                            ProcessingState = VectorizationProcessingState.New,
                                            Steps = new List<VectorizationStep>()
                                            {
                                                new VectorizationStep()
                                                {
                                                    Id = VectorizationSteps.Extract,
                                                    Parameters = new Dictionary<string, string>()
                                                },
                                                new VectorizationStep()
                                                {
                                                    Id = VectorizationSteps.Partition,
                                                    Parameters = new Dictionary<string, string>()
                                                    {
                                                        {"text_partitioning_profile_name", textPartitioningProfile.Name }
                                                    }
                                                },
                                                new VectorizationStep()
                                                {
                                                    Id = VectorizationSteps.Embed,
                                                    Parameters = new Dictionary<string, string>()
                                                    {
                                                        {"text_embedding_profile_name", textEmbeddingProfile.Name }
                                                    }
                                                },
                                                new VectorizationStep()
                                                {
                                                    Id = VectorizationSteps.Index,
                                                    Parameters = new Dictionary<string, string>()
                                                    {
                                                        {"indexing_profile_name", indexingProfile.Name }
                                                    }
                                                }
                                            },
                                            CompletedSteps = [],
                                            RemainingSteps = ["extract", "partition", "embed", "index"]
                                        };

                                        // submit the vectorization request, if an error occurs on a single file, record it and continue with the next file.
                                        // this does not result in the failure of the entire pipeline.
                                        try
                                        {
                                            //create the vectorization request
                                            await vectorizationRequest.UpdateVectorizationRequestResource(vectorizationResourceProvider);
                                            pipelineState.VectorizationRequestObjectIds.Add(vectorizationRequest.ObjectId!);

                                            //issue process action on the created vectorization request
                                            var processResult = await vectorizationRequest.ProcessVectorizationRequest(vectorizationResourceProvider);
                                            if(processResult.IsSuccess==false)
                                            {
                                                vectorizationRequest.ProcessingState = VectorizationProcessingState.Failed;
                                                pipelineState.ErrorMessages.Add($"Error while submitting process action on vectorization request {vectorizationRequest.Name} in pipeline {pipelineName}: {processResult.ErrorMessage!}");
                                            }
                                            await vectorizationRequest.UpdateVectorizationRequestResource(vectorizationResourceProvider);
                                        }
                                        catch (Exception ex)
                                        {
                                            var errorMessage = $"An error was encountered while creating the vectorization request for file {string.Join('/', vectorizationRequest.ContentIdentifier.MultipartId)}, exception: {ex.Message}";
                                            _logger.LogError(ex, errorMessage);                                           
                                            pipelineState.ErrorMessages.Add(errorMessage);                                           
                                        }
                                    }
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"An error was encountered while activating or executing pipeline {activePipeline.Name}.");
                            //get latest state of the pipeline execution.
                            pipelineState = await stateService.ReadPipelineState(pipelineName, pipelineExecutionId);
                            pipelineState.ErrorMessages.Add($"An error was encountered while activating or executing pipeline {activePipeline.Name}: {ex.Message}.");                           
                        }
                        finally
                        {
                            if(pipelineState.VectorizationRequestObjectIds.Count == 0)
                            {
                                pipelineState.ProcessingState = VectorizationProcessingState.Completed;
                                pipelineState.ExecutionEnd = DateTime.UtcNow;
                            }                           
                            await stateService.SavePipelineState(pipelineState);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error was encountered while running the pipeline execution cycle.");
                }

                await Task.Delay(TimeSpan.FromSeconds(60));
            }
        }

        /// <summary>
        /// Retrieves a resource of the specified type from the resource provider service.
        /// </summary>
        /// <typeparam name="T">Type of the resource to retrieve.</typeparam>
        /// <param name="objectId">The object id/resource path of the resource to retrieve.</param>
        /// <param name="resourceTypeName">The type of resource.</param>
        /// <param name="resourceProviderService">The resource provider service.</param>
        /// <returns>The requested resource object.</returns>
        private static Task<T> GetResource<T>(string objectId, string resourceTypeName, IResourceProviderService resourceProviderService)
            where T : ResourceBase =>       
          Task.FromResult(resourceProviderService.GetResource<T>($"/{resourceTypeName}/{objectId.Split("/").Last()}"));
        

        private static bool CheckNextExecution(string? cronExpression)
        {
            if (string.IsNullOrWhiteSpace(cronExpression))
                return false;

            var cronSchedule = new CronExpression(cronExpression);
            cronSchedule.TimeZone = TimeZoneInfo.Utc;

            var currentDate = DateTime.UtcNow;
            return cronSchedule.IsSatisfiedBy(new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, currentDate.Minute, 0));
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken) =>
            await Task.CompletedTask;
    }
}
