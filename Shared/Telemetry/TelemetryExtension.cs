using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace TaskApi.Shared.Telemetry;

public static class TelemetryExtensions
{
    public static IServiceCollection AddTelemetryInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var serviceName = configuration["OTEL_SERVICE_NAME"] ?? "TaskApi";
        var endpointStr = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] ?? "http://aspire_dashboard:18889";
        var endpoint = new Uri(endpointStr);

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(opts =>
                    {
                        opts.Filter = httpContext => !httpContext.Request.Path.Value?.StartsWith("/health") == true;
                    })
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = endpoint;
                        opts.Protocol = OtlpExportProtocol.Grpc;
                    });
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = endpoint;
                        opts.Protocol = OtlpExportProtocol.Grpc;
                    });
            })
            .WithLogging(logging =>
            {
                logging.AddOtlpExporter(opts =>
                {
                    opts.Endpoint = endpoint;
                    opts.Protocol = OtlpExportProtocol.Grpc;
                });
            });

        services.Configure<OpenTelemetryLoggerOptions>(opts =>
        {
            opts.IncludeFormattedMessage = true;
            opts.IncludeScopes = true;
        });

        return services;
    }
}