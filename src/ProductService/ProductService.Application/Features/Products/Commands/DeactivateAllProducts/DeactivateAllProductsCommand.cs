using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.Common.Interfaces;
using ProductService.Domain.Enums;

namespace ProductService.Application.Features.Products.Commands.DeactivateAllProducts;

public record DeactivateAllProductsCommand : IRequest<int>;

public class DeactivateAllProductsCommandHandler(IProductRepository productRepository) 
    : IRequestHandler<DeactivateAllProductsCommand, int>
{
    public async Task<int> Handle(DeactivateAllProductsCommand request, CancellationToken cancellationToken)
    {
        // Rule: Use Bulk Operations for high performance (ExecuteUpdate) via repository
        var affectedRows = await productRepository.DeactivateAllActiveAsync(cancellationToken);

        return affectedRows;
    }
}
