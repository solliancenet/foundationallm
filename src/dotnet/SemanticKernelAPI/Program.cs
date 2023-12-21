using Azure.Identity;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
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
                options.Connect(builder.Configuration["FoundationaLLM:AppConfig:ConnectionString"]);
                options.ConfigureKeyVault(options =>
                {
                    options.SetCredential(new DefaultAzureCredential());
                });
                options.Select("FoundationaLLM:APIs:*");
                options.Select("FoundationaLLM:DurableSystemPrompt:*");
                options.Select("FoundationaLLM:CognitiveSearchMemorySource:*");
                options.Select("FoundationaLLM:CoreAPI:BlobStorageMemorySource:*");
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
                .Bind(builder.Configuration.GetSection("FoundationaLLM:APIs:SemanticKernelAPI"));
            builder.Services.AddTransient<IAPIKeyValidationService, APIKeyValidationService>();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGenNewtonsoftSupport();
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
                });

            builder.Services.AddOptions<SemanticKernelServiceSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:SemanticKernelAPI"));
            builder.Services.AddSingleton<ISemanticKernelService, SemanticKernelService>();

            // Simple, static system prompt service
            //builder.Services.AddSingleton<ISystemPromptService, InMemorySystemPromptService>();
            builder.Services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
            {
                ConnectionString = builder.Configuration["FoundationaLLM:APIs:SemanticKernelAPI:AppInsightsConnectionString"],
                DeveloperMode = builder.Environment.IsDevelopment()
            });
            //builder.Services.AddServiceProfiler();

            // System prompt service backed by an Azure blob storage account
            builder.Services.AddOptions<DurableSystemPromptServiceSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:DurableSystemPrompt"));
            builder.Services.AddSingleton<ISystemPromptService, DurableSystemPromptService>();

            builder.Services.AddOptions<AzureCognitiveSearchMemorySourceSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:CognitiveSearchMemorySource"));
            builder.Services.AddTransient<IMemorySource, AzureCognitiveSearchMemorySource>();

            builder.Services.AddOptions<BlobStorageMemorySourceSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:BlobStorageMemorySource"));
            builder.Services.AddTransient<IMemorySource, BlobStorageMemorySource>();

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
