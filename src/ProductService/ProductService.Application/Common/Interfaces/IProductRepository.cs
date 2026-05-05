using BuildingBlocks.Data;
using ProductService.Domain.Entities;

namespace ProductService.Application.Common.Interfaces;

public interface IProductRepository : IRepository<Product, Guid>
{
	Task<int> DeactivateAllActiveAsync(CancellationToken cancellationToken = default);
}
