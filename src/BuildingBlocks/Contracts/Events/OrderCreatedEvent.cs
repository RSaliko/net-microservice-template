namespace BuildingBlocks.Contracts.Events;

public record OrderCreatedEvent(
    Guid OrderId, 
    string OrderCode, 
    string CustomerName, 
    DateTimeOffset CreatedAt
);
