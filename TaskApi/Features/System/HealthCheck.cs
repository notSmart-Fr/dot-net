using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TaskApi.Features.System;

public static class HealthChecks
{
    public static void Map(IEndpointRouteBuilder app)
    {
        // Exposes GET /health (Returns 200 OK "Healthy" or 503 "Unhealthy")
        app.MapHealthChecks("/health");
    }
}