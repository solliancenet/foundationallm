using Azure.Identity;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.OpenAPI;
using FoundationaLLM.SemanticKernel.Core.Interfaces;
using FoundationaLLM.SemanticKernel.Core.Models.ConfigurationOptions;
using FoundationaLLM.SemanticKernel.Core.Services;
using FoundationaLLM.SemanticKernel.MemorySource;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;

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
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_DurableSystemPrompt);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_CognitiveSearchMemorySource);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_BlobStorageMemorySource);
            });
            if (builder.Environment.IsDevelopment())
                builder.Configuration.AddJsonFile("appsettings.development.json", true, true);

            // Add services to the container.
            builder.Services.AddApplicationInsightsTelemetry();
            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            builder.Services.AddApiVersioning();

            // Add API Key Authorization
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<APIKeyAuthenticationFilter>();
            builder.Services.AddOptions<APIKeyValidationSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIs_SemanticKernelAPI));
            builder.Services.AddOptions<InstanceSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Instance));
            builder.Services.AddTransient<IAPIKeyValidationService, APIKeyValidationService>();

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

            builder.Services.AddOptions<SemanticKernelServiceSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_SemanticKernelAPI));
            builder.Services.AddSingleton<ISemanticKernelService, SemanticKernelService>();

            // Simple, static system prompt service
            //builder.Services.AddSingleton<ISystemPromptService, InMemorySystemPromptService>();
            builder.Services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
            {
                ConnectionString = builder.Configuration[AppConfigurationKeys.FoundationaLLM_APIs_SemanticKernelAPI_AppInsightsConnectionString],
                DeveloperMode = builder.Environment.IsDevelopment()
            });
            //builder.Services.AddServiceProfiler();

            // System prompt service backed by an Azure blob storage account
            builder.Services.AddOptions<DurableSystemPromptServiceSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_DurableSystemPrompt));
            builder.Services.AddSingleton<ISystemPromptService, DurableSystemPromptService>();

            builder.Services.AddOptions<AzureCognitiveSearchMemorySourceSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_CognitiveSearchMemorySource));
            builder.Services.AddTransient<IMemorySource, AzureCognitiveSearchMemorySource>();

            builder.Services.AddOptions<BlobStorageMemorySourceSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_BlobStorageMemorySource));
            builder.Services.AddTransient<IMemorySource, BlobStorageMemorySource>();

            builder.Services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
            });

            var app = builder.Build();

            app.UseExceptionHandler(exceptionHandlerApp
                => exceptionHandlerApp.Run(async context
                    => await Results.Problem().ExecuteAsync(context)));

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
