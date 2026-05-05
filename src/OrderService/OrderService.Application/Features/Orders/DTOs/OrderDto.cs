namespace OrderService.Application.Features.Orders.DTOs;

public record OrderDto(
	Guid Id,
	string OrderCode,
	string ReceiverName,
	string Phone,
	string Email,
	string Note,
	IReadOnlyCollection<OrderItemDto> Items,
	decimal TotalAmount);
