using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ProductService.Infrastructure.Health;

public class ProductServiceHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Add custom health check logic here
        return Task.FromResult(HealthCheckResult.Healthy("ProductService is operational"));
    }
}
