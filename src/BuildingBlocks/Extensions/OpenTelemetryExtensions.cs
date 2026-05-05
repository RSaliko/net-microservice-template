using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace BuildingBlocks.Extensions;

public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddOpenTelemetryObservability(this IServiceCollection services, IConfiguration configuration, string serviceName)
    {
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName)
            .AddTelemetrySdk();

        // 1. Configure OpenTelemetry Logging
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resourceBuilder);
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.AddOtlpExporter();
            });
        });

        // 2. Configure OpenTelemetry Tracing and Metrics
        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddSource(serviceName) // Custom spans
                    .AddOtlpExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics.SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddOtlpExporter();
            });

        // 3. Register ActivitySource for manual tracing
        services.AddSingleton(new ActivitySource(serviceName));

        return services;
    }
}
