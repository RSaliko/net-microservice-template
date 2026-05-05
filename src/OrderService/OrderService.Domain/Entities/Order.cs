using BuildingBlocks.Models;
using OrderService.Domain.Enums;
using OrderService.Domain.ValueObjects;
using System.Text.RegularExpressions;

namespace OrderService.Domain.Entities;

public class Order : BaseEntity
{
    private static readonly Regex OrderCodeRegex = new("^[A-Z0-9-]{4,32}$", RegexOptions.Compiled);
    private const int MaxOrderCodeLength = 32;
    private const int MaxDescriptionLength = 1000;

    public string OrderCode { get; private set; } = string.Empty;
    public string ReceiverName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public Address? Address { get; private set; }
    public string Note { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; } = OrderStatus.Draft;

    public void SetAddress(Address address)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
    }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public decimal TotalAmount => _items.Sum(x => x.LineTotal);

    private readonly List<OrderItem> _items = [];

    private Order() { }

    public Order(string orderCode, string receiverName, string phone, string email, string note)
    {
        OrderCode = ValidateOrderCode(orderCode);
        ReceiverName = ValidateReceiverName(receiverName);
        Phone = ValidatePhone(phone);
        Email = ValidateEmail(email);
        Note = ValidateNote(note);
        Status = OrderStatus.Draft;
    }

    public void UpdateDetails(string receiverName, string phone, string email, string note)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Only draft orders can be updated");

        ReceiverName = ValidateReceiverName(receiverName);
        Phone = ValidatePhone(phone);
        Email = ValidateEmail(email);
        Note = ValidateNote(note);
    }

    public void AddItem(Guid productId, int quantity, decimal unitPrice)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Only draft orders can be modified");

        var existingItem = _items.FirstOrDefault(x => x.ProductId == productId);
        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
            existingItem.ChangeUnitPrice(unitPrice);
            return;
        }

        _items.Add(new OrderItem(productId, quantity, unitPrice));
    }

    public void RemoveItem(Guid productId)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Only draft orders can be modified");

        var existingItem = _items.FirstOrDefault(x => x.ProductId == productId);
        if (existingItem is null)
            return;

        _items.Remove(existingItem);
    }

    public void Activate()
    {
        if (Status == OrderStatus.Active)
            throw new InvalidOperationException("Order is already active");

        Status = OrderStatus.Active;
    }

    public void Deactivate()
    {
        if (Status != OrderStatus.Active)
            throw new InvalidOperationException("Only active orders can be deactivated");

        Status = OrderStatus.Inactive;
    }

    public void MarkAsDraft()
    {
        if (Status == OrderStatus.Active)
            throw new InvalidOperationException("Cannot move active order back to draft");

        Status = OrderStatus.Draft;
    }

    private static string ValidateOrderCode(string orderCode)
    {
        if (string.IsNullOrWhiteSpace(orderCode))
            throw new ArgumentException("Order code cannot be empty", nameof(orderCode));

        var normalizedOrderCode = orderCode.Trim().ToUpperInvariant();
        if (normalizedOrderCode.Length > MaxOrderCodeLength)
            throw new ArgumentException($"Order code cannot exceed {MaxOrderCodeLength} characters", nameof(orderCode));

        if (!OrderCodeRegex.IsMatch(normalizedOrderCode))
            throw new ArgumentException("Order code must contain only A-Z, 0-9 or '-' and be between 4 and 32 characters", nameof(orderCode));

        return normalizedOrderCode;
    }

    private static string ValidateReceiverName(string receiverName)
    {
        if (string.IsNullOrWhiteSpace(receiverName))
            throw new ArgumentException("Receiver name cannot be empty", nameof(receiverName));

        return receiverName.Trim();
    }

    private static string ValidatePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone cannot be empty", nameof(phone));

        return phone.Trim();
    }

    private static string ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        return email.Trim();
    }

    private static string ValidateNote(string note)
    {
        return note?.Trim() ?? string.Empty;
    }
}
