using Asp.Versioning;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Middleware;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.OpenAPI;
using FoundationaLLM.Common.Services.Cache;
using FoundationaLLM.Common.Services.Security;
using FoundationaLLM.Common.Validation;
using FoundationaLLM.Orchestration.Core.Models.ConfigurationOptions;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FoundationaLLM.Orchestration.API
{
    /// <summary>
    /// Program class for the Orchestration API
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point for the Orchestration API
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            DefaultAuthentication.Initialize(
                builder.Environment.IsProduction(),
                ServiceNames.OrchestrationAPI);

            builder.Configuration.Sources.Clear();
            builder.Configuration.AddJsonFile("appsettings.json", false, true);
            builder.Configuration.AddEnvironmentVariables();
            builder.Configuration.AddAzureAppConfiguration((Action<Microsoft.Extensions.Configuration.AzureAppConfiguration.AzureAppConfigurationOptions>)(options =>
            {
                options.Connect(builder.Configuration[EnvironmentVariables.FoundationaLLM_AppConfig_ConnectionString]);
                options.ConfigureKeyVault(options =>
                {
                    options.SetCredential(DefaultAuthentication.AzureCredential);
                });
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Instance);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Logging);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Configuration);

                //TODO: Replace this with a more granular approach that would only bring in the configuration namespaces that are actually needed.
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints);

                options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_AuthorizationAPI_Essentials);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Configuration_Storage);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Agent_Storage);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Vectorization_Storage);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Vectorization_Storage);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_DataSource_Storage);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Attachment_Storage);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_AIModel_Storage);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Prompt_Storage);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_AzureOpenAI_Storage);

                options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_AzureEventGrid_Essentials);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_AzureEventGrid_Configuration);
                options.Select(AppConfigurationKeys.FoundationaLLM_Events_Profiles_OrchestrationAPI);
            }));
            if (builder.Environment.IsDevelopment())
                builder.Configuration.AddJsonFile("appsettings.development.json", true, true);

            // Add services to the container.

            // Add OpenTelemetry.
            builder.AddOpenTelemetry(
                AppConfigurationKeys.FoundationaLLM_APIEndpoints_OrchestrationAPI_Essentials_AppInsightsConnectionString,
                ServiceNames.OrchestrationAPI);

            builder.Services.AddInstanceProperties(builder.Configuration);

            // Add Azure ARM services.
            builder.AddAzureResourceManager();

            // Add event services.
            builder.Services.AddAzureEventGridEvents(
                builder.Configuration,
                AppConfigurationKeySections.FoundationaLLM_Events_Profiles_OrchestrationAPI);

            //builder.Services.AddServiceProfiler();
            builder.Services.AddControllers();

            // Add API Key Authorization
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<APIKeyAuthenticationFilter>();
            builder.Services.AddOptions<APIKeyValidationSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_OrchestrationAPI_Essentials));
            builder.Services.AddTransient<IAPIKeyValidationService, APIKeyValidationService>();
            builder.Services.AddOptions<InstanceSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Instance));

            builder.Services.AddOptions<SemanticKernelServiceSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_SemanticKernelAPI_Configuration));

            builder.Services.AddOptions<LangChainServiceSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_LangChainAPI_Configuration));

            builder.Services.AddScoped<ICallContext, CallContext>();
            builder.Services.AddScoped<IUserClaimsProviderService, NoOpUserClaimsProviderService>();

            builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            // Resource validation
            builder.Services.AddSingleton<IResourceValidatorFactory, ResourceValidatorFactory>();

            // Add authorization services.
            builder.AddGroupMembership();
            builder.AddAuthorizationService();

            //----------------------------
            // Resource providers
            //----------------------------
            builder.AddAgentResourceProvider();
            builder.AddPromptResourceProvider();
            builder.AddVectorizationResourceProvider();
            builder.AddConfigurationResourceProvider();
            builder.AddDataSourceResourceProvider();
            builder.AddAttachmentResourceProvider();
            builder.AddAIModelResourceProvider();
            builder.AddAzureOpenAIResourceProvider();

            // Register the downstream services and HTTP clients.
            builder.AddHttpClientFactoryService();
            builder.AddLLMOrchestrationServices();
            builder.AddOrchestrationService();

            builder.Services
                .AddApiVersioning(options =>
                {
                    // Reporting api versions will return the headers
                    // "api-supported-versions" and "api-deprecated-versions"
                    options.ReportApiVersions = true;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.DefaultApiVersion = new ApiVersion(new DateOnly(2024, 2, 16));
                })
                .AddMvc()
                .AddApiExplorer();

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

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
