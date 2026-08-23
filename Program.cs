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

// Reads configuration dynamically from appsettings.json
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

// Silence duplicate framework exception logs (since GlobalExceptionHandler handles it)
builder.Logging.AddFilter("Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware", LogLevel.None);

// =========================================================================
// 2. INFRASTRUCTURE & DATABASE
// =========================================================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

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

// Register all FluentValidation validators automatically from the assembly
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Register Feature Handlers
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

// 2. CONFIGURE HTTP PIPELINE
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Task API v1");
        
        // This line changes the URL from /swagger to /docs
        c.RoutePrefix = "docs"; 
    });
}

// Dev Test Endpoint for 500 Error Testing
if (app.Environment.IsDevelopment())
{
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

// =========================================================================
// 6. AUTOMATIC DATABASE MIGRATION & STARTUP
// =========================================================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();