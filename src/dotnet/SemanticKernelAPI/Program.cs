using Asp.Versioning;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Clients;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Middleware;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.OpenAPI;
using FoundationaLLM.Common.Services.Security;
using FoundationaLLM.Common.Validation;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FoundationaLLM.SemanticKernel.API
{
    /// <summary>
    /// Program class for the Semantic Kernel API.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point for the Semantic Kernel API.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ServiceContext.Initialize(
                builder.Environment.IsProduction(),
                ServiceNames.SemanticKernelAPI);

            builder.AddDIContainerSettings();

            builder.Configuration.Sources.Clear();
            builder.Configuration.AddJsonFile("appsettings.json", false, true);
            builder.Configuration.AddEnvironmentVariables();
            builder.Configuration.AddAzureAppConfiguration((Action<Microsoft.Extensions.Configuration.AzureAppConfiguration.AzureAppConfigurationOptions>)(options =>
            {
                options.Connect(builder.Configuration[EnvironmentVariables.FoundationaLLM_AppConfig_ConnectionString]);
                options.ConfigureKeyVault(options =>
                {
                    options.SetCredential(ServiceContext.AzureCredential);
                });
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Instance);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Configuration);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProvidersCache);

                options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Events_Profiles_VectorizationAPI);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_SemanticKernelAPI_Configuration);

                options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_AuthorizationAPI_Essentials);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_Configuration_Storage);

                options.Select(AppConfigurationKeyFilters.FoundationaLLM_ResourceProviders_AIModel_Storage);
            }));
            if (builder.Environment.IsDevelopment())
                builder.Configuration.AddJsonFile("appsettings.development.json", true, true);

            // Add OpenTelemetry.
            builder.AddOpenTelemetry(
                AppConfigurationKeys.FoundationaLLM_APIEndpoints_SemanticKernelAPI_Essentials_AppInsightsConnectionString,
                ServiceNames.SemanticKernelAPI);

            // CORS policies
            builder.AddCorsPolicies();

            // Generic exception handling
            builder.AddSemanticKernelGenericExceptionHandling();

            builder.AddSemanticKernelService();

            // Add authorization services.
            builder.AddGroupMembership();
            builder.Services.AddSingleton<IAuthorizationServiceClient, NullAuthorizationServiceClient>();

            // Resource validation
            builder.AddResourceValidatorFactory();

            // Add event services
            builder.Services.AddAzureEventGridEvents(
                builder.Configuration,
                AppConfigurationKeySections.FoundationaLLM_Events_Profiles_VectorizationAPI);

            // Add Azure ARM services
            builder.AddAzureResourceManager();

            //----------------------------
            // Resource providers
            //----------------------------
            builder.AddResourceProviderCacheSettings();

            builder.AddConfigurationResourceProvider();
            builder.AddAIModelResourceProvider();

            builder.AddHttpClientFactoryService();
            builder.AddOrchestrationContext();
            builder.Services.AddScoped<IUserClaimsProviderService, NoOpUserClaimsProviderService>();

            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
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

            // Add API Key Authorization
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<APIKeyAuthenticationFilter>();
            builder.Services.AddOptions<APIKeyValidationSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_SemanticKernelAPI_Essentials));
            builder.Services.AddOptions<InstanceSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Instance));
            builder.Services.AddTransient<IAPIKeyValidationService, APIKeyValidationService>();
            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

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

            // Set the CORS policy before other middleware.
            app.UseCors(CorsPolicyNames.AllowAllOrigins);

            // Register the middleware to extract the user identity context and other HTTP request context data required by the downstream services.
            app.UseMiddleware<CallContextMiddleware>();
            app.UseExceptionHandler();

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
                });

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
