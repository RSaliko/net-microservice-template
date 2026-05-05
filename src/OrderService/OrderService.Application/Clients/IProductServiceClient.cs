using Refit;
using OrderService.Application.Features.Orders.DTOs;

namespace OrderService.Application.Clients;

/// <summary>
/// Refit client for ProductService internal HTTP communication.
/// OrderService calls ProductService synchronously via this typed client (registered in Program.cs).
/// </summary>
public interface IProductServiceClient
{
    [Get("/api/v1/products")]
    Task<ProductServiceResponse> GetProductsAsync();
}

/// <summary>Matches ProductService's ApiResponse[List[ExternalProductDto]] envelope.</summary>
public record ProductServiceResponse(
    List<ExternalProductDto>? Data,
    int StatusCode,
    string Message,
    bool Success
);
