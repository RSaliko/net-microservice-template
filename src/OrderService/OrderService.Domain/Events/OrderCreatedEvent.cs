using OrderService.Domain.Entities;

namespace OrderService.Domain.Events;

public record OrderCreatedEvent(Order Order);
