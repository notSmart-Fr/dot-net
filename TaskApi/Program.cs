using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskApi.Common;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. CONFIGURE LOGGING
// =========================================================================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Silence ASP.NET Core framework internal deserialization stack traces
builder.Logging.AddFilter("Microsoft.AspNetCore.Http.RequestDelegateFactory", LogLevel.None);
builder.Logging.AddFilter("Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware", LogLevel.None);
builder.Logging.AddFilter("Microsoft.AspNetCore.Routing", LogLevel.Warning);

// =========================================================================
// 2. REGISTER SERVICES & GLOBAL EXCEPTION HANDLER
// =========================================================================
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddScoped<TaskApi.Features.Tasks.CreateTask.Handler>();

builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// =========================================================================
// 3. MIDDLEWARE PIPELINE
// =========================================================================

// Enables the registered IExceptionHandler
app.UseExceptionHandler();
// Converts unmapped routes (404) and other status codes into ProblemDetails JSON
app.UseStatusCodePages();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Bug test endpoint (Throws an unhandled exception to test 500 Error handling)
app.MapGet("/bug", _ => throw new Exception("Database connection failed!"));

// Map Feature Endpoints
TaskApi.Features.System.GetRoot.Map(app);
TaskApi.Features.System.HealthChecks.Map(app);
TaskApi.Features.Tasks.CreateTask.Map(app);

// Automatic DB Migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

// Startup Output
app.Start();

var addresses = app.Urls.Count > 0 
    ? string.Join(", ", app.Urls) 
    : "http://localhost:5131";

var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Now listening on: {Addresses}", addresses);
logger.LogInformation("Swagger UI available at: {Addresses}/swagger", addresses.Split(',')[0]);

await app.WaitForShutdownAsync();