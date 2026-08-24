using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TaskApi.Common;
using TaskApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// 1. LOGGING CONFIGURATION
// =========================================================================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddFilter("Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware", LogLevel.None);

// =========================================================================
// 2. INFRASTRUCTURE & DATABASE (PostgreSQL)
// =========================================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention());

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(
        name: "database", 
        tags: ["ready"])
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"]);

// =========================================================================
// 3. APPLICATION SERVICES & HANDLERS
// =========================================================================
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Feature Handlers
builder.Services.AddScoped<TaskApi.Features.Tasks.CreateTask.Handler>();
builder.Services.AddScoped<TaskApi.Features.Tasks.GetTasks.Handler>();
builder.Services.AddScoped<TaskApi.Features.Tasks.GetTaskById.Handler>();
builder.Services.AddScoped<TaskApi.Features.Tasks.UpdateTask.Handler>();
builder.Services.AddScoped<TaskApi.Features.Tasks.DeleteTask.Handler>();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// =========================================================================
// 4. MIDDLEWARE PIPELINE
// =========================================================================
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task API v1");
        c.RoutePrefix = "docs"; 
    });

    app.MapGet("/bug", _ => throw new Exception("Database connection failed!"));
}

// =========================================================================
// 5. MAP ENDPOINTS
// =========================================================================
TaskApi.Features.System.GetRoot.Map(app);
TaskApi.Features.System.HealthChecks.Map(app);
TaskApi.Features.Tasks.CreateTask.Map(app);
TaskApi.Features.Tasks.GetTasks.Map(app);
TaskApi.Features.Tasks.GetTaskById.Map(app);
TaskApi.Features.Tasks.UpdateTask.Map(app);
TaskApi.Features.Tasks.DeleteTask.Map(app);

app.Run();