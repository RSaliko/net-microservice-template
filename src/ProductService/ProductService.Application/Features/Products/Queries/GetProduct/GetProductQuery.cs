using BuildingBlocks.Caching;
using BuildingBlocks.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.Common.Interfaces;
using ProductService.Application.Features.Products.DTOs;
using ProductService.Application.Features.Products.Mappers;

namespace ProductService.Application.Features.Products.Queries.GetProduct;

public record GetProductQuery(Guid Id) : IRequest<ProductDto>;

public class GetProductQueryHandler(IProductRepository productRepository, ProductMapper mapper, ICacheService cacheService) 
    : IRequestHandler<GetProductQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        // BP: Check Redis cache first for improved performance
        var cacheKey = $"product:{request.Id}";
        var cachedProduct = await cacheService.GetAsync<ProductDto>(cacheKey, cancellationToken);

        if (cachedProduct is not null)
        {
            return cachedProduct;
        }
        
        // BP: Use AsNoTracking for read-only performance
        var product = await productRepository.FindByIdAsync(request.Id, cancellationToken);

        if (product == null)
        {
            throw new NotFoundException($"Product with ID {request.Id} not found.", "PRD_NOT_FOUND");
        }

        var productDto = mapper.MapToDto(product);
        
        await cacheService.SetAsync(cacheKey, productDto, cancellationToken: cancellationToken);

        return productDto;
    }
}
