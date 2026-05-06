using BuildingBlocks.Contracts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BuildingBlocks.Data;

/// <summary>
/// Base DbContext providing standardized configuration for SoftDelete and Optimistic Concurrency.
/// Interceptors (Audit, DomainEvents) are registered via DI.
/// </summary>
public abstract class BaseDbContext : DbContext
{
    protected BaseDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Standardize Soft Delete query filter
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var body = Expression.Equal(
                    Expression.Property(parameter, nameof(ISoftDelete.IsDeleted)),
                    Expression.Constant(false));
                var filter = Expression.Lambda(body, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
                
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(ISoftDelete.IsDeleted));
            }

            // Standardize RowVersion for Optimistic Concurrency
            var rowVersionProp = entityType.FindProperty("RowVersion");
            if (rowVersionProp != null && (rowVersionProp.ClrType == typeof(byte[]) || rowVersionProp.ClrType == typeof(uint)))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property("RowVersion")
                    .IsRowVersion();
            }
        }
    }
}
