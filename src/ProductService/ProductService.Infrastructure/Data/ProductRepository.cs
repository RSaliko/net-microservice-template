using BuildingBlocks.Data;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.Common.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Domain.Enums;
using ProductService.Persistence.Contexts;

namespace ProductService.Infrastructure.Data;

public class ProductRepository(ProductServiceDbContext dbContext) : Repository<Product, Guid>(dbContext), IProductRepository
{
	public async Task<int> DeactivateAllActiveAsync(CancellationToken cancellationToken = default)
	{
		return await _dbContext.Set<Product>()
			.Where(p => p.Status == ProductStatus.Active)
			.ExecuteUpdateAsync(setters => setters.SetProperty(p => p.Status, ProductStatus.Inactive), cancellationToken);
	}
}
