using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.DataPipeline.Clients;
using FoundationaLLM.DataPipeline.Interfaces;
using FoundationaLLM.DataPipelineEngine.Clients;
using FoundationaLLM.DataPipelineEngine.Interfaces;
using FoundationaLLM.DataPipelineEngine.Models.Configuration;
using FoundationaLLM.DataPipelineEngine.Services;
using FoundationaLLM.DataPipelineEngine.Services.CosmosDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM
{
    /// <summary>
    /// Provides dependency injection extensions for the FoundationaLLM Data Pipeline service.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Registers the <see cref="IDataPipelineService>"/> to the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder.</param>
        public static void AddDataPipelineService(this IHostApplicationBuilder builder) =>
            builder.Services.AddDataPipelineService(builder.Configuration);

        /// <summary>
        /// Registers the <see cref="IDataPipelineService>"/> to the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddDataPipelineService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<DataPipelineServiceSettings>()
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_DataPipelineAPI_Configuration));

            services.AddScoped<IDataPipelineService, DataPipelineService>(sp =>
                new DataPipelineService(
                    resourceProviders: sp.GetRequiredService<IEnumerable<IResourceProviderService>>(),
                    settings: sp.GetRequiredService<IOptions<DataPipelineServiceSettings>>().Value,
                    logger: sp.GetRequiredService<ILogger<DataPipelineService>>()));
        }

        /// <summary>
        /// Registers the Azure Cosmos DB service used by the Data Pipeline API to the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder.</param>
        public static void AddAzureCosmosDBDataPipelineService(this IHostApplicationBuilder builder) =>
            builder.Services.AddAzureCosmosDBDataPipelineService(builder.Configuration);

        /// <summary>
        /// Registers the Azure Cosmos DB service used by the DataPipeline API to the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddAzureCosmosDBDataPipelineService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<DataPipelineServiceSettings>()
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_DataPipelineAPI_Configuration));

            services.AddSingleton<IAzureCosmosDBDataPipelineService, AzureCosmosDBDataPipelineService>(sp =>
                new AzureCosmosDBDataPipelineService(
                    options: Options.Create<AzureCosmosDBSettings>(
                        sp.GetRequiredService<IOptions<DataPipelineServiceSettings>>().Value.CosmosDB),
                    logger: sp.GetRequiredService<ILogger<AzureCosmosDBDataPipelineService>>()));
            services.ActivateSingleton<IAzureCosmosDBDataPipelineService>();
        }

        /// <summary>
        /// Registers the Data Pipeline Trigger service used by the Data Pipeline API to the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder.</param>
        public static void AddDataPipelineTriggerService(this IHostApplicationBuilder builder) =>
            builder.Services.AddDataPipelineTriggerService();

        /// <summary>
        /// Registers the Data Pipeline Trigger service used by the Data Pipeline API to the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        public static void AddDataPipelineTriggerService(this IServiceCollection services)
        {
            services.AddSingleton<IDataPipelineTriggerService, DataPipelineTriggerService>();
            services.ActivateSingleton<IDataPipelineTriggerService>();
        }

        /// <summary>
        /// Registers the Data Pipeline State service used by the Data Pipeline API to the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder.</param>
        public static void AddDataPipelineStateService(this IHostApplicationBuilder builder) =>
            builder.Services.AddDataPipelineStateService();

        /// <summary>
        /// Registers the Data Pipeline State service used by the Data Pipeline API to the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        public static void AddDataPipelineStateService(this IServiceCollection services)
        {
            services.AddSingleton<IDataPipelineStateService, DataPipelineStateService>();
            services.ActivateSingleton<IDataPipelineStateService>();
        }

        /// <summary>
        /// Registers the local data pipeline service client with the Dependency Injection container.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void AddLocalDataPipelineServiceClient(
            this IHostApplicationBuilder builder) =>
            builder.Services.AddLocalDataPipelineServiceClient();

        /// <summary>
        /// Registers the local data pipeline service client with the Dependency Injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        public static void AddLocalDataPipelineServiceClient(
            this IServiceCollection services) =>
            services.AddSingleton<IDataPipelineServiceClient, LocalDataPipelineServiceClient>();
    }
}
