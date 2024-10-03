using Asp.Versioning;
using FoundationaLLM;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Extensions;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Middleware;
using FoundationaLLM.Common.Models.Configuration.CosmosDB;
using FoundationaLLM.Common.Models.Configuration.Instance;
using FoundationaLLM.Common.Models.Context;
using FoundationaLLM.Common.OpenAPI;
using FoundationaLLM.Common.Services.Security;
using FoundationaLLM.State.Interfaces;
using FoundationaLLM.State.Serializers;
using FoundationaLLM.State.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

DefaultAuthentication.Initialize(
    builder.Environment.IsProduction(),
    ServiceNames.StateAPI);

builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", false, true);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(builder.Configuration[EnvironmentVariables.FoundationaLLM_AppConfig_ConnectionString]);
    options.ConfigureKeyVault(options =>
    {
        options.SetCredential(DefaultAuthentication.AzureCredential);
    });
    options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_StateAPI_Essentials);
    options.Select(AppConfigurationKeyFilters.FoundationaLLM_Instance);
    options.Select(AppConfigurationKeyFilters.FoundationaLLM_Logging);
    options.Select(AppConfigurationKeyFilters.FoundationaLLM_APIEndpoints_StateAPI_Configuration_CosmosDB);
});
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddJsonFile("appsettings.development.json", true, true);

builder.AddOpenTelemetry(
    AppConfigurationKeys.FoundationaLLM_APIEndpoints_StateAPI_Essentials_AppInsightsConnectionString,
    ServiceNames.StateAPI);

builder.Services.AddInstanceProperties(builder.Configuration);

// CORS policies
builder.AddCorsPolicies();

// Generic exception handling
builder.AddStateGenericExceptionHandling();

// Open API (Swagger)
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

builder.Services.AddControllers();

// Add API Key Authorization
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICallContext, CallContext>();
builder.Services.AddScoped<IUserClaimsProviderService, NoOpUserClaimsProviderService>();
builder.Services.AddScoped<APIKeyAuthenticationFilter>();
builder.Services.AddOptions<APIKeyValidationSettings>()
    .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_StateAPI_Essentials));
builder.Services.AddTransient<IAPIKeyValidationService, APIKeyValidationService>();
builder.Services.AddOptions<InstanceSettings>()
    .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_Instance));

builder.Services.AddOptions<CosmosDbSettings>()
    .Bind(builder.Configuration.GetSection(AppConfigurationKeySections.FoundationaLLM_APIEndpoints_StateAPI_Configuration_CosmosDB));

builder.Services.AddSingleton<CosmosClient>(serviceProvider =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<CosmosDbSettings>>().Value;
    var opt = new JsonSerializerOptions()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    // Configure CosmosSystemTextJsonSerializer
    var serializer
        = new CosmosSystemTextJsonSerializer(opt);
    return new CosmosClientBuilder(settings.Endpoint, DefaultAuthentication.AzureCredential)
        .WithCustomSerializer(serializer)
        .WithConnectionModeGateway()
        .Build();
});

// Add authorization services.
builder.AddGroupMembership();

// Add services to the container.
builder.Services.AddAuthorization();

builder.Services.AddScoped<ICosmosDbService, CosmosDbService>();
builder.AddStateService();

builder.Services
    .AddApiVersioning(options =>
    {
        // Reporting api versions will return the headers
        // "api-supported-versions" and "api-deprecated-versions"
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(new DateOnly(2024, 2, 16));
    })
    .AddMvc()
    .AddApiExplorer();

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

        // Adds auth via X-API-KEY header
        options.AddAPIKeyAuth();
    })
    .AddSwaggerGenNewtonsoftSupport();

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.

// Set the CORS policy before other middleware.
app.UseCors(CorsPolicyNames.AllowAllOrigins);

// Register the middleware to extract the user identity context and other HTTP request context data required by the downstream services.
app.UseMiddleware<CallContextMiddleware>();

app.UseExceptionHandler(exceptionHandlerApp
    => exceptionHandlerApp.Run(async context
        => await Results.Problem().ExecuteAsync(context)));

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
