using FoundationaLLM.SemanticKernelAPI.Interfaces;
using FoundationaLLM.SemanticKernelAPI.Models.ConfigurationOptions;
using FoundationaLLM.SemanticKernelAPI.Services;

namespace SemanticKernelAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddApplicationInsightsTelemetry();

            builder.Services.AddOptions<SemanticKernelServiceSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:SemanticKernelService"));
            builder.Services.AddSingleton<ISemanticKernelService, SemanticKernelService>();

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

            app.UseHttpsRedirection();
            app.UseAuthorization();

            // Map the chat REST endpoints:
            using (var scope = app.Services.CreateScope())
            {
                var service = scope.ServiceProvider.GetService<SemanticKernelEndpoints>();
                service?.Map(app);
            }

            app.Run();
        }
    }
}