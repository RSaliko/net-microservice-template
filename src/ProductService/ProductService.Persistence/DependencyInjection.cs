using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BuildingBlocks.Extensions;
using ProductService.Application.Common.Interfaces;
using ProductService.Persistence.Contexts;
using ProductService.Persistence.Data;

namespace ProductService.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "Your_strong_Password123";
        var dbName = Environment.GetEnvironmentVariable("PRODUCT_DB_NAME") ?? "ProductServiceDb";

        var connectionString = !string.IsNullOrEmpty(password) && password != "Your_strong_Password123"
            ? $"Host={host};Port={port};Database={dbName};Username=postgres;Password={password}"
            : configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        var writeConnection = connectionString;
        var readConnection = configuration.GetConnectionString("ReplicaConnection") ?? writeConnection;

        // BP: Using standardized DbContext registration from BuildingBlocks with Read-Write Splitting
        services.AddReadWriteDbContext<ProductServiceDbContext>(writeConnection, readConnection);

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ProductServiceDbContext>());
        services.AddScoped<BuildingBlocks.Data.IUnitOfWork>(sp => sp.GetRequiredService<ProductServiceDbContext>());
        services.AddScoped<IProductRepository, Repositories.ProductRepository>();
        services.AddScoped<ProductService.Domain.Repositories.IProductReadOnlyRepository, Repositories.ProductReadOnlyRepository>();
        services.AddScoped<BuildingBlocks.Data.IDbInitializer, ProductServiceDbInitializer>();

        return services;
    }
}
