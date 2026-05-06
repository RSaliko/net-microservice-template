using MediatR;
using ProductService.Application.Common.Interfaces;
using BuildingBlocks.Caching;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Common;
using BuildingBlocks.Data;

namespace ProductService.Application.Features.Products.Commands.UpdateProduct;

public record UpdateProductCommand(Guid Id, string Name, string Description, decimal UnitPrice, int QuantityStock) : IRequest;

public class UpdateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork, ICacheService cacheService) 
    : IRequestHandler<UpdateProductCommand>
{
    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.FindByIdAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (product == null)
            throw new NotFoundException($"Product {request.Id} not found", ErrorCodes.Product.NotFound);

        product.Rename(request.Name);
        product.UpdateDescription(request.Description);
        product.SetUnitPrice(request.UnitPrice);
        product.SetQuantityStock(request.QuantityStock);

        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // BP #14: Cache Invalidation on Mutation
        await cacheService.RemoveAsync($"product:{product.Id}", cancellationToken).ConfigureAwait(false);
    }
}
