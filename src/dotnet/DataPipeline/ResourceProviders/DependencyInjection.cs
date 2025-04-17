using FluentValidation;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.ResourceProviders;
using FoundationaLLM.Common.Models.Plugins;
using FoundationaLLM.Common.Models.ResourceProviders.DataPipeline;
using FoundationaLLM.Common.Validation.Plugins;
using FoundationaLLM.DataPipeline.Clients;
using FoundationaLLM.DataPipeline.Interfaces;
using FoundationaLLM.DataPipeline.ResourceProviders;
using FoundationaLLM.DataPipeline.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM
{
    /// <summary>
    /// Provides extension methods used to configure dependency injection.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Registers the FoundationaLLM DataPipeline resource provider with the Dependency Injection container.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <param name="remoteClient">The flag the controls which Data Pipeline API client should the resource provider use.
        /// Defaults to <c>true</c> which means the remote client will be used (HTTP calls to the Data Pipeline API).</param>
        public static void AddDataPipelineResourceProvider(
            this IHostApplicationBuilder builder)
        {
            builder.AddDataPipelineResourceProviderStorage();

            // Register validators.
            builder.Services.AddSingleton<IValidator<DataPipelineDefinition>, DataPipelineDefinitionValidator>();
            builder.Services.AddSingleton<IValidator<DataPipelineRun>, DataPipelineRunValidator>();
            builder.Services.AddSingleton<IValidator<PluginComponent>, PluginComponentValidator>();

            // Register the resource provider services (cannot use Keyed singletons due to the Microsoft Identity package being incompatible):
            builder.Services.AddSingleton<IResourceProviderService>(sp => 
                new DataPipelineResourceProviderService(                   
                    sp.GetRequiredService<IOptions<InstanceSettings>>(),
                    sp.GetRequiredService<IOptions<ResourceProviderCacheSettings>>(),
                    sp.GetRequiredService<IAuthorizationServiceClient>(),
                    sp.GetRequiredService<IDataPipelineServiceClient>(),
                    sp.GetRequiredService<IEnumerable<IStorageService>>()
                        .Single(s => s.InstanceName == DependencyInjectionKeys.FoundationaLLM_ResourceProviders_DataPipeline),
                    sp.GetRequiredService<IEventService>(),
                    sp.GetRequiredService<IResourceValidatorFactory>(),
                    sp,                    
                    sp.GetRequiredService<ILoggerFactory>()));          

            builder.Services.ActivateSingleton<IResourceProviderService>();
        }

        /// <summary>
        /// Registers the remote data pipeline service client with the Dependency Injection container.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void AddRemoteDataPipelineServiceClient(
            this IHostApplicationBuilder builder) =>
            builder.Services.AddSingleton<IDataPipelineServiceClient, RemoteDataPipelineServiceClient>();
    }
}
