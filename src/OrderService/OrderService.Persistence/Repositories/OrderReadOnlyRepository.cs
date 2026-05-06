using BuildingBlocks.Data;
using OrderService.Domain.Entities;
using OrderService.Domain.Repositories;
using OrderService.Persistence.Contexts;

namespace OrderService.Persistence.Repositories;

public class OrderReadOnlyRepository([Microsoft.Extensions.DependencyInjection.FromKeyedServices("readonly")] Microsoft.EntityFrameworkCore.DbContext context) 
    : ReadOnlyRepository<Order, Guid>(context), IOrderReadOnlyRepository
{
}
