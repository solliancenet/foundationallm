using System.Net;
using Asp.Versioning;
using Azure.Identity;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Middleware;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.OpenAPI;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Common.Services.API;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions;
using FoundationaLLM.Gatekeeper.Core.Services;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FoundationaLLM.Gatekeeper.API
{
    /// <summary>
    /// Program class for the Gatekeeper API.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point for Gatekeeper API.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.Sources.Clear();
            builder.Configuration.AddJsonFile("appsettings.json", false, true);
            builder.Configuration.AddEnvironmentVariables();
            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                options.Connect(builder.Configuration[AppConfigurationKeys.FoundationaLLM_AppConfig_ConnectionString]);
                options.ConfigureKeyVault(options =>
                {
                    options.SetCredential(new DefaultAzureCredential());
                });
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIs);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Refinement);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_AzureContentSafety);
            });
            if (builder.Environment.IsDevelopment())
                builder.Configuration.AddJsonFile("appsettings.development.json", true, true);

            // Add services to the container.
            builder.Services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
            {
                ConnectionString = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_GatekeeperAPI_AppInsightsConnectionString],
                DeveloperMode = builder.Environment.IsDevelopment()
            });
            //builder.Services.AddServiceProfiler();
            builder.Services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = Common.Settings.CommonJsonSerializerSettings.GetJsonSerializerSettings().ContractResolver;
            });

            // Add API Key Authorization
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<APIKeyAuthenticationFilter>();
            builder.Services.AddOptions<APIKeyValidationSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIs_GatekeeperAPI));
            builder.Services.AddOptions<InstanceSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Instance));

            // Register the downstream services and HTTP clients.
            RegisterDownstreamServices(builder);

            builder.Services.AddTransient<IAPIKeyValidationService, APIKeyValidationService>();

            builder.Services.AddOptions<RefinementServiceSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Refinement));
            builder.Services.AddScoped<IRefinementService, RefinementService>();

            builder.Services.AddOptions<AzureContentSafetySettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_AzureContentSafety));
            builder.Services.AddScoped<IContentSafetyService, AzureContentSafetyService>();
            builder.Services.AddScoped<IGatekeeperIntegrationAPIService, GatekeeperIntegrationAPIService>();

            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            builder.Services.AddScoped<ICallContext, CallContext>();
            builder.Services.AddScoped<IHttpClientFactoryService, HttpClientFactoryService>();
            builder.Services.AddScoped<IUserClaimsProviderService, NoOpUserClaimsProviderService>();

            builder.Services.AddOptions<GatekeeperServiceSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIs_GatekeeperAPI_Configuration));
            builder.Services.AddScoped<IGatekeeperService, GatekeeperService>();

            builder.Services
                .AddApiVersioning(options =>
                {
                    // Reporting api versions will return the headers
                    // "api-supported-versions" and "api-deprecated-versions"
                    options.ReportApiVersions = true;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                })
                .AddMvc()
                .AddApiExplorer(options =>
                {
                    // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                    // note: the specified format code will format the version as "'v'major[.minor][-status]"
                    options.GroupNameFormat = "'v'VVV";
                });

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(
                options =>
                {
                    // Add a custom operation filter which sets default values
                    options.OperationFilter<SwaggerDefaultValues>();

                    var fileName = typeof(Program).Assembly.GetName().Name + ".xml";
                    var filePath = Path.Combine(AppContext.BaseDirectory, fileName);

                    // Integrate xml comments
                    options.IncludeXmlComments(filePath);

                    // Adds auth via X-API-KEY header
                    options.AddAPIKeyAuth();
                })
                .AddSwaggerGenNewtonsoftSupport();

            builder.Services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });

            var app = builder.Build();

            // Register the middleware to extract the user identity context and other HTTP request context data required by the downstream services.
            app.UseMiddleware<CallContextMiddleware>();

            app.UseExceptionHandler(exceptionHandlerApp
                => exceptionHandlerApp.Run(async context
                    => await Results.Problem().ExecuteAsync(context)));

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI(
                options =>
                {
                    var descriptions = app.DescribeApiVersions();

                    // build a swagger endpoint for each discovered API version
                    foreach (var description in descriptions)
                    {
                        var url = $"/swagger/{description.GroupName}/swagger.json";
                        var name = description.GroupName.ToUpperInvariant();
                        options.SwaggerEndpoint(url, name);
                    }
                });

            _ = bool.TryParse(builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_GatekeeperAPI_ForceHttpsRedirection], out var forceHttpsRedirection);
            if (forceHttpsRedirection)
            {
                app.UseHttpsRedirection();
            }
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        /// <summary>
        /// Bind the downstream API settings to the configuration and register the HTTP clients.
        /// The AddResilienceHandler extension method is used to add the standard Polly resilience
        /// strategies to the HTTP clients.
        /// </summary>
        /// <param name="builder"></param>
        private static void RegisterDownstreamServices(WebApplicationBuilder builder)
        {
            var downstreamAPISettings = new DownstreamAPISettings
            {
                DownstreamAPIs = []
            };

            var agentFactoryAPISettings = new DownstreamAPIKeySettings
            {
                APIUrl = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_AgentFactoryAPI_APIUrl]!,
                APIKey = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_AgentFactoryAPI_APIKey]!
            };

            downstreamAPISettings.DownstreamAPIs[HttpClients.AgentFactoryAPI] = agentFactoryAPISettings;

            builder.Services
                    .AddHttpClient(HttpClients.AgentFactoryAPI,
                        client => { client.BaseAddress = new Uri(agentFactoryAPISettings.APIUrl); })
                    .AddResilienceHandler(
                        "DownstreamPipeline",
                        static strategyBuilder =>
                        {
                            CommonHttpRetryStrategyOptions.GetCommonHttpRetryStrategyOptions();
                        });

            var gatekeeperIntegrationAPISettings = new DownstreamAPIKeySettings
            {
                APIUrl = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_GatekeeperIntegrationAPI_APIUrl]!,
                APIKey = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_GatekeeperIntegrationAPI_APIKey]!
            };

            downstreamAPISettings.DownstreamAPIs[HttpClients.GatekeeperIntegrationAPI] = gatekeeperIntegrationAPISettings;

            builder.Services
                    .AddHttpClient(HttpClients.GatekeeperIntegrationAPI,
                        client => { client.BaseAddress = new Uri(gatekeeperIntegrationAPISettings.APIUrl); })
                    .AddResilienceHandler(
                        "DownstreamPipeline",
                        static strategyBuilder =>
                        {
                            // See: https://www.pollydocs.org/strategies/retry.html
                            strategyBuilder.AddRetry(new HttpRetryStrategyOptions
                            {
                                BackoffType = DelayBackoffType.Exponential,
                                MaxRetryAttempts = 5,
                                UseJitter = true
                            });
                        });

            builder.Services.AddSingleton<IDownstreamAPISettings>(downstreamAPISettings);
            builder.Services.AddScoped<IDownstreamAPIService, DownstreamAPIService>((serviceProvider)
                => new DownstreamAPIService(HttpClients.AgentFactoryAPI, serviceProvider.GetService<IHttpClientFactoryService>()!));
        }
    }
}
