using Azure.Monitor.OpenTelemetry.AspNetCore;
using Azure.Monitor.OpenTelemetry.Exporter;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Common.Services.API;
using FoundationaLLM.Common.Services.Azure;
using FoundationaLLM.Common.Services.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Cosmos;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.ResourceDetectors.Azure;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using FoundationaLLM.Common.Constants.Configuration;

namespace FoundationaLLM
{
    /// <summary>
    /// General purpose dependency injection extensions.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Configures logging defaults.
        /// </summary>
        /// <param name="builder">The host application builder.</param>
        public static void AddLogging(this IHostApplicationBuilder builder) =>
            builder.Services.AddLogging(config =>
            {
                // clear out default configuration
                config.ClearProviders();

                config.AddConfiguration(builder.Configuration.GetSection("Logging"));
                config.AddDebug();
                config.AddEventSourceLogger();

                //get the log level
                string logLevel = builder.Configuration["Logging:LogLevel:Default"];

                //enable console for debug or trace
                switch (logLevel)
                {
                    case "Trace":
                    case "Debug":
                        config.AddConsole();
                        break;
                }
            });

        /// <summary>
        /// Configures logging defaults.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfigurationManager"/> application configuration manager.</param>
        public static void AddLogging(this IServiceCollection services, IConfigurationManager configuration) =>
            services.AddLogging(config =>
            {
                // clear out default configuration
                config.ClearProviders();

                config.AddConfiguration(configuration.GetSection("Logging"));
                config.AddDebug();
                config.AddEventSourceLogger();

                //get the log level
                string logLevel = configuration["Logging:LogLevel:Default"];

                //enable console for debug or trace
                switch (logLevel)
                {
                    case "Trace":
                    case "Debug":
                        config.AddConsole();
                        break;
                }
            });


        /// <summary>
        /// Add CORS policies the the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        public static void AddCorsPolicies(this IHostApplicationBuilder builder) =>
            builder.Services.AddCors(policyBuilder =>
                {
                    policyBuilder.AddPolicy(CorsPolicyNames.AllowAllOrigins,
                        policy =>
                        {
                            policy.AllowAnyOrigin();
                            policy.WithHeaders("DNT", "Keep-Alive", "User-Agent", "X-Requested-With", "If-Modified-Since",
                                "Cache-Control", "Content-Type", "Range", "Authorization", "X-User-Identity",
                                "Access-Control-Request-Headers");
                            policy.AllowAnyMethod();
                        });
                });

        /// <summary>
        /// Adds OpenTelemetry the the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        /// <param name="connectionStringConfigurationKey">The configuration key for the OpenTelemetry connection string.</param>
        /// <param name="serviceName">The name of the service.</param>
        public static void AddOpenTelemetry(this IHostApplicationBuilder builder,
            string connectionStringConfigurationKey,
            string serviceName)
        {
            AzureMonitorOptions options = new AzureMonitorOptions { ConnectionString = builder.Configuration[connectionStringConfigurationKey] };

            var resourceBuilder = ResourceBuilder.CreateDefault();

            // Create a dictionary of resource attributes.
            var resourceAttributes = new Dictionary<string, object> {
                     { "service.name", serviceName },
                     { "service.namespace", "FoundationaLLM" },
                     { "service.version", builder.Configuration[EnvironmentVariables.FoundationaLLM_Version] },
                     { "service.instance.id", ValidatedEnvironment.MachineName }
                 };

            resourceBuilder.AddAttributes(resourceAttributes);

            builder.Services.AddOpenTelemetry()
             .WithTracing(b =>
             {
                 b
                 .SetResourceBuilder(resourceBuilder)
                 .AddSource("Azure.*")
                 .AddSource(serviceName)
                 //.AddConsoleExporter()
                 .AddAspNetCoreInstrumentation()
                 .AddHttpClientInstrumentation(o => o.FilterHttpRequestMessage = (_) =>
                 {
                     // Azure SDKs create their own client span before calling the service using HttpClient
                     // In this case, we would see two spans corresponding to the same operation
                     // 1) created by Azure SDK 2) created by HttpClient
                     // To prevent this duplication we are filtering the span from HttpClient
                     // as span from Azure SDK contains all relevant information needed.
                     var parentActivity = Activity.Current?.Parent;
                     if (parentActivity != null && parentActivity.Source.Name.Equals("Azure.Core.Http"))
                     {
                         return false;
                     }
                     return true;
                 })
                 .AddAzureMonitorTraceExporter(options => { options.ConnectionString = builder.Configuration[connectionStringConfigurationKey]; });
             });

            builder.Services.AddLogging(logging =>
            {
                // clear out default configuration
                logging.ClearProviders();

                logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
                logging.AddDebug();
                logging.AddEventSourceLogger();

                //get the log level
                string logLevel = builder.Configuration["Logging:LogLevel:Default"];

                //enable console for debug or trace
                switch (logLevel)
                {
                    case "Trace":
                    case "Debug":
                        logging.AddConsole();
                        break;
                }

                logging.AddOpenTelemetry(builderOptions =>
                {
                    builderOptions.SetResourceBuilder(resourceBuilder);
                    builderOptions.IncludeFormattedMessage = true;
                    builderOptions.IncludeScopes = false;
                    builderOptions.AddAzureMonitorLogExporter(options => { options.ConnectionString = builder.Configuration[connectionStringConfigurationKey]; });
                })
                .AddConfiguration(builder.Configuration);

                });
        }

        /// <summary>
        /// Adds OpenTelemetry the the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        /// <param name="connectionStringConfigurationKey">The configuration key for the OpenTelemetry connection string.</param>
        /// <param name="serviceName">The name of the service.</param>
        public static void AddOpenTelemetry(this IServiceCollection services, IConfigurationManager configuration,
            string connectionStringConfigurationKey,
            string serviceName)
        {
            AzureMonitorOptions options = new AzureMonitorOptions { ConnectionString = configuration[connectionStringConfigurationKey] };

            var resourceBuilder = ResourceBuilder.CreateDefault();

            // Create a dictionary of resource attributes.
            var resourceAttributes = new Dictionary<string, object> {
                     { "service.name", serviceName },
                     { "service.namespace", "FoundationaLLM" },
                     { "service.version", configuration[EnvironmentVariables.FoundationaLLM_Version] },
                     { "service.instance.id", ValidatedEnvironment.MachineName }
                 };

            resourceBuilder.AddAttributes(resourceAttributes);

            services.AddOpenTelemetry()
             .WithTracing(b =>
             {


                 b
                 .SetResourceBuilder(resourceBuilder)
                 .AddSource("Azure.*")
                 .AddSource(serviceName)
                 //.AddConsoleExporter()
                 .AddAspNetCoreInstrumentation()
                 .AddHttpClientInstrumentation(o => o.FilterHttpRequestMessage = (_) =>
                 {
                     // Azure SDKs create their own client span before calling the service using HttpClient
                     // In this case, we would see two spans corresponding to the same operation
                     // 1) created by Azure SDK 2) created by HttpClient
                     // To prevent this duplication we are filtering the span from HttpClient
                     // as span from Azure SDK contains all relevant information needed.
                     var parentActivity = Activity.Current?.Parent;
                     if (parentActivity != null && parentActivity.Source.Name.Equals("Azure.Core.Http"))
                     {
                         return false;
                     }
                     return true;
                 })
                 .AddAzureMonitorTraceExporter(options => { options.ConnectionString = configuration[connectionStringConfigurationKey]; });
             });

            services.AddLogging(logging =>
            {
                // clear out default configuration
                logging.ClearProviders();

                logging.AddConfiguration(configuration.GetSection("Logging"));
                logging.AddDebug();
                logging.AddEventSourceLogger();

                //get the log level
                string logLevel = configuration["Logging:LogLevel:Default"];

                //enable console for debug or trace
                switch (logLevel)
                {
                    case "Trace":
                    case "Debug":
                        logging.AddConsole();
                        break;
                }

                logging.AddOpenTelemetry(builderOptions =>
                {
                    builderOptions.SetResourceBuilder(resourceBuilder);
                    builderOptions.IncludeFormattedMessage = true;
                    builderOptions.IncludeScopes = false;
                    builderOptions.AddAzureMonitorLogExporter(options => { options.ConnectionString = configuration[connectionStringConfigurationKey]; });
                });
            });

            // Configure the OpenTelemetry tracer provider to add the resource attributes to all traces.
            services.ConfigureOpenTelemetryTracerProvider((sp, builder) =>
                builder.ConfigureResource(resourceBuilder =>
                    resourceBuilder.AddAttributes(resourceAttributes)).AddSource(serviceName)
            );
        }

        /// <summary>
        /// Adds authentication configuration to the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        /// <param name="entraInstanceConfigurationKey">The configuration key for the Entra ID instance.</param>
        /// <param name="entraTenantIdConfigurationKey">The configuration key for the Entra ID tenant id.</param>
        /// <param name="entraClientIdConfigurationkey">The configuration key for the Entra ID client id.</param>
        /// <param name="entraScopesConfigurationKey">The configuration key for the Entra ID scopes.</param>
        /// <param name="policyName">The name of the authorization policy.</param>
        /// <param name="requireScopes">Indicates whether a scope claim (scp) is required for authorization.</param>
        /// <param name="allowACLAuthorization">Indicates whether tokens that do not have either of the "scp" or "roles" claims are accepted (True means they are accepted).</param>
        public static void AddAuthenticationConfiguration(this IHostApplicationBuilder builder,
            string entraInstanceConfigurationKey,
            string entraTenantIdConfigurationKey,
            string entraClientIdConfigurationkey,
            string? entraScopesConfigurationKey,
            string policyName = "DefaultPolicy",
            bool requireScopes = true,
            bool allowACLAuthorization = false)
        {
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(jwtOptions => { },
                    identityOptions =>
                    {
                        identityOptions.Instance = builder.Configuration[entraInstanceConfigurationKey] ?? "";
                        identityOptions.TenantId = builder.Configuration[entraTenantIdConfigurationKey];
                        identityOptions.ClientId = builder.Configuration[entraClientIdConfigurationkey];
                        identityOptions.AllowWebApiToBeAuthorizedByACL = allowACLAuthorization;
                    });

            builder.Services.AddScoped<IUserClaimsProviderService, EntraUserClaimsProviderService>();

            // Configure the policy used by the API controllers:
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(policyName, policyBuilder =>
                {
                    policyBuilder.RequireAuthenticatedUser();
                    if (requireScopes)
                    {
                        var requiredScopes = builder.Configuration[entraScopesConfigurationKey]?.Split(' ');
                        if (requiredScopes != null && requiredScopes.Length > 0)
                        {
                            policyBuilder.RequireClaim(ClaimConstants.Scope, requiredScopes);
                        }
                        else
                        {
                            throw new InvalidOperationException("Scopes are required but no valid scopes are configured.");
                        }
                    }
                });
            });
        }

        /// <summary>
        /// Registers the <see cref="AzureKeyVaultService"/> with the dependency injection container.
        /// </summary>
        /// <param name="builder">The host application builder.</param>
        /// <param name="keyVaultUriConfigurationKeyName">The name of the configuration key that provides the URI of the Azure Key Vault service.</param>
        public static void AddAzureKeyVaultService(this IHostApplicationBuilder builder,
            string keyVaultUriConfigurationKeyName)
        {
            builder.Services.AddAzureClients(clientBuilder =>
            {
                var keyVaultUri = builder.Configuration[keyVaultUriConfigurationKeyName];
                clientBuilder.AddSecretClient(new Uri(keyVaultUri!))
                    .WithCredential(DefaultAuthentication.AzureCredential);
            });

            // Configure logging to filter out Azure Core and Azure Key Vault informational logs.
            builder.Logging.AddFilter("Azure.Core", LogLevel.Warning);
            builder.Logging.AddFilter("Azure.Security.KeyVault.Secrets", LogLevel.Warning);
            builder.Services.AddSingleton<IAzureKeyVaultService, AzureKeyVaultService>();
        }

        /// <summary>
        /// Registers the <see cref="HttpClientFactoryService"/> with the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        public static void AddHttpClientFactoryService(this IHostApplicationBuilder builder)
        {
            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<IHttpClientFactoryService, HttpClientFactoryService>();
            builder.Services.ActivateSingleton<IHttpClientFactoryService>();
        }

        /// <summary>
        /// Registers the <see cref="HttpClientFactoryService"/> with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        public static void AddHttpClientFactoryService(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddSingleton<IHttpClientFactoryService, HttpClientFactoryService>();
            services.ActivateSingleton<IHttpClientFactoryService>();
        }

        /// <summary>
        /// Registers the <see cref="IDownstreamAPIService"/> implementation for a named API service with the dependency injection container.
        /// </summary>
        /// <param name="builder">The host application builder.</param>
        /// <param name="apiServiceName">The name of the API service whose implementation is being registered.</param>
        public static void AddDownstreamAPIService(this IHostApplicationBuilder builder, string apiServiceName) =>
            builder.Services.AddScoped<IDownstreamAPIService, DownstreamAPIService>(
                (serviceProvider) => new DownstreamAPIService(
                    apiServiceName,
                    serviceProvider.GetService<ICallContext>()!,
                    serviceProvider.GetService<IHttpClientFactoryService>()!,
                    serviceProvider.GetService<ILogger<DownstreamAPIService>>()!
                ));

        /// <summary>
        /// Registers the <see cref="IAzureResourceManagerService"/> implementation with the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        public static void AddAzureResourceManager(
            this IHostApplicationBuilder builder) =>
            builder.Services.AddSingleton<IAzureResourceManagerService, AzureResourceManagerService>();

        /// <summary>
        /// Registers the <see cref="IAzureResourceManagerService"/> implementation with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        public static void AddAzureResourceManager(
            this IServiceCollection services) =>
            services.AddSingleton<IAzureResourceManagerService, AzureResourceManagerService>();

        /// <summary>
        /// Registers the <see cref="IAzureCosmosDBService"/> implementation with the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        public static void AddAzureCosmosDBService(this IHostApplicationBuilder builder) =>
            builder.Services.AddAzureCosmosDBService(builder.Configuration);

        /// <summary>
        /// Registers the <see cref="IAzureCosmosDBService"/> implementation with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfigurationManager"/> application configuration manager.</param>
        public static void AddAzureCosmosDBService(this IServiceCollection services, IConfigurationManager configuration)
        {
            services.AddOptions<CosmosDbSettings>()
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_CoreAPI_Configuration_CosmosDB));

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

            services.AddSingleton<IAzureCosmosDBService, AzureCosmosDBService>();
        }
    }
}
