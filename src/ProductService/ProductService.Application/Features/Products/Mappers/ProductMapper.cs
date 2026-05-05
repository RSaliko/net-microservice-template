using Riok.Mapperly.Abstractions;
using ProductService.Application.Features.Products.DTOs;
using ProductService.Domain.Entities;

namespace ProductService.Application.Features.Products.Mappers;

[Mapper]
public partial class ProductMapper
{
    public partial ProductDto MapToDto(Product product);
    
    /// <summary>
    /// Support for IQueryable projection to optimize database queries.
    /// </summary>
    public partial IQueryable<ProductDto> ProjectToDto(IQueryable<Product> query);
}
