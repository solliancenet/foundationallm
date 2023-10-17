using Azure.Identity;
using FoundationaLLM.Chat.Helpers;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.Configuration;
using FoundationaLLM.Common.Models.Configuration.Authentication;
using FoundationaLLM.Common.Services;
using FoundationaLLM.Core.Interfaces;
using FoundationaLLM.Core.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Polly;
using System;

var builder = WebApplication.CreateBuilder(args);


builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(builder.Configuration["FoundationaLLM:AppConfig:ConnectionString"]);
    options.ConfigureKeyVault(options =>
    {
        options.SetCredential(new DefaultAzureCredential());
    });
});

builder.Services.AddHttpClient(FoundationaLLM.Common.Constants.HttpClients.CoreAPI,
        httpClient =>
        {
            httpClient.BaseAddress = new Uri(builder.Configuration["FoundationaLLM:ChatManager:APIUrl"]);
        })
    .AddTransientHttpErrorPolicy(policyBuilder =>
        policyBuilder.WaitAndRetryAsync(
            3, retryNumber => TimeSpan.FromMilliseconds(600)));

builder.Services.Configure<EntraSettings>(builder.Configuration.GetSection("FoundationaLLM:Chat:Entra"));
builder.Services.AddOptions<KeyVaultConfigurationServiceSettings>()
    .Bind(builder.Configuration.GetSection("FoundationaLLM:Configuration"));

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        options.ClientSecret = builder.Configuration["FoundationaLLM:Chat:Entra:ClientSecret"]; ;
        options.Instance = builder.Configuration["FoundationaLLM:Chat:Entra:Instance"];
        options.TenantId = builder.Configuration["FoundationaLLM:Chat:Entra:TenantId"];
        options.ClientId = builder.Configuration["FoundationaLLM:Chat:Entra:ClientId"];
        options.CallbackPath = builder.Configuration["FoundationaLLM:Chat:Entra:CallbackPath"];
    })
    .EnableTokenAcquisitionToCallDownstreamApi(new string[] { builder.Configuration["FoundationaLLM:Chat:Entra:Scopes"] })
    .AddInMemoryTokenCaches();
builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();
builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.RegisterConfiguration();
builder.Services.AddRazorPages();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddServerSideBlazor()
    .AddMicrosoftIdentityConsentHandler();
builder.Services.RegisterServices();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages(); // If Razor pages
    endpoints.MapControllers();
});

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

await app.RunAsync();

static class ProgramExtensions
{
    public static void RegisterConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<ChatManagerSettings>()
            .Bind(builder.Configuration.GetSection("FoundationaLLM:ChatManager"));
    }

    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<IChatManager, ChatManager>();
        services.AddScoped<IBrandManager, BrandManager>();
        services.AddScoped<IBrandingService, BrandingService>();
        services.AddScoped<IAuthenticatedHttpClientFactory, EntraAuthenticatedHttpClientFactory>();
    }
}
