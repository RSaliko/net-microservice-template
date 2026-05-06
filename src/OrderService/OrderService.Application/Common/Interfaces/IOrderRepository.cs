using BuildingBlocks.Data;
using OrderService.Domain.Entities;

namespace OrderService.Application.Common.Interfaces;

public interface IOrderRepository : IRepository<Order, int>
{
    
}
