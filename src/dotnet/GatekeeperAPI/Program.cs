using FoundationaLLM.Common.Constants;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions;
using FoundationaLLM.Gatekeeper.Core.Services;
using Polly;

namespace FoundationaLLM.Gatekeeper.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddApplicationInsightsTelemetry();
            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            builder.Services.AddApiVersioning();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSingleton<IAgentFactoryAPIService, AgentFactoryAPIService>();

            builder.Services
                .AddHttpClient(HttpClients.AgentFactoryAPIClient,
                    httpClient =>
                    {
                        httpClient.BaseAddress = new Uri(builder.Configuration["FoundationaLLM:GatekeeperApi:APIUrl"]);
                        httpClient.DefaultRequestHeaders.Add("X-API-KEY", builder.Configuration["FoundationaLLM:GatekeeperApi:APIKey"]);
                    })
                .AddTransientHttpErrorPolicy(policyBuilder =>
                    policyBuilder.WaitAndRetryAsync(
                        3, retryNumber => TimeSpan.FromMilliseconds(600)));

            builder.Services.AddOptions<RefinementServiceSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:Refinement"));
            builder.Services.AddScoped<IRefinementService, RefinementService>();

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