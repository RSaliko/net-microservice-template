namespace BuildingBlocks.Messaging.Events;

public record OrderCreatedIntegrationEvent(Guid OrderId, string CustomerName, decimal TotalAmount) : IntegrationEvent;
