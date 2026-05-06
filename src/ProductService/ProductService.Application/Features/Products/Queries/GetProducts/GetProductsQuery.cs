using MediatR;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Models;
using BuildingBlocks.Extensions;
using ProductService.Application.Common.Interfaces;
using ProductService.Application.Features.Products.DTOs;
using ProductService.Application.Features.Products.Mappers;

namespace ProductService.Application.Features.Products.Queries.GetProducts;

public record GetProductsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SortBy = null,
    string? Filter = null
) : IRequest<PaginatedResult<ProductDto>>;

public class GetProductsQueryHandler(IProductRepository productRepository, ProductMapper mapper) 
    : IRequestHandler<GetProductsQuery, PaginatedResult<ProductDto>>
{
    public async Task<PaginatedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = productRepository.Query().AsNoTracking();

        // Filtering
        if (!string.IsNullOrEmpty(request.Filter))
        {
            query = query.Where(p => p.Name.Contains(request.Filter) || p.Sku.Contains(request.Filter));
        }

        // Sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => query.OrderBy(p => p.Name),
            "sku" => query.OrderBy(p => p.Sku),
            "price" => query.OrderBy(p => p.UnitPrice),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        // BP: Standardized pagination with IQueryable projection
        return await mapper.ProjectToDto(query)
            .ToPaginatedResultAsync(request.PageNumber, request.PageSize, cancellationToken);
    }
}
