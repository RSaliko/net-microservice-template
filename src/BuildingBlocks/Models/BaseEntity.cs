using BuildingBlocks.Contracts;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuildingBlocks.Models;

public abstract class BaseEntity<TId> : ISoftDelete, IAuditEntity
{
    public TId Id { get; set; } = default!;
    
    // IAuditEntity
    public DateTimeOffset CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // ISoftDelete
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // Optimistic Concurrency
    public byte[] RowVersion { get; set; } = [];

    // Domain Events
    private readonly List<IDomainEvent> _domainEvents = [];
    
    [NotMapped]
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void RemoveDomainEvent(IDomainEvent domainEvent) => _domainEvents.Remove(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}

public abstract class BaseEntity : BaseEntity<Guid>
{
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
    }
}
