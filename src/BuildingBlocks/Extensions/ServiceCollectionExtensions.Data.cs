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
        return services.AddReadWriteDbContext<TContext>(connectionString);
    }

    /// <summary>
    /// BP #34: Read-Write Splitting registration.
    /// </summary>
    public static IServiceCollection AddReadWriteDbContext<TContext>(
        this IServiceCollection services, 
        string writeConnectionString,
        string? readConnectionString = null) where TContext : DbContext
    {
        services.AddScoped<DispatchDomainEventsInterceptor>();
        services.AddScoped<AuditInterceptor>();

        // Primary Context (Write)
        services.AddDbContext<TContext>((sp, options) =>
        {
            options.UseNpgsql(writeConnectionString, npgsql => npgsql.EnableRetryOnFailure())
                   .AddInterceptors(sp.GetRequiredService<AuditInterceptor>())
                   .AddInterceptors(sp.GetRequiredService<DispatchDomainEventsInterceptor>());
        });

        // Replica Context (Read-Only)
        var replicaConnectionString = readConnectionString ?? writeConnectionString;
        services.AddKeyedScoped<DbContext>("readonly", (sp, key) =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<TContext>();
            optionsBuilder.UseNpgsql(replicaConnectionString, npgsql => npgsql.EnableRetryOnFailure())
                          .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            
            return (TContext)Activator.CreateInstance(typeof(TContext), optionsBuilder.Options)!;
        });

        return services;
    }
}
