using MediatR;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Models;
using BuildingBlocks.Extensions;
using ProductService.Application.Common.Interfaces;
using ProductService.Application.Features.Products.DTOs;
using ProductService.Application.Features.Products.Mappers;

namespace ProductService.Application.Features.Products.Queries.GetProducts;

public record GetProductsQuery(
    PagingParams Paging,
    SortingParams Sorting,
    FilteringParams Filtering
) : IRequest<PaginatedResult<ProductDto>>;

public class GetProductsQueryHandler(IProductRepository productRepository, ProductMapper mapper) 
    : IRequestHandler<GetProductsQuery, PaginatedResult<ProductDto>>
{
    public async Task<PaginatedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        // BP #23: Advanced Distributed Tracing - Manual Span with Business Tags
        using var activity = BuildingBlocks.Observability.TracingConstants.ActivitySource.StartActivity("GetProducts");
        activity?.SetTag("query.page", request.Paging.PageNumber);
        activity?.SetTag("query.size", request.Paging.PageSize);
        activity?.SetTag("query.filter", request.Filtering.SearchTerm);
        var query = productRepository.Query().AsNoTracking();

        // Filtering
        if (!string.IsNullOrEmpty(request.Filtering.SearchTerm))
        {
            query = query.Where(p => p.Name.Contains(request.Filtering.SearchTerm) || p.Sku.Contains(request.Filtering.SearchTerm));
        }

        // Sorting
        query = request.Sorting.SortBy?.ToLower() switch
        {
            "name" => query.OrderBy(p => p.Name),
            "sku" => query.OrderBy(p => p.Sku),
            "price" => query.OrderBy(p => p.UnitPrice),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        // BP: Standardized pagination with IQueryable projection
        return await mapper.ProjectToDto(query)
            .ToPaginatedResultAsync(request.Paging.PageNumber, request.Paging.PageSize, cancellationToken);
    }
}
