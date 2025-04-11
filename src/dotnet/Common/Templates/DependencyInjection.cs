// -------------------------------------------------------------------------------
//
// WARNING!
// This file is auto-generated based on the AppConfiguration.json file.
// Do not make changes to this file, as they will be automatically overwritten.
//
// -------------------------------------------------------------------------------
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Services.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM
{
    /// <summary>
    /// Dependency injection extensions for resource provider storage services.
    /// </summary>
    public static partial class DependencyInjection
    {        
        /// <summary>
        /// Add the named <see cref="IStorageService"/> implementation for the FoundationaLLM.AIModel resource provider.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void AddAIModelResourceProviderStorage(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_ResourceProviders_AIModel)
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_ResourceProviders_AIModel_Storage));

            builder.Services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_AIModel);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_ResourceProviders_AIModel
                };
            });
        }

        /// <summary>
        /// Add the named <see cref="IStorageService"/> implementation for the FoundationaLLM.AIModel resource provider.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddAIModelResourceProviderStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_ResourceProviders_AIModel)
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_ResourceProviders_AIModel_Storage));

            services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_AIModel);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_ResourceProviders_AIModel
                };
            });
        }
        
        /// <summary>
        /// Add the named <see cref="IStorageService"/> implementation for the FoundationaLLM.Agent resource provider.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void AddAgentResourceProviderStorage(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Agent)
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_ResourceProviders_Agent_Storage));

            builder.Services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Agent);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Agent
                };
            });
        }

        /// <summary>
        /// Add the named <see cref="IStorageService"/> implementation for the FoundationaLLM.Agent resource provider.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddAgentResourceProviderStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Agent)
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_ResourceProviders_Agent_Storage));

            services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Agent);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Agent
                };
            });
        }
        
        /// <summary>
        /// Add the named <see cref="IStorageService"/> implementation for the FoundationaLLM.Attachment resource provider.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void AddAttachmentResourceProviderStorage(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Attachment)
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_ResourceProviders_Attachment_Storage));

            builder.Services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Attachment);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Attachment
                };
            });
        }

        /// <summary>
        /// Add the named <see cref="IStorageService"/> implementation for the FoundationaLLM.Attachment resource provider.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddAttachmentResourceProviderStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Attachment)
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_ResourceProviders_Attachment_Storage));

            services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Attachment);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Attachment
                };
            });
        }
        
        /// <summary>
        /// Add the named <see cref="IStorageService"/> implementation for the FoundationaLLM.Configuration resource provider.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void AddConfigurationResourceProviderStorage(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Configuration)
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_ResourceProviders_Configuration_Storage));

            builder.Services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Configuration);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Configuration
                };
            });
        }

        /// <summary>
        /// Add the named <see cref="IStorageService"/> implementation for the FoundationaLLM.Configuration resource provider.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddConfigurationResourceProviderStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Configuration)
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_ResourceProviders_Configuration_Storage));

            services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Configuration);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Configuration
                };
            });
        }
        
        /// <summary>
        /// Add the named <see cref="IStorageService"/> implementation for the FoundationaLLM.DataSource resource provider.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void AddDataSourceResourceProviderStorage(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_ResourceProviders_DataSource)
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_ResourceProviders_DataSource_Storage));

            builder.Services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_DataSource);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_ResourceProviders_DataSource
                };
            });
        }

        /// <summary>
        /// Add the named <see cref="IStorageService"/> implementation for the FoundationaLLM.DataSource resource provider.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddDataSourceResourceProviderStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_ResourceProviders_DataSource)
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_ResourceProviders_DataSource_Storage));

            services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_DataSource);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_ResourceProviders_DataSource
                };
            });
        }
        
        /// <summary>
        /// Add the named <see cref="IStorageService"/> implementation for the FoundationaLLM.Prompt resource provider.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void AddPromptResourceProviderStorage(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Prompt)
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_ResourceProviders_Prompt_Storage));

            builder.Services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Prompt);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Prompt
                };
            });
        }

        /// <summary>
        /// Add the named <see cref="IStorageService"/> implementation for the FoundationaLLM.Prompt resource provider.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddPromptResourceProviderStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Prompt)
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_ResourceProviders_Prompt_Storage));

            services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Prompt);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Prompt
                };
            });
        }
        
        /// <summary>
        /// Add the named <see cref="IStorageService"/> implementation for the FoundationaLLM.Vectorization resource provider.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void AddVectorizationResourceProviderStorage(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Vectorization)
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_ResourceProviders_Vectorization_Storage));

            builder.Services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Vectorization);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Vectorization
                };
            });
        }

        /// <summary>
        /// Add the named <see cref="IStorageService"/> implementation for the FoundationaLLM.Vectorization resource provider.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddVectorizationResourceProviderStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Vectorization)
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_ResourceProviders_Vectorization_Storage));

            services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Vectorization);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Vectorization
                };
            });
        }
        
        /// <summary>
        /// Add the named <see cref="IStorageService"/> implementation for the FoundationaLLM.DataPipeline resource provider.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void AddDataPipelineResourceProviderStorage(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_ResourceProviders_DataPipeline)
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_ResourceProviders_DataPipeline_Storage));

            builder.Services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_DataPipeline);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_ResourceProviders_DataPipeline
                };
            });
        }

        /// <summary>
        /// Add the named <see cref="IStorageService"/> implementation for the FoundationaLLM.DataPipeline resource provider.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddDataPipelineResourceProviderStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_ResourceProviders_DataPipeline)
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_ResourceProviders_DataPipeline_Storage));

            services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_DataPipeline);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_ResourceProviders_DataPipeline
                };
            });
        }
        
        /// <summary>
        /// Add the named <see cref="IStorageService"/> implementation for the FoundationaLLM.Plugin resource provider.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        public static void AddPluginResourceProviderStorage(this IHostApplicationBuilder builder)
        {
            builder.Services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Plugin)
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_ResourceProviders_Plugin_Storage));

            builder.Services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Plugin);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Plugin
                };
            });
        }

        /// <summary>
        /// Add the named <see cref="IStorageService"/> implementation for the FoundationaLLM.Plugin resource provider.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> application configuration provider.</param>
        public static void AddPluginResourceProviderStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<BlobStorageServiceSettings>(
                DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Plugin)
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_ResourceProviders_Plugin_Storage));

            services.AddSingleton<IStorageService, BlobStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptionsMonitor<BlobStorageServiceSettings>>()
                    .Get(DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Plugin);
                var logger = sp.GetRequiredService<ILogger<BlobStorageService>>();

                return new BlobStorageService(
                    Options.Create<BlobStorageServiceSettings>(settings),
                    logger)
                {
                    InstanceName = DependencyInjectionKeys.FoundationaLLM_ResourceProviders_Plugin
                };
            });
        }
    }
}
