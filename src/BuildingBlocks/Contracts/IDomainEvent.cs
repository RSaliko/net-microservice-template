using MediatR;

namespace BuildingBlocks.Contracts;

/// <summary>
/// Interface for all domain events.
/// Domain events happen within a single microservice (Intra-service).
/// </summary>
public interface IDomainEvent : INotification
{
    public DateTimeOffset OccurredOn { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
