namespace BuildingBlocks.Contracts.Events;

public record OrderItemEvent(Guid ProductId, int Quantity);

public record OrderCreatedEvent(
    Guid OrderId, 
    string OrderCode, 
    string CustomerName, 
    DateTimeOffset CreatedAt,
    IReadOnlyCollection<OrderItemEvent> Items
);
