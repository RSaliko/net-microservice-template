using BuildingBlocks.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.Common.Interfaces;
using ProductService.Domain.Entities;
using System.Reflection;

namespace ProductService.Persistence.Contexts;

public class ProductServiceDbContext : BaseDbContext, IApplicationDbContext
{
    public ProductServiceDbContext(DbContextOptions<ProductServiceDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
