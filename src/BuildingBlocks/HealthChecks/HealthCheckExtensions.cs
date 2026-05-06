using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.HealthChecks;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddStandardHealthChecks(this IServiceCollection services, string? sqlConnectionString = null, string? rabbitMqHost = null)
    {
        var builder = services.AddHealthChecks();

        if (!string.IsNullOrEmpty(sqlConnectionString))
            builder.AddNpgSql(sqlConnectionString);

        if (!string.IsNullOrEmpty(rabbitMqHost))
            builder.AddRabbitMQ($"amqp://guest:guest@{rabbitMqHost}", null, "rabbitmq");

        var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost";
        builder.AddRedis($"{redisHost}:6379", "redis");

        return services;
    }

    public static IApplicationBuilder UseStandardHealthChecks(this IApplicationBuilder app)
    {
        app.UseHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true
        });

        app.UseHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        return app;
    }
}
