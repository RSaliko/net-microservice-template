using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
