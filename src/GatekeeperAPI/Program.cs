using FoundationaLLM.GatekeeperAPI.Interfaces;
using FoundationaLLM.GatekeeperAPI.Models.ConfigurationOptions;
using FoundationaLLM.GatekeeperAPI.Services;

namespace FoundationaLLM.GatekeeperAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddApplicationInsightsTelemetry();

            builder.Services.AddOptions<GatekeeperServiceSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:GatekeeperService"));
            builder.Services.AddSingleton<IGatekeeperService, GatekeeperService>();

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
                var service = scope.ServiceProvider.GetService<GatekeeperEndpoints>();
                service?.Map(app);
            }

            app.Run();
        }
    }
}