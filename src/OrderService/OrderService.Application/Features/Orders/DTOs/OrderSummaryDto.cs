namespace OrderService.Application.Features.Orders.DTOs;

/// <summary>Summary DTO returned by OrderService — contains aggregated data from ProductService.</summary>
public record OrderSummaryDto(
    int TotalProducts,
    IReadOnlyList<ExternalProductDto> RecentProducts
);
