using Asp.Versioning;
using Azure.Identity;
using FoundationaLLM.AgentFactory.Core.Interfaces;
using FoundationaLLM.AgentFactory.Core.Models.ConfigurationOptions;
using FoundationaLLM.AgentFactory.Core.Services;
using FoundationaLLM.AgentFactory.Interfaces;
using FoundationaLLM.AgentFactory.Models.ConfigurationOptions;
using FoundationaLLM.AgentFactory.Services;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Middleware;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Configuration;
using FoundationaLLM.Common.OpenAPI;
using FoundationaLLM.Common.Services;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.Extensions.Options;
using Polly;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FoundationaLLM.AgentFactory.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.Sources.Clear();
            builder.Configuration.AddJsonFile("appsettings.json", false, true);
            builder.Configuration.AddEnvironmentVariables();
            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                options.Connect(builder.Configuration["FoundationaLLM:AppConfig:ConnectionString"]);
                options.ConfigureKeyVault(options =>
                {
                    options.SetCredential(new DefaultAzureCredential());
                });
            });
            if (builder.Environment.IsDevelopment())
                builder.Configuration.AddJsonFile("appsettings.development.json", true, true);

            // Add services to the container.
            builder.Services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
            {
                ConnectionString = builder.Configuration["FoundationaLLM:APIs:AgentFactoryAPI:AppInsightsConnectionString"],
                DeveloperMode = builder.Environment.IsDevelopment()
            });
            builder.Services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = Common.Settings.CommonJsonSerializerSettings.GetJsonSerializerSettings().ContractResolver;
            });

            // Add API Key Authorization
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IUserClaimsProviderService, NoOpUserClaimsProviderService>();
            builder.Services.AddScoped<APIKeyAuthenticationFilter>();
            builder.Services.AddOptions<APIKeyValidationSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:APIs:AgentFactoryAPI"));
            builder.Services.AddTransient<IAPIKeyValidationService, APIKeyValidationService>();

            builder.Services.AddOptions<SemanticKernelServiceSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:APIs:SemanticKernelAPI"));

            builder.Services.AddOptions<LangChainServiceSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:APIs:LangChainAPI"));

            builder.Services.AddOptions<AgentHubSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:APIs:AgentHubAPI"));

            builder.Services.AddOptions<PromptHubSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:APIs:PromptHubAPI"));

            builder.Services.AddOptions<AgentFactorySettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:AgentFactory"));

            builder.Services.AddOptions<KeyVaultConfigurationServiceSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:Configuration"));
            
            builder.Services.AddScoped<ILLMOrchestrationService, SemanticKernelService>();
            builder.Services.AddScoped<ILLMOrchestrationService, LangChainService>();

            builder.Services.AddScoped<IAgentFactoryService, AgentFactoryService>();
            builder.Services.AddScoped<IAgentHubAPIService, AgentHubAPIService>();
            builder.Services.AddScoped<IDataSourceHubAPIService, DataSourceHubAPIService>();
            builder.Services.AddScoped<IPromptHubAPIService, PromptHubAPIService>();
            builder.Services.AddScoped<IUserIdentityContext, UserIdentityContext>();
            builder.Services.AddScoped<IHttpClientFactoryService, HttpClientFactoryService>();
            builder.Services.AddScoped<IUserClaimsProviderService, NoOpUserClaimsProviderService>();

            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            // Register the downstream services and HTTP clients.
            RegisterDownstreamServices(builder);


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
                });

            var app = builder.Build();

            // Register the middleware to set the user identity context.
            app.UseMiddleware<UserIdentityMiddleware>();

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

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }

        /// <summary>
        /// Bind the downstream API settings to the configuration and register the HTTP clients.
        /// </summary>
        /// <param name="builder"></param>
        private static void RegisterDownstreamServices(WebApplicationBuilder builder)
        {
            var downstreamAPISettings = new DownstreamAPISettings
            {
                DownstreamAPIs = new Dictionary<string, DownstreamAPIKeySettings>()
            };

            var agentHubAPISettings = new DownstreamAPIKeySettings
            {
                APIUrl = builder.Configuration[$"FoundationaLLM:APIs:{HttpClients.AgentHubAPI}:APIUrl"],
                APIKey = builder.Configuration[$"FoundationaLLM:APIs:{HttpClients.AgentHubAPI}:APIKey"]
            };
            downstreamAPISettings.DownstreamAPIs[HttpClients.AgentHubAPI] = agentHubAPISettings;

            builder.Services
                    .AddHttpClient(HttpClients.AgentHubAPI,
                        client => { client.BaseAddress = new Uri(agentHubAPISettings.APIUrl); })
                    .AddTransientHttpErrorPolicy(policyBuilder =>
                        policyBuilder.WaitAndRetryAsync(
                            3, retryNumber => TimeSpan.FromMilliseconds(600)));

            var dataSourceHubAPISettings = new DownstreamAPIKeySettings
            {
                APIUrl = builder.Configuration[$"FoundationaLLM:APIs:{HttpClients.DataSourceHubAPI}:APIUrl"],
                APIKey = builder.Configuration[$"FoundationaLLM:APIs:{HttpClients.DataSourceHubAPI}:APIKey"]
            };
            downstreamAPISettings.DownstreamAPIs[HttpClients.DataSourceHubAPI] = dataSourceHubAPISettings;

            builder.Services
                    .AddHttpClient(HttpClients.DataSourceHubAPI,
                        client => { client.BaseAddress = new Uri(dataSourceHubAPISettings.APIUrl); })
                    .AddTransientHttpErrorPolicy(policyBuilder =>
                        policyBuilder.WaitAndRetryAsync(
                            3, retryNumber => TimeSpan.FromMilliseconds(600)));

            var promptHubAPISettings = new DownstreamAPIKeySettings
            {
                APIUrl = builder.Configuration[$"FoundationaLLM:APIs:{HttpClients.PromptHubAPI}:APIUrl"],
                APIKey = builder.Configuration[$"FoundationaLLM:APIs:{HttpClients.PromptHubAPI}:APIKey"]
            };
            downstreamAPISettings.DownstreamAPIs[HttpClients.PromptHubAPI] = promptHubAPISettings;

            builder.Services
                    .AddHttpClient(HttpClients.PromptHubAPI,
                        client => { client.BaseAddress = new Uri(promptHubAPISettings.APIUrl); })
                    .AddTransientHttpErrorPolicy(policyBuilder =>
                        policyBuilder.WaitAndRetryAsync(
                            3, retryNumber => TimeSpan.FromMilliseconds(600)));

            var langChainAPISettings = new DownstreamAPIKeySettings
            {
                APIUrl = builder.Configuration[$"FoundationaLLM:APIs:{HttpClients.LangChainAPI}:APIUrl"],
                APIKey = builder.Configuration[$"FoundationaLLM:APIs:{HttpClients.LangChainAPI}:APIKey"]
            };
            downstreamAPISettings.DownstreamAPIs[HttpClients.LangChainAPI] = langChainAPISettings;

            builder.Services
                    .AddHttpClient(HttpClients.LangChainAPI,
                        client => { client.BaseAddress = new Uri(langChainAPISettings.APIUrl); })
                    .AddTransientHttpErrorPolicy(policyBuilder =>
                        policyBuilder.WaitAndRetryAsync(
                            3, retryNumber => TimeSpan.FromMilliseconds(600)));

            var semanticKernelAPISettings = new DownstreamAPIKeySettings
            {
                APIUrl = builder.Configuration[$"FoundationaLLM:APIs:{HttpClients.SemanticKernelAPI}:APIUrl"],
                APIKey = builder.Configuration[$"FoundationaLLM:APIs:{HttpClients.SemanticKernelAPI}:APIKey"]
            };
            downstreamAPISettings.DownstreamAPIs[HttpClients.SemanticKernelAPI] = semanticKernelAPISettings;

            builder.Services
                    .AddHttpClient(HttpClients.SemanticKernelAPI,
                        client => { client.BaseAddress = new Uri(semanticKernelAPISettings.APIUrl); })
                    .AddTransientHttpErrorPolicy(policyBuilder =>
                        policyBuilder.WaitAndRetryAsync(
                            3, retryNumber => TimeSpan.FromMilliseconds(600)));

            builder.Services.AddSingleton<IDownstreamAPISettings>(downstreamAPISettings);
        }
    }
}