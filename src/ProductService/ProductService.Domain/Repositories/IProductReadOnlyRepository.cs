using BuildingBlocks.Data;
using ProductService.Domain.Entities;

namespace ProductService.Domain.Repositories;

public interface IProductReadOnlyRepository : IReadOnlyRepository<Product, Guid>
{
}
