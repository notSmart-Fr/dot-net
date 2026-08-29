using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TaskApi.Core.Interfaces;

namespace TaskApi.Features.System;

public static class HealthChecks
{
    // ENDPOINT (Implements IEndpoint for Assembly Auto-Scanning)
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            // 1. Liveness Probe (/healthz/live) -> Returns 200 if the process is alive
            app.MapHealthChecks("/healthz/live", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("live")
            })
            .WithTags("System")
            .ExcludeFromDescription();

            // 2. Readiness Probe (/healthz/ready) -> Checks Database + Redis before sending traffic
            app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = WriteJsonResponse
            })
            .WithTags("System")
            .ExcludeFromDescription();
        }

        private static Task WriteJsonResponse(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                status = report.Status.ToString(),
                totalDuration = report.TotalDuration.TotalMilliseconds + "ms",
                entries = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds + "ms",
                    exception = e.Value.Exception?.Message
                })
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}