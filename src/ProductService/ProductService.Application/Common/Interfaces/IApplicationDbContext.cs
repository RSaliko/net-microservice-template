using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;

namespace ProductService.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
