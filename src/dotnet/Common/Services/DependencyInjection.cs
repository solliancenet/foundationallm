using Azure.Monitor.OpenTelemetry.Exporter;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Authorization;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.Common.Models.Configuration.Environment;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Common.Services.API;
using FoundationaLLM.Common.Services.Azure;
using FoundationaLLM.Common.Services.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace FoundationaLLM
{
    /// <summary>
    /// General purpose dependency injection extensions.
    /// </summary>
    public static partial class DependencyInjection
    {
        /// <summary>
        /// Adds CORS policies the the dependency injection container.
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
            var resourceBuilder = ResourceBuilder
                .CreateDefault()
                .AddAttributes(new Dictionary<string, object>
                    {
                        { "service.name", serviceName },
                        { "service.namespace", "FoundationaLLM" },
                        { "service.version", builder.Configuration[EnvironmentVariables.FoundationaLLM_Version]! },
                        { "service.instance.id", ValidatedEnvironment.MachineName }
                    });


            // Add the OpenTelemetry logging provider and send logs to Azure Monitor.
            builder.Logging.AddOpenTelemetry(openTelemetryLoggerOptions =>
            {
                openTelemetryLoggerOptions.SetResourceBuilder(resourceBuilder);
                openTelemetryLoggerOptions.IncludeFormattedMessage = true;
                openTelemetryLoggerOptions.IncludeScopes = true;
                openTelemetryLoggerOptions.AddAzureMonitorLogExporter(azureMonitorOptions =>
                {
                    azureMonitorOptions.ConnectionString = builder.Configuration[connectionStringConfigurationKey];
                });
            });

            // Add the OpenTelemetry telemetry service and send telemetry data to Azure Monitor.
            builder.Services.AddOpenTelemetry()
                .WithTracing(traceProviderBuilder => traceProviderBuilder
                    .SetResourceBuilder(resourceBuilder)
                    .AddSource("Azure.*")
                    .AddSource(serviceName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation(httpOptions => httpOptions.FilterHttpRequestMessage = (_) =>
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
                    .AddAzureMonitorTraceExporter(azureMonitorOptions =>
                        {
                            azureMonitorOptions.ConnectionString = builder.Configuration[connectionStringConfigurationKey];
                        }));
        }

        /// <summary>
        /// Adds Microsoft EntraID authentication configuration to the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        /// <param name="entraInstanceConfigurationKey">The configuration key for the Entra ID instance.</param>
        /// <param name="entraTenantIdConfigurationKey">The configuration key for the Entra ID tenant id.</param>
        /// <param name="entraClientIdConfigurationkey">The configuration key for the Entra ID client id.</param>
        /// <param name="entraScopesConfigurationKey">The configuration key for the Entra ID scopes.</param>
        /// <param name="policyName">The name of the authorization policy.</param>
        /// <param name="requireScopes">Indicates whether a scope claim (scp) is required for authorization.</param>
        /// <param name="allowACLAuthorization">Indicates whether tokens that do not have either of the "scp" or "roles" claims are accepted (True means they are accepted).</param>
        public static void AddMicrosoftEntraIDAuthentication(this IHostApplicationBuilder builder,
            string entraInstanceConfigurationKey,
            string entraTenantIdConfigurationKey,
            string entraClientIdConfigurationkey,
            string? entraScopesConfigurationKey,
            string policyName = AuthorizationPolicyNames.MicrosoftEntraIDStandard,
            bool requireScopes = true,
            bool allowACLAuthorization = false)
        {
            // Entra ID authentication configuration.
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
                // 
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
        /// Adds FoundationaLLM Agent Access Token authentication configuration to the dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="IHostApplicationBuilder"/> application builder managing the dependency injection container.</param>
        public static void AddFoundationaLLMAgentAccessTokenAuthentication(this IHostApplicationBuilder builder)
        {
            // Agent access token authentication configuration.
            builder.Services.AddAuthentication()
                .AddScheme<AgentAccessTokenOptions, AgentAccessTokenAuthenticationHandler>(
                    AgentAccessTokenDefaults.AuthenticationScheme, options => { })
                .AddPolicyScheme("MultiSchemeAuthenticaiton", "Bearer, AgentAccessToken", options =>
                {
                    options.ForwardDefaultSelector = context =>
                        context.Request.Headers.ContainsKey(HttpHeaders.AgentAccessToken)
                            ? AgentAccessTokenDefaults.AuthenticationScheme
                            : JwtBearerDefaults.AuthenticationScheme;
                });

            // Configure the policy used by the API controllers:
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthorizationPolicyNames.FoundationaLLMAgentAccessToken, policyBuilder =>
                {
                    policyBuilder.RequireAuthenticatedUser();
                });
            });
        }

        /// <summary>
        /// Registers the <see cref="AzureKeyVaultService"/> with the dependency injection container.
        /// </summary>
        /// <param name="builder">The host application builder.</param>
        /// <param name="keyVaultUriConfigurationKeyName">The name of the configuration key that provides the URI of the Azure Key Vault service.</param>
        public static void AddAzureKeyVaultService(this IHostApplicationBuilder builder,
            string keyVaultUriConfigurationKeyName) =>
            builder.Services.AddAzureKeyVaultService(
                builder.Configuration,
                keyVaultUriConfigurationKeyName);

        /// <summary>
        /// Registers the <see cref="AzureKeyVaultService"/> service with the dependency injection container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> dependency injection container service collection.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> configuration provider.</param>
        /// <param name="keyVaultUriConfigurationKeyName">The name of the configuration key that provides the URI of the Azure Key Vault service.</param>
        public static void AddAzureKeyVaultService(
            this IServiceCollection services,
            IConfiguration configuration,
            string keyVaultUriConfigurationKeyName)
        {
            services.AddAzureClients(clientBuilder =>
            {
                var keyVaultUri = configuration[keyVaultUriConfigurationKeyName];
                clientBuilder.AddSecretClient(new Uri(keyVaultUri!))
                    .WithCredential(ServiceContext.AzureCredential);
            });

            services.AddSingleton<IAzureKeyVaultService, AzureKeyVaultService>();
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
                    serviceProvider.GetService<IOrchestrationContext>()!,
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
            services.AddOptions<AzureCosmosDBSettings>()
                .Bind(configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_CoreAPI_Configuration_CosmosDB));

            services.AddSingleton<CosmosClient>(serviceProvider =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<AzureCosmosDBSettings>>().Value;
                return new CosmosClientBuilder(settings.Endpoint, ServiceContext.AzureCredential)
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
