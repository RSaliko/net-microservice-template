using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace OrderService.Infrastructure.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Skeleton logic
        return Task.FromResult(HealthCheckResult.Healthy("Database is responding."));
    }
}
