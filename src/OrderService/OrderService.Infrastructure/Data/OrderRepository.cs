using BuildingBlocks.Data;
using OrderService.Application.Common.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Infrastructure.Data;

public class OrderRepository(OrderServiceDbContext dbContext) : Repository<Order, int>(dbContext), IOrderRepository
{
}
