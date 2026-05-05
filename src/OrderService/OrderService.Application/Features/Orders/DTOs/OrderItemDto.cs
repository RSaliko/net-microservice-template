namespace OrderService.Application.Features.Orders.DTOs;

public record OrderItemDto(Guid ProductId, int Quantity, decimal UnitPrice, decimal LineTotal);
