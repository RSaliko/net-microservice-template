namespace ProductService.Application.Features.Products.DTOs;

public record ProductDto
{
    public Guid Id { get; init; }
    public string Sku { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public int QuantityStock { get; init; }
}
