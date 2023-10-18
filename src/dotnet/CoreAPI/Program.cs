using System.Security.Claims;
using Asp.Versioning;
using FoundationaLLM.Common.OpenAPI;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Services;
using Microsoft.Extensions.Options;
using Polly;
using Swashbuckle.AspNetCore.SwaggerGen;
using FoundationaLLM.Common.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration;
using Microsoft.Identity.Client;
using FoundationaLLM.Core.Models.Configuration;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Models.Authentication;
using FoundationaLLM.Common.Middleware;
using FoundationaLLM.Common.Models.Configuration.Branding;
using Newtonsoft.Json;
using Azure.Identity;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;

namespace FoundationaLLM.Core.API
{
    public class Program
    {
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
            });
            if (builder.Environment.IsDevelopment())
                builder.Configuration.AddJsonFile("appsettings.development.json", true, true);
              
            var allowAllCorsOrigins = "AllowAllOrigins";
            builder.Services.AddCors(policyBuilder =>
            {
                policyBuilder.AddPolicy(allowAllCorsOrigins,
                    policy =>
                    {
                        policy.AllowAnyOrigin();
                        policy.AllowAnyHeader();
                        policy.AllowAnyMethod();
                    });
            });

            builder.Services.AddOptions<CosmosDbSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:CosmosDB"));
            builder.Services.AddOptions<KeyVaultConfigurationServiceSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:Configuration"));
            builder.Services.AddOptions<ClientBrandingConfiguration>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:Branding"));

            // Register the downstream services and HTTP clients.
            RegisterDownstreamServices(builder);

            builder.Services.AddScoped<ICosmosDbService, CosmosDbService>();
            builder.Services.AddScoped<ICoreService, CoreService>();
            builder.Services.AddScoped<IGatekeeperAPIService, GatekeeperAPIService>();

            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            builder.Services.AddScoped<IUserIdentityContext, UserIdentityContext>();
            builder.Services.AddScoped<IHttpClientFactoryService, HttpClientFactoryService>();

            // Register the authentication services
            RegisterAuthConfiguration(builder);

            builder.Services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
            {
                ConnectionString = builder.Configuration["FoundationaLLM:APIs:CoreAPI:AppInsightsConnectionString"],
                DeveloperMode = builder.Environment.IsDevelopment()
            });

            builder.Services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = Common.Settings.CommonJsonSerializerSettings.GetJsonSerializerSettings().ContractResolver;
            });
            builder.Services.AddProblemDetails();
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

            // For the CoreAPI, we need to make sure that UseAuthentication is called before the UserIdentityMiddleware.
            app.UseAuthentication();
            app.UseAuthorization();

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
            app.MapControllers();

            app.UseCors(allowAllCorsOrigins);

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

            var gatekeeperAPISettings = new DownstreamAPIKeySettings
            {
                APIUrl = builder.Configuration[$"FoundationaLLM:APIs:{HttpClients.GatekeeperAPI}:APIUrl"],
                APIKey = builder.Configuration[$"FoundationaLLM:APIs:{HttpClients.GatekeeperAPI}:APIKey"]
            };
            downstreamAPISettings.DownstreamAPIs[HttpClients.GatekeeperAPI] = gatekeeperAPISettings;

            builder.Services
                    .AddHttpClient(HttpClients.GatekeeperAPI,
                        client => { client.BaseAddress = new Uri(gatekeeperAPISettings.APIUrl); })
                    .AddTransientHttpErrorPolicy(policyBuilder =>
                        policyBuilder.WaitAndRetryAsync(
                            3, retryNumber => TimeSpan.FromMilliseconds(600)));

            builder.Services.AddSingleton<IDownstreamAPISettings>(downstreamAPISettings);
        }

        public static void RegisterAuthConfiguration(WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(jwtOptions =>
                    {

                    },
                    identityOptions =>
                    {
                        identityOptions.ClientSecret = builder.Configuration["FoundationaLLM:CoreAPI:Entra:ClientSecret"];
                        identityOptions.Instance = builder.Configuration["FoundationaLLM:CoreAPI:Entra:Instance"];
                        identityOptions.TenantId = builder.Configuration["FoundationaLLM:CoreAPI:Entra:TenantId"];
                        identityOptions.ClientId = builder.Configuration["FoundationaLLM:CoreAPI:Entra:ClientId"];
                        identityOptions.CallbackPath = builder.Configuration["FoundationaLLM:CoreAPI:Entra:CallbackPath"];
                    });
                //.EnableTokenAcquisitionToCallDownstreamApi()
                //.AddInMemoryTokenCaches();

            //builder.Services.AddScoped<IAuthenticatedHttpClientFactory, EntraAuthenticatedHttpClientFactory>();
            builder.Services.AddScoped<IUserClaimsProviderService, EntraUserClaimsProviderService>();

            // Configure the scope used by the API controllers:
            var requiredScope = builder.Configuration["FoundationaLLM:CoreAPI:Entra:Scopes"];
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequiredScope", policyBuilder =>
                {
                    policyBuilder.RequireAuthenticatedUser();
                    policyBuilder.RequireClaim("http://schemas.microsoft.com/identity/claims/scope", requiredScope.Split(' '));
                });
            });
        }
    }
}