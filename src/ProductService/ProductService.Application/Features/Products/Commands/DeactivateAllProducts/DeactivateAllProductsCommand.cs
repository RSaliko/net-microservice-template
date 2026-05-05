using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.Common.Interfaces;
using ProductService.Domain.Enums;

namespace ProductService.Application.Features.Products.Commands.DeactivateAllProducts;

public record DeactivateAllProductsCommand : IRequest<int>;

public class DeactivateAllProductsCommandHandler(IApplicationDbContext context) 
    : IRequestHandler<DeactivateAllProductsCommand, int>
{
    public async Task<int> Handle(DeactivateAllProductsCommand request, CancellationToken cancellationToken)
    {
        // Rule: Use Bulk Operations for high performance (ExecuteUpdate)
        var affectedRows = await context.Products
            .Where(j => j.Status == ProductStatus.Active)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(j => j.Status, ProductStatus.Inactive), 
                cancellationToken);

        return affectedRows;
    }
}
