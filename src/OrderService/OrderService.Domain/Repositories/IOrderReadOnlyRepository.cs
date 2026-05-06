using BuildingBlocks.Data;
using OrderService.Domain.Entities;

namespace OrderService.Domain.Repositories;

public interface IOrderReadOnlyRepository : IReadOnlyRepository<Order, Guid>
{
}
