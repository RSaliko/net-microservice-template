using BuildingBlocks.Data.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Extensions;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// Standardized DbContext registration for microservices.
    /// Includes SQL Server configuration, retry policy, and standard interceptors (Audit, DispatchDomainEvents).
    /// </summary>
    public static IServiceCollection AddAppDbContext<TContext>(
        this IServiceCollection services, 
        string connectionString) where TContext : DbContext
    {
        services.AddScoped<DispatchDomainEventsInterceptor>();
        services.AddScoped<AuditInterceptor>();

        services.AddDbContext<TContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure())
                   .AddInterceptors(sp.GetRequiredService<AuditInterceptor>())
                   .AddInterceptors(sp.GetRequiredService<DispatchDomainEventsInterceptor>());
        });

        return services;
    }
}
