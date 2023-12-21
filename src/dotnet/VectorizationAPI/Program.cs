using Asp.Versioning;
using Azure.Identity;
using FoundationaLLM.Common.OpenAPI;

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
    options.Select("FoundationaLLM:Vectorization:*");
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
            policy.WithHeaders("DNT", "Keep-Alive", "User-Agent", "X-Requested-With", "If-Modified-Since", "Cache-Control", "Content-Type", "Range", "Authorization", "X-AGENT-HINT");
            policy.AllowAnyMethod();
        });
});

// Add services to the container.

builder.Services.AddControllers();
builder.Services
    .AddApiVersioning(options =>
     {
         // Reporting api versions will return the headers
         // "api-supported-versions" and "api-deprecated-versions"
         options.ReportApiVersions = true;
         options.AssumeDefaultVersionWhenUnspecified = true;
         options.DefaultApiVersion = new ApiVersion(1, 0);
     })
    .AddApiExplorer(options =>
    {
        // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
        // note: the specified format code will format the version as "'v'major[.minor][-status]"
        options.GroupNameFormat = "'v'VVV";
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenNewtonsoftSupport();
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
