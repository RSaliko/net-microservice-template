using BuildingBlocks.Contracts.Events;
using BuildingBlocks.Caching;
using MassTransit;
using MediatR;
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
    IApplicationDbContext context,
    IPublishEndpoint publishEndpoint,
    ProductMapper productMapper,
    ICacheService cacheService) 
    : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly ProductMapper _productMapper = productMapper;
    private readonly ICacheService _cacheService = cacheService;

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // BP: Domain logic encapsulated in the Entity constructor
        var product = new Product(
            request.Sku,
            request.Name, 
            request.Description,
            request.UnitPrice,
            request.QuantityStock);

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        // BP: Event-driven synchronization via Outbox (MassTransit)
        await _publishEndpoint.Publish(new ProductCreatedEvent(
            product.Id,
            product.Name,
            product.Sku,
            product.UnitPrice,
            product.QuantityStock), cancellationToken);

        var cacheKey = $"product:{product.Id}";
        var productDto = _productMapper.MapToDto(product);
        await _cacheService.SetAsync(cacheKey, productDto, cancellationToken: cancellationToken);

        return product.Id;
    }
}
