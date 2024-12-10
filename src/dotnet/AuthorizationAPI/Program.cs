using FoundationaLLM;
using FoundationaLLM.AuthorizationEngine.Middleware;
using FoundationaLLM.Common.Authentication;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Constants.Configuration;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Validation;

var builder = WebApplication.CreateBuilder(args);

DefaultAuthentication.Initialize(
    builder.Environment.IsProduction(),
    ServiceNames.AuthorizationAPI);

builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", false, true);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddAzureKeyVault(
    new Uri(Environment.GetEnvironmentVariable(EnvironmentVariables.FoundationaLLM_AuthorizationAPI_KeyVaultURI)!),
    DefaultAuthentication.AzureCredential);
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddJsonFile("appsettings.development.json", true, true);

// Add services to the container.
builder.AddAzureKeyVaultService(
    EnvironmentVariables.FoundationaLLM_AuthorizationAPI_KeyVaultURI);

// Resource validation.
builder.Services.AddSingleton<IResourceValidatorFactory, ResourceValidatorFactory>();

// Authorization core.
builder.AddAuthorizationCore();

// CORS policies
builder.AddCorsPolicies();

// Add authentication configuration.
builder.AddAuthenticationConfiguration(
    AuthorizationKeyVaultSecretNames.FoundationaLLM_APIEndpoints_AuthorizationAPI_Configuration_Entra_Instance,
    AuthorizationKeyVaultSecretNames.FoundationaLLM_APIEndpoints_AuthorizationAPI_Configuration_Entra_TenantId,
    AuthorizationKeyVaultSecretNames.FoundationaLLM_APIEndpoints_AuthorizationAPI_Configuration_Entra_ClientId,
    null,
    policyName: "RequiredClaims",
    requireScopes: false,
    allowACLAuthorization: true);

// Add OpenTelemetry.
builder.AddOpenTelemetry(
    AuthorizationKeyVaultSecretNames.FoundationaLLM_APIEndpoints_AuthorizationAPI_AppInsightsConnectionString,
    ServiceNames.AuthorizationAPI);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.

// Set the CORS policy before other middleware.
app.UseCors(CorsPolicyNames.AllowAllOrigins);

app.UseAuthentication();
app.UseAuthorization();

// Register the middleware to authorize the processing of authorization requests.
app.UseMiddleware<AuthorizationAPIMiddleware>();

app.UseExceptionHandler(exceptionHandlerApp
    => exceptionHandlerApp.Run(async context
        => await Results.Problem().ExecuteAsync(context)));

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
