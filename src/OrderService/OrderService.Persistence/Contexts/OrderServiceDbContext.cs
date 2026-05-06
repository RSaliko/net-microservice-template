using BuildingBlocks.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.Common.Interfaces;
using OrderService.Domain.Entities;
using System.Reflection;

namespace OrderService.Persistence.Contexts;

public class OrderServiceDbContext(
    DbContextOptions<OrderServiceDbContext> options, 
    BuildingBlocks.Security.IEncryptionService encryptionService) 
    : BaseDbContext(options), IApplicationDbContext, BuildingBlocks.Data.IUnitOfWork
{

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // BP #35: Transparent PII Encryption at rest
        var converter = new BuildingBlocks.Data.EncryptedValueConverter(encryptionService);
        modelBuilder.Entity<Order>().Property(x => x.Email).HasConversion(converter);
        modelBuilder.Entity<Order>().Property(x => x.Phone).HasConversion(converter);
        
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}
