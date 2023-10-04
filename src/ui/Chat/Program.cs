using FoundationaLLM.Chat.Helpers;
using Polly;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient(FoundationaLLM.Core.Constants.HttpClients.DefaultHttpClient,
        httpClient =>
        {
            httpClient.BaseAddress = new Uri(builder.Configuration["FoundationaLLM:ChatManager:APIUrl"]);
        })
    .AddTransientHttpErrorPolicy(policyBuilder =>
        policyBuilder.WaitAndRetryAsync(
            3, retryNumber => TimeSpan.FromMilliseconds(600)));

builder.RegisterConfiguration();
builder.Services.AddRazorPages();
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddServerSideBlazor();
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
        services.AddSingleton<IChatManager, ChatManager>();
    }
}
