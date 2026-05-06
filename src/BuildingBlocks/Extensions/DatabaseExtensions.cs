using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using BuildingBlocks.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Extensions;

public static class DatabaseExtensions
{
    /// <summary>
    /// Runs database migrations and seeds data on startup.
    /// Non-fatal: logs the error and continues so the service can still start
    /// when a database is temporarily unavailable (e.g., Docker startup race).
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var initializer = scope.ServiceProvider.GetService<IDbInitializer>();

        if (initializer == null) return;

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IDbInitializer>>();
        try
        {
            logger.LogInformation("Initializing database...");
            await initializer.InitializeAsync();
            logger.LogInformation("Database initialization completed.");
        }
        catch (Exception ex)
        {
            // Non-fatal: service still starts. DB may be temporarily unavailable.
            // Useful in Docker Compose where DB startup is not instant.
            logger.LogWarning(ex, "Database initialization failed. Service will start without seeded data. Retry manually or restart the service.");
        }
    }
    /// <summary>
    /// BP #24: Bulk Soft Delete using ExecuteUpdate.
    /// Efficiency: Runs a single UPDATE statement on the database.
    /// </summary>
    public static async Task<int> SoftDeleteAsync<T>(this IQueryable<T> query, CancellationToken cancellationToken = default)
        where T : class, Contracts.ISoftDelete
    {
        return await query.ExecuteUpdateAsync(s => s
            .SetProperty(p => p.IsDeleted, true)
            .SetProperty(p => p.DeletedAt, DateTimeOffset.UtcNow),
            cancellationToken);
    }
}

