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
// Register GetTasks.Handler in DI
builder.Services.AddScoped<TaskApi.Features.Tasks.GetTasks.Handler>();
builder.Services.AddScoped<TaskApi.Features.Tasks.GetTaskById.Handler>();
// Register UpdateTask.Handler in DI
builder.Services.AddScoped<TaskApi.Features.Tasks.UpdateTask.Handler>();
// Register DeleteTask.Handler in DI
builder.Services.AddScoped<TaskApi.Features.Tasks.DeleteTask.Handler>();

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
TaskApi.Features.Tasks.GetTasks.Map(app);
TaskApi.Features.Tasks.GetTaskById.Map(app);
TaskApi.Features.Tasks.UpdateTask.Map(app);
TaskApi.Features.Tasks.DeleteTask.Map(app);

// Automatic DB Migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

// Startup Output
app.Start();

await app.WaitForShutdownAsync();