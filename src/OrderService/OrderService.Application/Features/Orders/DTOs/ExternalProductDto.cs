namespace OrderService.Application.Features.Orders.DTOs;

public record ExternalProductDto(
    Guid Id, 
    string Name, 
    string Status, 
    string Sku, 
    string? Description, 
    decimal UnitPrice,
    int QuantityStock
);
