using Asp.Versioning;
using FoundationaLLM.Common.OpenAPI;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Models.ConfigurationOptions;
using FoundationaLLM.Core.Services;
using Microsoft.Extensions.Options;
using Polly;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FoundationaLLM.Core.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHttpClient(FoundationaLLM.Core.Constants.HttpClients.LangChainApiClient,
                    httpClient =>
                    {
                        httpClient.BaseAddress = new Uri(builder.Configuration["FoundationaLLM:LangChainOrchestration:APIUrl"]);
                        httpClient.DefaultRequestHeaders.Add("X-API-KEY", builder.Configuration["FoundationaLLM:LangChainOrchestration:APIKey"]);
                    })
                .AddTransientHttpErrorPolicy(policyBuilder =>
                    policyBuilder.WaitAndRetryAsync(
                        3, retryNumber => TimeSpan.FromMilliseconds(600)));
            builder.Services.AddHttpClient(FoundationaLLM.Core.Constants.HttpClients.SemanticKernelApiClient,
                    httpClient =>
                    {
                        httpClient.BaseAddress = new Uri(builder.Configuration["FoundationaLLM:SemanticKernelOrchestration:APIUrl"]);
                        httpClient.DefaultRequestHeaders.Add("X-API-KEY", builder.Configuration["FoundationaLLM:SemanticKernelOrchestration:APIKey"]);
                    })
                .AddTransientHttpErrorPolicy(policyBuilder =>
                    policyBuilder.WaitAndRetryAsync(
                        3, retryNumber => TimeSpan.FromMilliseconds(600)));

            builder.Services.AddApplicationInsightsTelemetry();
            builder.Services.AddControllers();
            builder.Services.AddProblemDetails();
            builder.Services.AddApiVersioning(options =>
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

            builder.Services.AddOptions<CosmosDbSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:CosmosDB"));

            builder.Services.AddOptions<SemanticKernelOrchestrationServiceSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:SemanticKernelOrchestration"));

            builder.Services.AddOptions<LangChainOrchestrationServiceSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:LangChainOrchestration"));

            builder.Services.AddOptions<ChatServiceSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:Chat"));

            builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();
            builder.Services.AddSingleton<ISemanticKernelOrchestrationService, SemanticKernelOrchestrationService>();
            builder.Services.AddSingleton<ILangChainOrchestrationService, LangChainOrchestrationService>();
            builder.Services.AddSingleton<IChatService, ChatService>();

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
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

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}