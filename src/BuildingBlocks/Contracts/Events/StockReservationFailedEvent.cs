namespace BuildingBlocks.Contracts.Events;

public record StockReservationFailedEvent(Guid OrderId, string Reason);
