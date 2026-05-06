using BuildingBlocks.Data;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using ProductService.Persistence.Contexts;

namespace ProductService.Persistence.Repositories;

public class ProductReadOnlyRepository([Microsoft.Extensions.DependencyInjection.FromKeyedServices("readonly")] Microsoft.EntityFrameworkCore.DbContext context) 
    : ReadOnlyRepository<Product, Guid>(context), IProductReadOnlyRepository
{
}
