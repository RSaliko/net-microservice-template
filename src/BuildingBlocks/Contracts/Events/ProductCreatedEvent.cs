namespace BuildingBlocks.Contracts.Events;

public record ProductCreatedEvent(
    Guid ProductId, 
    string Name, 
    string Sku, 
    decimal UnitPrice,
    int QuantityStock
);
