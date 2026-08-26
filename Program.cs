using TaskApi.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. Core & Infrastructure Services
builder.AddCustomLogging();
builder.Services.AddGlobalExceptionInfrastructure();
builder.Services.AddSupabaseAuthentication(builder.Configuration);
builder.Services.AddDatabaseAndCaching(builder.Configuration);

// 2. Application Feature Services & Documentation
builder.Services.AddApplicationServices();
builder.Services.AddOpenApiDocumentation();

// 3. Build Container & Perform Boot Checks
var app = builder.Build();
await app.Services.VerifyInfrastructureConnectionsAsync(builder.Configuration);

// 4. Middleware Pipeline
app.UseGlobalMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseDevelopmentApiDocs();
}

// 5. Map Endpoints
app.MapApplicationEndpoints();

app.Run();