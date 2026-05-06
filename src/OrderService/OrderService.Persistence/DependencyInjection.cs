using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BuildingBlocks.Extensions;
using OrderService.Application.Common.Interfaces;
using OrderService.Persistence.Contexts;
using OrderService.Persistence.Data;
using BuildingBlocks.Data;

namespace OrderService.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        // ... (previous logic)
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "Your_strong_Password123";
        var dbName = Environment.GetEnvironmentVariable("ORDER_DB_NAME") ?? "OrderServiceDb";

        var connectionString = !string.IsNullOrEmpty(password) && password != "Your_strong_Password123"
            ? $"Host={host};Port={port};Database={dbName};Username=postgres;Password={password}"
            : configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        // BP: Standardized registration with Interceptors (Audit, Domain Events)
        services.AddAppDbContext<OrderServiceDbContext>(connectionString);

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<OrderServiceDbContext>());
        services.AddScoped<BuildingBlocks.Data.IUnitOfWork>(sp => sp.GetRequiredService<OrderServiceDbContext>());
        services.AddScoped<BuildingBlocks.Data.IDbInitializer, OrderServiceDbInitializer>();

        return services;
    }
}
