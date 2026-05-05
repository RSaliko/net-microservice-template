using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;
using OrderService.Persistence.Contexts;

namespace OrderService.Persistence.Repositories;

public class OrderRepository(OrderServiceDbContext context) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task AddAsync(Order order, CancellationToken ct)
    {
        await context.Orders.AddAsync(order, ct);
    }
}
