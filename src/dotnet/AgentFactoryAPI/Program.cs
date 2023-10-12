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
using FoundationaLLM.Common.Models.Configuration;
using FoundationaLLM.Common.OpenAPI;
using FoundationaLLM.Common.Services;
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

            // Add services to the container.
            builder.Services.AddApplicationInsightsTelemetry();
            builder.Services.AddControllers();

            // Add API Key Authorization
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<IUserClaimsProviderService, NoOpUserClaimsProviderService>();
            builder.Services.AddScoped<APIKeyAuthenticationFilter>();
            builder.Services.AddOptions<APIKeyValidationSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:AgentFactoryAPI"));
            builder.Services.AddTransient<IAPIKeyValidationService, APIKeyValidationService>();

            builder.Services.AddOptions<SemanticKernelOrchestrationServiceSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:SemanticKernelAPI"));

            builder.Services.AddOptions<LangChainOrchestrationServiceSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:LangChainAPI"));

            builder.Services.AddOptions<AgentHubSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:AgentHubAPI"));

            builder.Services.AddOptions<AgentFactorySettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:AgentFactory"));

            builder.Services.AddOptions<KeyVaultConfigurationServiceSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:Configuration"));

            builder.Services.AddSingleton<IConfigurationService, KeyVaultConfigurationService>();
            
            builder.Services.AddScoped<ISemanticKernelOrchestrationService, SemanticKernelOrchestrationService>();
            builder.Services.AddScoped<ILangChainOrchestrationService, LangChainOrchestrationService>();
            builder.Services.AddScoped<IAgentFactoryService, AgentFactoryService>();
            builder.Services.AddScoped<IAgentHubService, AgentHubAPIService>();

            builder.Configuration.AddAzureKeyVault(
                new Uri(builder.Configuration["FoundationaLLM:Configuration:KeyVaultUri"]),
                new DefaultAzureCredential());


            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

            builder.Services
                .AddHttpClient(HttpClients.AgentHubAPIClient,
                    httpClient =>
                    {
                        httpClient.BaseAddress = new Uri(builder.Configuration["FoundationaLLM:AgentHubAPI:APIUrl"]);
                        httpClient.DefaultRequestHeaders.Add("X-API-KEY", builder.Configuration[
                            builder.Configuration["FoundationaLLM:AgentHubAPI:APIKeySecretName"]]);
                    })
                .AddTransientHttpErrorPolicy(policyBuilder =>
                    policyBuilder.WaitAndRetryAsync(
                        3, retryNumber => TimeSpan.FromMilliseconds(600)));
            builder.Services
                .AddHttpClient(HttpClients.LangChainAPIClient,
                    httpClient =>
                    {
                        httpClient.BaseAddress = new Uri(builder.Configuration["FoundationaLLM:LangChainAPI:APIUrl"]);
                        httpClient.DefaultRequestHeaders.Add("X-API-KEY", builder.Configuration[
                            builder.Configuration["FoundationaLLM:LangChainAPI:APIKeySecretName"]]);
                        httpClient.Timeout = TimeSpan.FromSeconds(600);
                    })
                .AddTransientHttpErrorPolicy(policyBuilder =>
                    policyBuilder.WaitAndRetryAsync(
                        3, retryNumber => TimeSpan.FromMilliseconds(600)));
            builder.Services
                .AddHttpClient(HttpClients.SemanticKernelAPIClient,
                    httpClient =>
                    {
                        httpClient.BaseAddress = new Uri(builder.Configuration["FoundationaLLM:SemanticKernelAPI:APIUrl"]);
                        httpClient.DefaultRequestHeaders.Add("X-API-KEY", builder.Configuration[
                            builder.Configuration["FoundationaLLM:SemanticKernelAPI:APIKeySecretName"]]);
                    })
                .AddTransientHttpErrorPolicy(policyBuilder =>
                    policyBuilder.WaitAndRetryAsync(
                        3, retryNumber => TimeSpan.FromMilliseconds(600)));

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