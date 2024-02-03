using Asp.Versioning;
using Azure.Identity;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Middleware;
using FoundationaLLM.Common.Models.Configuration.Branding;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.OpenAPI;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Common.Services.API;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Management.Interfaces;
using FoundationaLLM.Management.Models.Configuration;
using FoundationaLLM.Management.Services;
using FoundationaLLM.Management.Services.APIServices;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Polly;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FoundationaLLM.Management.API
{
    /// <summary>
    /// Main entry point for the Management API.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Management API service configuration.
        /// </summary>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.Sources.Clear();
            builder.Configuration.AddJsonFile("appsettings.json", false, true);
            builder.Configuration.AddEnvironmentVariables();
            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                options.Connect(builder.Configuration[AppConfigurationKeys.FoundationaLLM_AppConfig_ConnectionString]);
                options.ConfigureKeyVault(options => { options.SetCredential(new DefaultAzureCredential()); });
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Instance);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIs);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_CosmosDB);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Branding);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_ManagementAPI_Entra);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Vectorization);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Agent);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Prompt);
            });

            if (builder.Environment.IsDevelopment())
                builder.Configuration.AddJsonFile("appsettings.development.json", true, true);

            var allowAllCorsOrigins = "AllowAllOrigins";
            builder.Services.AddCors(policyBuilder =>
            {
                policyBuilder.AddPolicy(allowAllCorsOrigins,
                    policy =>
                    {
                        policy.AllowAnyOrigin();
                        policy.WithHeaders("DNT", "Keep-Alive", "User-Agent", "X-Requested-With", "If-Modified-Since",
                            "Cache-Control", "Content-Type", "Range", "Authorization", "X-AGENT-HINT");
                        policy.AllowAnyMethod();
                    });
            });

            builder.Services.AddOptions<CosmosDbSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_CosmosDB));
            builder.Services.AddOptions<ClientBrandingConfiguration>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Branding));
            builder.Services.AddOptions<AppConfigurationSettings>()
                .Configure(o =>
                    o.ConnectionString = builder.Configuration[AppConfigurationKeys.FoundationaLLM_AppConfig_ConnectionString]!);

            builder.Services.AddInstanceProperties(builder.Configuration);

            builder.Services.AddScoped<IAgentFactoryAPIService, AgentFactoryAPIService>();
            builder.Services.AddScoped<IAgentHubAPIService, AgentHubAPIService>();
            builder.Services.AddScoped<IDataSourceHubAPIService, DataSourceHubAPIService>();
            builder.Services.AddScoped<IPromptHubAPIService, PromptHubAPIService>();
            builder.Services.AddScoped<IConfigurationManagementService, ConfigurationManagementService>();
            builder.Services.AddScoped<ICacheManagementService, CacheManagementService>();

            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            builder.Services.AddScoped<ICallContext, CallContext>();
            builder.Services.AddScoped<IHttpClientFactoryService, HttpClientFactoryService>();

            //----------------------------
            // Resource providers
            //----------------------------

            builder.Services.AddVectorizationResourceProvider(builder.Configuration);
            builder.Services.AddAgentResourceProvider(builder.Configuration);
            builder.Services.AddPromptResourceProvider(builder.Configuration);

            // Activate all resource providers (give them a chance to initialize).
            builder.Services.ActivateSingleton<IEnumerable<IResourceProviderService>>();

            // Register the authentication services:
            RegisterAuthConfiguration(builder);

            builder.Services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
            {
                ConnectionString = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_ManagementAPI_AppInsightsConnectionString],
                DeveloperMode = builder.Environment.IsDevelopment()
            });
            //builder.Services.AddServiceProfiler();

            // Register the downstream services and HTTP clients.
            RegisterDownstreamServices(builder);

            builder.Services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = FoundationaLLM.Common.Settings
                    .CommonJsonSerializerSettings
                    .GetJsonSerializerSettings().ContractResolver;
            });
            builder.Services.AddProblemDetails();
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

                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Id = "azure_auth",
                                    Type = ReferenceType.SecurityScheme
                                }
                            },
                            new[] {"user_impersonation"}
                        }
                    });                    

                    options.AddSecurityDefinition("azure_auth", new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Azure Active Directory Oauth2 Flow",
                        Name = "azure_auth",
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            Implicit = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri("https://login.microsoftonline.com/common/oauth2/authorize"),
                                Scopes = new Dictionary<string, string>
                                {
                                    {
                                        "user_impersonation",
                                        "impersonate your user account"
                                    }
                                }
                            }
                        },
                        BearerFormat = "JWT",
                        Scheme = "bearer"
                    });
                })
                .AddSwaggerGenNewtonsoftSupport();

            var app = builder.Build();

            // Set the CORS policy before other middleware.
            app.UseCors(allowAllCorsOrigins);

            // For the CoreAPI, we need to make sure that UseAuthentication is called before the UserIdentityMiddleware.
            app.UseAuthentication();
            app.UseAuthorization();

            // Register the middleware to extract the user identity context and other HTTP request context data required by the downstream services.
            app.UseMiddleware<CallContextMiddleware>();

            app.UseExceptionHandler(exceptionHandlerApp
                => exceptionHandlerApp.Run(async context
                    => await Results.Problem().ExecuteAsync(context)));

            // Configure the HTTP request pipeline.
            app.UseSwagger(p => p.SerializeAsV2 = true);
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

                    options.OAuthAdditionalQueryStringParams(new Dictionary<string, string>() { { "resource", builder.Configuration[AppConfigurationKeys.FoundationaLLM_Management_Entra_ClientId] } });
                });

            app.UseHttpsRedirection();
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
                DownstreamAPIs = new Dictionary<string, DownstreamAPIKeySettings>()
            };
            var retryOptions = CommonHttpRetryStrategyOptions.GetCommonHttpRetryStrategyOptions();

            // AgentFactoryAPI:
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
                    strategyBuilder =>
                    {
                        strategyBuilder.AddRetry(retryOptions);
                    });

            // AgentHubAPI:
            var agentHubAPISettings = new DownstreamAPIKeySettings
            {
                APIUrl = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_AgentHubAPI_APIUrl]!,
                APIKey = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_AgentHubAPI_APIKey]!
            };
            downstreamAPISettings.DownstreamAPIs[HttpClients.AgentHubAPI] = agentHubAPISettings;

            builder.Services
                    .AddHttpClient(HttpClients.AgentHubAPI,
                        client => { client.BaseAddress = new Uri(agentHubAPISettings.APIUrl); })
                    .AddResilienceHandler(
                        "DownstreamPipeline",
                        strategyBuilder =>
                        {
                            strategyBuilder.AddRetry(retryOptions);
                        });

            // DataSourceHubAPI:
            var dataSourceHubAPISettings = new DownstreamAPIKeySettings
            {
                APIUrl = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_DataSourceHubAPI_APIUrl]!,
                APIKey = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_DataSourceHubAPI_APIKey]!
            };
            downstreamAPISettings.DownstreamAPIs[HttpClients.DataSourceHubAPI] = dataSourceHubAPISettings;

            builder.Services
                .AddHttpClient(HttpClients.DataSourceHubAPI,
                    client => { client.BaseAddress = new Uri(dataSourceHubAPISettings.APIUrl); })
                .AddResilienceHandler(
                    "DownstreamPipeline",
                    strategyBuilder =>
                    {
                        strategyBuilder.AddRetry(retryOptions);
                    });

            // PromptHubAPI:
            var promptHubAPISettings = new DownstreamAPIKeySettings
            {
                APIUrl = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_PromptHubAPI_APIUrl]!,
                APIKey = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_PromptHubAPI_APIKey]!
            };
            downstreamAPISettings.DownstreamAPIs[HttpClients.PromptHubAPI] = promptHubAPISettings;

            builder.Services
                    .AddHttpClient(HttpClients.PromptHubAPI,
                        client => { client.BaseAddress = new Uri(promptHubAPISettings.APIUrl); })
                    .AddResilienceHandler(
                        "DownstreamPipeline",
                        strategyBuilder =>
                        {
                            strategyBuilder.AddRetry(retryOptions);
                        });

            // LangChainAPI:
            var langChainAPISettings = new DownstreamAPIKeySettings
            {
                APIUrl = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_LangChainAPI_APIUrl]!,
                APIKey = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_LangChainAPI_APIKey]!
            };
            downstreamAPISettings.DownstreamAPIs[HttpClients.LangChainAPI] = langChainAPISettings;

            builder.Services
                    .AddHttpClient(HttpClients.LangChainAPI,
                        client => { client.BaseAddress = new Uri(langChainAPISettings.APIUrl); })
                    .AddResilienceHandler(
                        "DownstreamPipeline",
                        strategyBuilder =>
                        {
                            strategyBuilder.AddRetry(retryOptions);
                        });

            // SemanticKernelAPI:
            var semanticKernelAPISettings = new DownstreamAPIKeySettings
            {
                APIUrl = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_SemanticKernelAPI_APIUrl]!,
                APIKey = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_SemanticKernelAPI_APIKey]!
            };
            downstreamAPISettings.DownstreamAPIs[HttpClients.SemanticKernelAPI] = semanticKernelAPISettings;

            builder.Services
                    .AddHttpClient(HttpClients.SemanticKernelAPI,
                        client => { client.BaseAddress = new Uri(semanticKernelAPISettings.APIUrl); })
                    .AddResilienceHandler(
                        "DownstreamPipeline",
                        strategyBuilder =>
                        {
                            strategyBuilder.AddRetry(retryOptions);
                        });

            builder.Services.AddSingleton<IDownstreamAPISettings>(downstreamAPISettings);
        }

        /// <summary>
        /// Register the authentication services.
        /// </summary>
        /// <param name="builder"></param>
        public static void RegisterAuthConfiguration(WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(jwtOptions => { },
                    identityOptions =>
                    {
                        identityOptions.ClientSecret =
                            builder.Configuration[AppConfigurationKeys.FoundationaLLM_ManagementAPI_Entra_ClientSecret];
                        identityOptions.Instance = builder.Configuration[AppConfigurationKeys.FoundationaLLM_ManagementAPI_Entra_Instance] ?? "";
                        identityOptions.TenantId = builder.Configuration[AppConfigurationKeys.FoundationaLLM_ManagementAPI_Entra_TenantId];
                        identityOptions.ClientId = builder.Configuration[AppConfigurationKeys.FoundationaLLM_ManagementAPI_Entra_ClientId];
                    });
            //.EnableTokenAcquisitionToCallDownstreamApi()
            //.AddInMemoryTokenCaches();

            //builder.Services.AddScoped<IAuthenticatedHttpClientFactory, EntraAuthenticatedHttpClientFactory>();
            builder.Services.AddScoped<IUserClaimsProviderService, EntraUserClaimsProviderService>();

            // Configure the scope used by the API controllers:
            var requiredScope = builder.Configuration[AppConfigurationKeys.FoundationaLLM_ManagementAPI_Entra_Scopes] ?? "";
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequiredScope", policyBuilder =>
                {
                    policyBuilder.RequireAuthenticatedUser();
                    policyBuilder.RequireClaim("http://schemas.microsoft.com/identity/claims/scope",
                        requiredScope.Split(' '));
                });
            });
        }
    }
}
