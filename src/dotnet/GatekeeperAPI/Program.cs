using Asp.Versioning;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Middleware;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Models.Configuration;
using FoundationaLLM.Common.OpenAPI;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Gatekeeper.Core.Interfaces;
using FoundationaLLM.Gatekeeper.Core.Models.ConfigurationOptions;
using FoundationaLLM.Gatekeeper.Core.Services;
using Microsoft.Extensions.Options;
using Polly;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FoundationaLLM.Gatekeeper.API
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
            builder.Services.AddScoped<APIKeyAuthenticationFilter>();
            builder.Services.AddOptions<APIKeyValidationSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:GatekeeperAPI"));
            builder.Services.AddOptions<KeyVaultConfigurationServiceSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:Configuration"));

            // Register the downstream services and HTTP clients.
            RegisterDownstreamServices(builder);


            builder.Services.AddTransient<IAPIKeyValidationService, APIKeyValidationService>();
            builder.Services.AddSingleton<IConfigurationService, KeyVaultConfigurationService>();

            builder.Services.AddOptions<RefinementServiceSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:Refinement"));
            builder.Services.AddScoped<IRefinementService, RefinementService>();

            builder.Services.AddOptions<AzureContentSafetySettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:AzureContentSafety"));
            builder.Services.AddScoped<IContentSafetyService, AzureContentSafetyService>();

            builder.Services.AddScoped<IAgentFactoryAPIService, AgentFactoryAPIService>();

            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            builder.Services.AddScoped<IUserIdentityContext, UserIdentityContext>();
            builder.Services.AddScoped<IHttpClientFactoryService, HttpClientFactoryService>();
            builder.Services.AddScoped<IUserClaimsProviderService, NoOpUserClaimsProviderService>();
            builder.Services.AddScoped<IGatekeeperService, GatekeeperService>();

            builder.Services
                .AddHttpClient(HttpClients.AgentFactoryAPIClient,
                    httpClient =>
                    {
                        httpClient.BaseAddress = new Uri(builder.Configuration["FoundationaLLM:AgentFactoryAPI:APIUrl"]);
                        //httpClient.DefaultRequestHeaders.Add("X-API-KEY", builder.Configuration["FoundationaLLM:AgentFactoryAPI:APIKey"]);
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

            // Register the middleware to set the user identity context.
            app.UseMiddleware<UserIdentityMiddleware>();

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

        /// <summary>
        /// Bind the downstream API settings to the configuration and register the HTTP clients.
        /// </summary>
        /// <param name="builder"></param>
        private static void RegisterDownstreamServices(WebApplicationBuilder builder)
        {
            var downstreamAPISettings = new DownstreamAPISettings
            {
                DownstreamAPIs = new Dictionary<string, DownstreamAPIKeySettings>()
            };
            foreach (var apiSetting in builder.Configuration.GetSection("FoundationaLLM:DownstreamAPIs").GetChildren())
            {
                var key = apiSetting.Key;
                var settings = apiSetting.Get<DownstreamAPIKeySettings>();
                downstreamAPISettings.DownstreamAPIs[key] = settings;
                builder.Services
                    .AddHttpClient(key, client => { client.BaseAddress = new Uri(settings.APIUrl); })
                    .AddTransientHttpErrorPolicy(policyBuilder =>
                        policyBuilder.WaitAndRetryAsync(
                            3, retryNumber => TimeSpan.FromMilliseconds(600)));
            }

            builder.Services.AddSingleton<IDownstreamAPISettings>(downstreamAPISettings);
        }
    }
}