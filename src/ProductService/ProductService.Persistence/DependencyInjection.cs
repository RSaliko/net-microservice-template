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
        var host = Environment.GetEnvironmentVariable("MSSQL_HOST") ?? "localhost";
        var port = Environment.GetEnvironmentVariable("MSSQL_PORT") ?? "14333";
        var password = Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD") ?? "Your_strong_Password123";
        var dbName = Environment.GetEnvironmentVariable("PRODUCT_DB_NAME") ?? "ProductServiceDb";

        var connectionString = !string.IsNullOrEmpty(password) && password != "Your_strong_Password123"
            ? $"Server={host},{port};Database={dbName};User Id=sa;Password={password};TrustServerCertificate=True;Encrypt=False"
            : configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        // BP: Using standardized DbContext registration from BuildingBlocks
        services.AddAppDbContext<ProductServiceDbContext>(connectionString);

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ProductServiceDbContext>());
        services.AddScoped<BuildingBlocks.Data.IDbInitializer, ProductServiceDbInitializer>();

        return services;
    }
}
