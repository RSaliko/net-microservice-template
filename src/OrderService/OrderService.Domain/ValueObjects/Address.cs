namespace OrderService.Domain.ValueObjects;

/// <summary>
/// BP #27: Value Object - Immutable, no identity.
/// </summary>
public record Address(string Street, string City, string State, string ZipCode, string Country);
