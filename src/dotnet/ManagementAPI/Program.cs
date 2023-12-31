using Asp.Versioning;
using Azure.Identity;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Net.Http;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Middleware;
using FoundationaLLM.Common.Models.Configuration.Branding;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.OpenAPI;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Common.Settings;
using FoundationaLLM.Management.Interfaces;
using FoundationaLLM.Management.Models.Configuration;
using FoundationaLLM.Management.Services;
using Microsoft.Identity.Web;

namespace FoundationaLLM.Management.API
{
    /// <summary>
    /// Main entry point for the Management API.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Management API service configuration.
        /// </summary>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.Sources.Clear();
            builder.Configuration.AddJsonFile("appsettings.json", false, true);
            builder.Configuration.AddEnvironmentVariables();
            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                options.Connect(builder.Configuration["FoundationaLLM:AppConfig:ConnectionString"]);
                options.ConfigureKeyVault(options => { options.SetCredential(new DefaultAzureCredential()); });
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIs);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_CosmosDB);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_Branding);
                options.Select(AppConfigurationKeyFilters.FoundationaLLM_ManagementAPI_Entra);
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
                        policy.WithHeaders("DNT", "Keep-Alive", "User-Agent", "X-Requested-With", "If-Modified-Since",
                            "Cache-Control", "Content-Type", "Range", "Authorization", "X-AGENT-HINT");
                        policy.AllowAnyMethod();
                    });
            });

            builder.Services.AddOptions<CosmosDbSettings>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_CosmosDB));
            builder.Services.AddOptions<ClientBrandingConfiguration>()
                .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Branding));
            builder.Services.AddOptions<AppConfigurationSettings>()
                .Configure(o =>
                    o.ConnectionString = builder.Configuration["FoundationaLLM:AppConfig:ConnectionString"]);

            builder.Services.AddScoped<IConfigurationManagementService, ConfigurationManagementService>();

            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            builder.Services.AddScoped<ICallContext, CallContext>();
            //builder.Services.AddScoped<IHttpClientFactoryService, HttpClientFactoryService>();

            // Register the authentication services
            RegisterAuthConfiguration(builder);

            builder.Services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
            {
                ConnectionString = builder.Configuration["FoundationaLLM:APIs:ManagementAPI:AppInsightsConnectionString"],
                DeveloperMode = builder.Environment.IsDevelopment()
            });
            //builder.Services.AddServiceProfiler();
            builder.Services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = FoundationaLLM.Common.Settings
                    .CommonJsonSerializerSettings
                    .GetJsonSerializerSettings().ContractResolver;
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

            // Set the CORS policy before other middleware.
            app.UseCors(allowAllCorsOrigins);

            // For the CoreAPI, we need to make sure that UseAuthentication is called before the UserIdentityMiddleware.
            app.UseAuthentication();
            app.UseAuthorization();

            // Register the middleware to extract the user identity context and other HTTP request context data required by the downstream services.
            app.UseMiddleware<CallContextMiddleware>();

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

            app.Run();
        }

        /// <summary>
        /// Register the authentication services.
        /// </summary>
        /// <param name="builder"></param>
        public static void RegisterAuthConfiguration(WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(jwtOptions => { },
                    identityOptions =>
                    {
                        identityOptions.ClientSecret =
                            builder.Configuration["FoundationaLLM:ManagementAPI:Entra:ClientSecret"];
                        identityOptions.Instance = builder.Configuration["FoundationaLLM:ManagementAPI:Entra:Instance"] ?? "";
                        identityOptions.TenantId = builder.Configuration["FoundationaLLM:ManagementAPI:Entra:TenantId"];
                        identityOptions.ClientId = builder.Configuration["FoundationaLLM:ManagementAPI:Entra:ClientId"];
                    });
            //.EnableTokenAcquisitionToCallDownstreamApi()
            //.AddInMemoryTokenCaches();

            //builder.Services.AddScoped<IAuthenticatedHttpClientFactory, EntraAuthenticatedHttpClientFactory>();
            builder.Services.AddScoped<IUserClaimsProviderService, EntraUserClaimsProviderService>();

            // Configure the scope used by the API controllers:
            var requiredScope = builder.Configuration["FoundationaLLM:ManagementAPI:Entra:Scopes"] ?? "";
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("RequiredScope", policyBuilder =>
                {
                    policyBuilder.RequireAuthenticatedUser();
                    policyBuilder.RequireClaim("http://schemas.microsoft.com/identity/claims/scope",
                        requiredScope.Split(' '));
                });
            });
        }
    }
}
