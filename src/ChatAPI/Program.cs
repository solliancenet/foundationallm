using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Models.ConfigurationOptions;
using FoundationaLLM.Core.Services;
using Polly;

namespace FoundationaLLM.ChatAPI
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

            builder.Services.AddApplicationInsightsTelemetry();
            builder.Services.AddControllers();
            builder.Services.AddProblemDetails();
            builder.Services.AddApiVersioning();

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
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseExceptionHandler(exceptionHandlerApp
                    => exceptionHandlerApp.Run(async context
                        => await Results.Problem().ExecuteAsync(context)));

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}