using Solliance.AICopilot.SemanticKernel.MemorySource;
using Solliance.AICopilot.Core.Interfaces;
using Solliance.AICopilot.Core.Models.ConfigurationOptions;
using Solliance.AICopilot.Core.Services;

namespace Solliance.AICopilot.ChatAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddOptions<CosmosDbSettings>()
                .Bind(builder.Configuration.GetSection("MSCosmosDBOpenAI:CosmosDB"));

            builder.Services.AddOptions<SemanticKernelOrchestrationServiceSettings>()
                .Bind(builder.Configuration.GetSection("MSCosmosDBOpenAI"));

            builder.Services.AddOptions<LangChainOrchestrationServiceSettings>()
                .Bind(builder.Configuration.GetSection("SollianceAICopilot:LangChainOrchestration"));

            builder.Services.AddSingleton<ICosmosDbService, CosmosDbService>();
            builder.Services.AddSingleton<ISemanticKernelOrchestrationService, SemanticKernelOrchestrationService>();
            builder.Services.AddSingleton<ILangChainOrchestrationService, LangChainOrchestrationService>();
            builder.Services.AddSingleton<IChatService, ChatService>();

            // Simple, static system prompt service
            //builder.Services.AddSingleton<ISystemPromptService, InMemorySystemPromptService>();

            // System prompt service backed by an Azure blob storage account
            builder.Services.AddOptions<DurableSystemPromptServiceSettings>()
                .Bind(builder.Configuration.GetSection("MSCosmosDBOpenAI:DurableSystemPrompt"));
            builder.Services.AddSingleton<ISystemPromptService, DurableSystemPromptService>();

            builder.Services.AddOptions<AzureCognitiveSearchMemorySourceSettings>()
                .Bind(builder.Configuration.GetSection("MSCosmosDBOpenAI:CognitiveSearchMemorySource"));
            builder.Services.AddTransient<IMemorySource, AzureCognitiveSearchMemorySource>();

            builder.Services.AddOptions<BlobStorageMemorySourceSettings>()
                .Bind(builder.Configuration.GetSection("MSCosmosDBOpenAI:BlobStorageMemorySource"));
            builder.Services.AddTransient<IMemorySource, BlobStorageMemorySource>();

            builder.Services.AddScoped<ChatEndpoints>();

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseAuthorization();

            // Map the chat REST endpoints:
            using (var scope = app.Services.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<ChatEndpoints>();
                service?.Map(app);
            }

            app.Run();
        }
    }
}