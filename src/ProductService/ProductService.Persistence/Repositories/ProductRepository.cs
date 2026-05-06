using BuildingBlocks.Data;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.Common.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Domain.Enums;
using ProductService.Persistence.Contexts;

namespace ProductService.Persistence.Repositories;

public class ProductRepository(ProductServiceDbContext context) 
    : Repository<Product, Guid>(context), IProductRepository
{
    public async Task<int> DeactivateAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await context.Products
            .Where(x => x.Status == ProductStatus.Active)
            .ExecuteUpdateAsync(s => s.SetProperty(b => b.Status, ProductStatus.Inactive), cancellationToken);
    }
}
