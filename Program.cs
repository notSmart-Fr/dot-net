using TaskApi.Features.Auth;
using TaskApi.Infrastructure.ApiDocs;
using TaskApi.Shared.Caching;
using TaskApi.Shared.Database;
using TaskApi.Shared.ExceptionHandling;
using TaskApi.Shared.Extensions;
using TaskApi.Shared.Telemetry;
using TaskApi.Features.Scraper;

var builder = WebApplication.CreateBuilder(args);

// 1. Co-Located Infrastructure Modules
builder.Services.AddDatabaseInfrastructure(builder.Configuration);
builder.Services.AddRedisInfrastructure(builder.Configuration);
builder.Services.AddAuthInfrastructure(builder.Configuration);
builder.Services.AddTelemetryInfrastructure(builder.Configuration);
builder.Services.AddExceptionHandlingInfrastructure(); // Auto-scans all IExceptionMappers
builder.Services.AddApiDocsInfrastructure();

// 2. Feature Application Services
builder.Services.AddFeatureInfrastructure(); // Auto-scans all Handlers & Validators
// 2.1. Scraper Feature
builder.Services.AddScraperFeature();
var app = builder.Build();
//verify Redis connection
await app.VerifyRedisConnectionAsync();

// 3. Request Pipeline Orchestration
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseApiDocsDevelopmentUI();
}

app.MapApplicationEndpoints(); // Auto-scans and registers all IEndpoint implementations

app.Run();