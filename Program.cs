using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using Supabase;
using TaskApi.Common;
using TaskApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
// 1. Fetch Supabase URL
var supabaseUrl = builder.Configuration["Supabase:Url"] 
    ?? throw new InvalidOperationException("Supabase URL missing");
var supabaseKey = builder.Configuration["Supabase:AnonKey"] 
    ?? throw new InvalidOperationException("Supabase Key missing");

// 2. Register Native .NET JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Supabase issues tokens with Authority = https://<project-ref>.supabase.co/auth/v1
        options.Authority = $"{supabaseUrl}/auth/v1";
        options.Audience = "authenticated"; // Default audience for Supabase Auth JWTs

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = $"{supabaseUrl}/auth/v1",
            ValidateAudience = true,
            ValidAudience = "authenticated",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // Strict token expiration handling
        };
    });

builder.Services.AddAuthorization();

// 3. Register Supabase Client SDK (used ONLY for signup/login/logout operations)
builder.Services.AddScoped<Supabase.Client>(_ => 
    new Supabase.Client(supabaseUrl, supabaseKey, new SupabaseOptions
    {
        AutoRefreshToken = true,
        AutoConnectRealtime = false
    }));

// =========================================================================
// 1. LOGGING CONFIGURATION
// =========================================================================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddFilter("Microsoft.AspNetCore.Diagnostics.ExceptionHandlerMiddleware", LogLevel.None);

// =========================================================================
// 2. INFRASTRUCTURE & DATABASE (PostgreSQL & Redis)
// =========================================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? builder.Configuration.GetConnectionString("Default");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention());

// Configure Redis Connection Options (Graceful retry on boot)
// Change "localhost:6379" to "redis:6379"
var redisConnectionString = builder.Configuration["REDIS_CONNECTION"] ?? "redis:6379";
var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
redisOptions.AbortOnConnectFail = false; // Prevents container crash on initial startup delay
redisOptions.ConnectTimeout = 5000;
redisOptions.ConnectRetry = 5;

var redisConnection = ConnectionMultiplexer.Connect(redisOptions);
builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);

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

// BUILD THE APP CONTAINER
var app = builder.Build();

// =========================================================================
// 4. RUNTIME PING & REDIS VERIFICATION
// =========================================================================
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var redis = app.Services.GetRequiredService<IConnectionMultiplexer>();
var redisDb = redis.GetDatabase();

var latency = await redisDb.PingAsync();

// Use the source-generated LoggerMessage extension method
logger.RedisConnected(redisConnectionString, latency.TotalMilliseconds);
// Clickable Host URL Log for Terminal
logger.LogInformation("🚀 API Docs available at: http://localhost:5131/docs");
// =========================================================================
// 5. MIDDLEWARE PIPELINE
// =========================================================================
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication(); // 1. Extracts & validates JWT signature
app.UseAuthorization();  // 2. Evaluates access policy

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
// 6. MAP ENDPOINTS
// =========================================================================
TaskApi.Features.System.GetRoot.Map(app);
TaskApi.Features.System.HealthChecks.Map(app);
TaskApi.Features.Tasks.CreateTask.Map(app);
TaskApi.Features.Tasks.GetTasks.Map(app);
TaskApi.Features.Tasks.GetTaskById.Map(app);
TaskApi.Features.Tasks.UpdateTask.Map(app);
TaskApi.Features.Tasks.DeleteTask.Map(app);

app.Run();
