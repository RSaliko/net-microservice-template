using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.HealthChecks;

/// <summary>
/// Extension methods for configuring health checks with proper separation of startup, 
/// readiness, and liveness probes per Kubernetes best practices.
/// 
/// Tag meanings:
/// - "startup": Checked during application startup (slow, resource-intensive checks)
/// - "ready": Checked for readiness (fast, dependency checks)
/// - "live": Checked for liveness (minimal overhead)
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds standard health checks for database, message broker, and cache.
    /// Tags health checks for use in startup, readiness, and liveness probes.
    /// </summary>
    public static IServiceCollection AddStandardHealthChecks(this IServiceCollection services, string? sqlConnectionString = null, string? rabbitMqHost = null)
    {
        var builder = services.AddHealthChecks();

        if (!string.IsNullOrEmpty(sqlConnectionString))
            // Database check: startup (slow), ready (dependency required), live (optional)
            builder.AddNpgSql(sqlConnectionString, tags: ["startup", "ready", "live"]);

        if (!string.IsNullOrEmpty(rabbitMqHost))
            // Message broker check: ready and live only (not required for startup)
            builder.AddRabbitMQ($"amqp://guest:guest@{rabbitMqHost}", null, "rabbitmq", tags: ["ready", "live"]);

        var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost";
        // Cache check: ready and live (not critical for startup)
        builder.AddRedis($"{redisHost}:6379", "redis", tags: ["ready", "live"]);

        return services;
    }

    /// <summary>
    /// Maps three health check endpoints following Kubernetes probe patterns:
    /// - /health: Overall health (all checks)
    /// - /health/startup: Startup probe (slow, comprehensive checks)
    /// - /health/ready: Readiness probe (dependencies required for normal operation)
    /// - /health/live: Liveness probe (minimal checks for crash detection)
    /// </summary>
    public static IApplicationBuilder UseStandardHealthChecks(this IApplicationBuilder app)
    {
        // Overall health endpoint - runs all checks
        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true
        });

        // Startup probe: Run once during application startup
        // Returns 200 only when the application has completed initialization
        // Includes all resource-intensive checks
        app.UseHealthChecks("/health/startup", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("startup"),
            ResultStatusCodes =
            {
                [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy] = 200,
                [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded] = 200, // Allow degraded for startup
                [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy] = 503
            }
        });

        // Readiness probe: Checked periodically (e.g., every 10 seconds)
        // Returns 200 only when the application is ready to accept traffic
        // Requires all critical dependencies (database, message broker)
        app.UseHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResultStatusCodes =
            {
                [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy] = 200,
                [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded] = 503,
                [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy] = 503
            }
        });

        // Liveness probe: Checked frequently (e.g., every 5 seconds)
        // Returns 200 if the application is still running
        // Minimal checks to detect if the pod needs to be restarted
        app.UseHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResultStatusCodes =
            {
                [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy] = 200,
                [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded] = 200, // Allow degraded for liveness
                [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy] = 503
            }
        });

        return app;
    }
}
