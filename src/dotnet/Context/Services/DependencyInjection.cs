using FluentValidation;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Models.CodeExecution;
using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Services.Storage;
using FoundationaLLM.Context.Interfaces;
using FoundationaLLM.Context.Models.Configuration;
using FoundationaLLM.Context.Services;
using FoundationaLLM.Context.Services.CosmosDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM
{
    /// <summary>
    /// Provides dependency injection extensions for the FoundationaLLM Context service.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Registers the <see cref="IFileService>"/> to the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder.</param>
        public static void AddFileService(this IHostApplicationBuilder builder) =>
            builder.Services.AddFileService(builder.Configuration);

        /// <summary>
        /// Registers the <see cref="IFileService>"/> to the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddFileService(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<ContextServiceSettings>()
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_ContextAPI_Configuration));

            services.AddScoped<IFileService, FileService>(sp =>
                new FileService(
                    cosmosDBService: sp.GetRequiredService<IAzureCosmosDBFileService>(),
                    storageService: new BlobStorageService(
                        Options.Create<BlobStorageServiceSettings>(
                            sp.GetRequiredService<IOptions<ContextServiceSettings>>().Value.FileService.Storage),
                        sp.GetRequiredService<ILogger<BlobStorageService>>()),
                    settings: sp.GetRequiredService<IOptions<ContextServiceSettings>>().Value.FileService,
                    logger: sp.GetRequiredService<ILogger<FileService>>()));
        }

        /// <summary>
        /// Registers the <see cref="ICodeSessionService"/> implementation with the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder.</param>
        public static void AddCodeSessionService(this IHostApplicationBuilder builder) =>
            builder.Services.AddCodeSessionService(builder.Configuration);

        /// <summary>
        /// Registers the <see cref="ICodeSessionService"/> implementation with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddCodeSessionService(this IServiceCollection services, IConfiguration configuration)
        {
            // Register validators.
            services.AddSingleton<IValidator<CreateCodeSessionRequest>, CreateCodeSessionRequestValidator>();
            services.AddSingleton<IValidator<CodeSessionFileUploadRequest>, CodeSessionFileUploadRequestValidator>();
            services.AddSingleton<IValidator<CodeSessionFileDownloadRequest>, CodeSessionFileDownloadRequestValidator>();

            services.AddScoped<ICodeSessionService, CodeSessionService>();
        }

        /// <summary>
        /// Registers the Azure Cosmos DB services used by the Context API to the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder.</param>
        public static void AddAzureCosmosDBContextServices(this IHostApplicationBuilder builder) =>
            builder.Services.AddAzureCosmosDBContextServices(builder.Configuration);

        /// <summary>
        /// Registers the Azure Cosmos DB services used by the Context API to the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddAzureCosmosDBContextServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<ContextServiceSettings>()
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_ContextAPI_Configuration));

            services.AddSingleton<IAzureCosmosDBServiceBase, AzureCosmosDBServiceBase>(sp =>
                new AzureCosmosDBServiceBase(
                    options: Options.Create<AzureCosmosDBSettings>(
                        sp.GetRequiredService<IOptions<ContextServiceSettings>>().Value.FileService.CosmosDB),
                    logger: sp.GetRequiredService<ILogger<AzureCosmosDBServiceBase>>()));
            services.ActivateSingleton<IAzureCosmosDBServiceBase>();

            services.AddSingleton<IAzureCosmosDBFileService, AzureCosmosDBFileService>();
            services.ActivateSingleton<IAzureCosmosDBFileService>();

            services.AddSingleton<IAzureCosmosDBCodeSessionService, AzureCosmosDBCodeSessionService>();
            services.ActivateSingleton<IAzureCosmosDBCodeSessionService>();
        }

        /// <summary>
        /// Registers the <see cref="ICodeSessionProviderService"/> implementations with the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder.</param>
        public static void AddAzureContainerAppsCodeSessionProviderServices(this IHostApplicationBuilder builder) =>
            builder.Services.AddAzureContainerAppsCodeSessionProviderServices(builder.Configuration);

        /// <summary>
        /// Registers the <see cref="ICodeSessionProviderService"/> implementations with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddAzureContainerAppsCodeSessionProviderServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<AzureContainerAppsCodeInterpreterServiceSettings>()
                .Bind(configuration.GetSection(
                    AppConfigurationKeys.FoundationaLLM_Code_CodeExecution_AzureContainerAppsDynamicSessions_CodeInterpreter));

            services.AddSingleton<ICodeSessionProviderService, AzureContainerAppsCodeInterpreterService>();

            services.AddOptions<AzureContainerAppsCustomContainerServiceSettings>()
                .Bind(configuration.GetSection(
                    AppConfigurationKeys.FoundationaLLM_Code_CodeExecution_AzureContainerAppsDynamicSessions_CustomContainer));

            services.AddSingleton<ICodeSessionProviderService, AzureContainerAppsCustomContainerService>();

            services.ActivateSingleton<ICodeSessionProviderService>();
        }
    }
}
