using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.API;
using FoundationaLLM.Common.Models.Configuration.AzureAI;
using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Configuration.Storage;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Common.Services.API;
using FoundationaLLM.Common.Services.Storage;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Core.Examples.Exceptions;
using FoundationaLLM.Core.Examples.Interfaces;
using FoundationaLLM.Core.Examples.Models;
using FoundationaLLM.Core.Examples.Services;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Services;
using FoundationaLLM.SemanticKernel.Core.Models.Configuration;
using FoundationaLLM.SemanticKernel.Core.Services.Indexing;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationaLLM.Core.Examples.Setup
{
    public static class TestServicesInitializer
	{
		/// <summary>
		/// Configure base services and dependencies for the tests.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="configRoot"></param>
		public static void InitializeServices(
			IServiceCollection services,
			IConfigurationRoot configRoot)
		{
			TestConfiguration.Initialize(configRoot, services);

            services.AddOptions<BlobStorageServiceSettings>(
                    DependencyInjectionKeys.FoundationaLLM_ResourceProvider_Vectorization)
                .Bind(configRoot.GetSection("FoundationaLLM:Vectorization:ResourceProviderService:Storage"));

            RegisterInstance(services, configRoot);
            RegisterClientLibraries(services, configRoot);
			RegisterHttpClients(services, configRoot);
			RegisterCosmosDb(services, configRoot);
            RegisterAzureAIService(services, configRoot);
            RegisterLogging(services);
			RegisterServiceManagers(services);
            RegisterSearchIndex(services, configRoot);
        }

        private static void RegisterInstance(IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<InstanceSettings>()
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Instance));
        }

		private static void RegisterSearchIndex(IServiceCollection services, IConfiguration configuration)
		{
            services.AddOptions<AzureAISearchIndexingServiceSettings>()
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Vectorization_AzureAISearchIndexingService));

            services.AddKeyedSingleton<IIndexingService, AzureAISearchIndexingService>(
                DependencyInjectionKeys.FoundationaLLM_Vectorization_AzureAISearchIndexingService);

        }

        private static void RegisterClientLibraries(IServiceCollection services, IConfiguration configuration) =>
            services.AddCoreClient(
                configuration[AppConfigurationKeys.FoundationaLLM_APIs_CoreAPI_APIUrl]!,
                DefaultAuthentication.AzureCredential!);

        private static void RegisterHttpClients(IServiceCollection services, IConfiguration configuration)
		{
			services.Configure<HttpClientOptions>(HttpClients.CoreAPI, options =>
            {
                options.BaseUri = configuration[AppConfigurationKeys.FoundationaLLM_APIs_CoreAPI_APIUrl]!;
                options.Scope = configuration[AppConfigurationKeys.FoundationaLLM_Chat_Entra_Scopes]!;
                options.Timeout = TimeSpan.FromMinutes(40);
            });
            
            services.Configure<HttpClientOptions>(HttpClients.ManagementAPI, options =>
            {
                options.BaseUri = configuration[AppConfigurationKeys.FoundationaLLM_APIs_ManagementAPI_APIUrl]!;
                options.Scope = configuration[AppConfigurationKeys.FoundationaLLM_Management_Entra_Scopes]!;
                options.Timeout = TimeSpan.FromMinutes(5);
            });

            services.Configure<HttpClientOptions>(HttpClients.VectorizationAPI, options =>
            {
                options.BaseUri = configuration[AppConfigurationKeys.FoundationaLLM_APIs_VectorizationAPI_APIUrl]!;
                options.Timeout = TimeSpan.FromMinutes(10);
            });

            var vectorizationAPISettings = new DownstreamAPIClientConfiguration
            {
                APIUrl = configuration[AppConfigurationKeys.FoundationaLLM_APIs_VectorizationAPI_APIUrl]!,
                APIKey = configuration[AppConfigurationKeys.FoundationaLLM_APIs_VectorizationAPI_APIKey]!,
                Timeout = TimeSpan.FromMinutes(10)
            };
            var downstreamAPISettings = new DownstreamAPISettings
            {
                DownstreamAPIs = []
            };

            downstreamAPISettings.DownstreamAPIs[HttpClients.VectorizationAPI] = vectorizationAPISettings;
            
            services.AddHttpClient(HttpClients.CoreAPI)
                .ConfigureHttpClient((serviceProvider, client) =>
                {
                    var options = serviceProvider.GetRequiredService<IOptionsSnapshot<HttpClientOptions>>().Get(HttpClients.CoreAPI);
                    client.BaseAddress = new Uri(options.BaseUri!);
                    if (options.Timeout != null) client.Timeout = (TimeSpan)options.Timeout;
                })
                .AddResilienceHandler("DownstreamPipeline", static strategyBuilder =>
                {
                    CommonHttpRetryStrategyOptions.GetCommonHttpRetryStrategyOptions();
                });

            services.AddHttpClient(HttpClients.ManagementAPI)
                .ConfigureHttpClient((serviceProvider, client) =>
                {
                    var options = serviceProvider.GetRequiredService<IOptionsSnapshot<HttpClientOptions>>().Get(HttpClients.ManagementAPI);
                    client.BaseAddress = new Uri(options.BaseUri!);                   
                    if (options.Timeout != null) client.Timeout = (TimeSpan)options.Timeout;
                })
                .AddResilienceHandler("DownstreamPipeline", static strategyBuilder =>
                {
                    CommonHttpRetryStrategyOptions.GetCommonHttpRetryStrategyOptions();
                });

            services.AddHttpClient(HttpClients.VectorizationAPI)
                 .ConfigureHttpClient((serviceProvider, client) =>
                 {
                     var options = serviceProvider.GetRequiredService<IOptionsSnapshot<HttpClientOptions>>().Get(HttpClients.VectorizationAPI);
                     client.DefaultRequestHeaders.Add("X-API-KEY", vectorizationAPISettings.APIKey);
                     client.BaseAddress = new Uri(options.BaseUri!);                     
                     if (options.Timeout != null) client.Timeout = (TimeSpan)options.Timeout;
                 })
                 .AddResilienceHandler("DownstreamPipeline", static strategyBuilder =>
                 {
                     CommonHttpRetryStrategyOptions.GetCommonHttpRetryStrategyOptions();
                 });

            services.AddSingleton<IDownstreamAPISettings>(downstreamAPISettings);

            services.AddScoped<IDownstreamAPIService, DownstreamAPIService>((serviceProvider)
                => new DownstreamAPIService(HttpClients.VectorizationAPI, serviceProvider.GetService<IHttpClientFactoryService>()!));

            services.Configure<DownstreamAPISettings>(configuration.GetSection("DownstreamAPIs"));
        }

		private static void RegisterCosmosDb(IServiceCollection services, IConfiguration configuration)
		{
			services.AddOptions<CosmosDbSettings>()
				.Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_CosmosDB));

			services.AddSingleton<CosmosClient>(serviceProvider =>
			{
				var settings = serviceProvider.GetRequiredService<IOptions<CosmosDbSettings>>().Value;
				return new CosmosClientBuilder(settings.Endpoint, DefaultAuthentication.AzureCredential)
					.WithSerializerOptions(new CosmosSerializationOptions
					{
						PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
					})
					.WithConnectionModeGateway()
					.Build();
			});

			services.AddScoped<ICosmosDbService, CosmosDbService>();
		}

		private static void RegisterAzureAIService(IServiceCollection services, IConfiguration configuration)
		{
            try
            {
                var completionQualityMeasurementConfiguration = TestConfiguration.CompletionQualityMeasurementConfiguration;
                if (completionQualityMeasurementConfiguration is { AgentPrompts: not null })
                {
                    services.AddOptions<AzureAISettings>()
                        .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_AzureAIStudio));
                    services.AddOptions<BlobStorageServiceSettings>()
                        .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_AzureAIStudio_BlobStorageServiceSettings));

                    services.AddScoped<IAzureAIService, AzureAIService>();
                    services.AddSingleton<IStorageService, BlobStorageService>();
                }
                else
                {
                    Console.WriteLine($"Skipping Azure AI Service initialization. No agent prompts defined in the {nameof(CompletionQualityMeasurementConfiguration)} configuration section.");
                }
            }
            catch (ConfigurationNotFoundException cex)
            {
                Console.WriteLine($"Skipping Azure AI Service initialization. {cex.Message}");
            }
		}

		private static void RegisterLogging(IServiceCollection services)
		{
			services.AddLogging(builder =>
			{
				builder.AddConsole();
			});
		}

        private static void RegisterServiceManagers(IServiceCollection services)
        {
            services.AddScoped<ICoreAPITestManager, CoreAPITestManager>();
			services.AddScoped<IManagementAPITestManager, ManagementAPITestManager>();
            services.AddScoped<IAuthenticationService, MicrosoftEntraIDAuthenticationService>();
            services.AddScoped<IHttpClientManager, HttpClientManager>();
			services.AddScoped<IAgentConversationTestService, AgentConversationTestService>();
            services.AddScoped<IVectorizationTestService, VectorizationTestService>();
        }
    }
}
