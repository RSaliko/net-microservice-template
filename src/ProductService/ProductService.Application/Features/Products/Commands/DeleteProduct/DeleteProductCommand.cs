using MediatR;
using ProductService.Application.Common.Interfaces;
using BuildingBlocks.Caching;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Common;
using BuildingBlocks.Data;

namespace ProductService.Application.Features.Products.Commands.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest;

public class DeleteProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork, ICacheService cacheService) 
    : IRequestHandler<DeleteProductCommand>
{
    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.FindByIdAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);

        if (product == null)
            throw new NotFoundException($"Product {request.Id} not found", ErrorCodes.Product.NotFound);

        await productRepository.DeleteAsync(product, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        // BP #14: Cache Invalidation on Mutation
        await cacheService.RemoveAsync($"product:{request.Id}", cancellationToken).ConfigureAwait(false);
    }
}
