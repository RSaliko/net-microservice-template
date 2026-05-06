using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Caching;
using MassTransit;
using MediatR;
using BuildingBlocks.Data;
using ProductService.Application.Common.Interfaces;
using ProductService.Application.Features.Products.DTOs;
using ProductService.Application.Features.Products.Mappers;
using ProductService.Domain.Entities;

namespace ProductService.Application.Features.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Sku,
    string Name,
    string? Description,
    decimal UnitPrice,
    int QuantityStock
) : IRequest<Guid>;

public class CreateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint publishEndpoint,
    ProductMapper productMapper,
    ICacheService cacheService) 
    : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // BP #23: Advanced Distributed Tracing - Manual Span with Business Tags
        using var activity = BuildingBlocks.Observability.TracingConstants.ActivitySource.StartActivity("CreateProduct");
        activity?.SetTag("product.sku", request.Sku);
        activity?.SetTag("product.name", request.Name);
        activity?.SetTag("product.price", request.UnitPrice);
        // BP: Domain logic encapsulated in the Entity constructor
        var product = new Product(
            request.Sku,
            request.Name, 
            request.Description,
            request.UnitPrice,
            request.QuantityStock);

        await productRepository.AddAsync(product, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // BP: Event-driven synchronization via Outbox (MassTransit)
        await publishEndpoint.Publish(new ProductCreatedEvent(
            product.Id,
            product.Name,
            product.Sku,
            product.UnitPrice,
            product.QuantityStock), cancellationToken);

        var cacheKey = $"product:{product.Id}";
        var productDto = productMapper.MapToDto(product);
        await cacheService.SetAsync(cacheKey, productDto, cancellationToken: cancellationToken);

        // BP #14: Event-driven invalidation.
        // TODO: In a production scenario, any Update/Delete Command should call cacheService.RemoveAsync(cacheKey).

        return product.Id;
    }
}
