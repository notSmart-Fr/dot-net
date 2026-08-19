var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks(); // Adds Health Check Services
// 1. Add OpenAPI & Swagger Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 2. Enable Swagger Middleware in Development Mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();   // Serves /swagger/v1/swagger.json
    app.UseSwaggerUI(); // Serves the interactive Web UI at /swagger
}

// Map endpoints...
TaskApi.Features.System.GetRoot.Map(app);
TaskApi.Features.System.HealthChecks.Map(app);

app.Run();