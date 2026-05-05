namespace OrderService.Domain.Entities;

public class OrderItem
{
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = null!;

    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    public decimal LineTotal => decimal.Round(Quantity * UnitPrice, 2, MidpointRounding.AwayFromZero);

    private OrderItem() { }

    public OrderItem(Guid productId, int quantity, decimal unitPrice)
    {
        ProductId = ValidateProductId(productId);
        Quantity = ValidateQuantity(quantity);
        UnitPrice = ValidateUnitPrice(unitPrice);
    }

    public void IncreaseQuantity(int quantity)
    {
        var delta = ValidateQuantity(quantity);
        Quantity += delta;
    }

    public void ChangeQuantity(int quantity)
    {
        Quantity = ValidateQuantity(quantity);
    }

    public void ChangeUnitPrice(decimal unitPrice)
    {
        UnitPrice = ValidateUnitPrice(unitPrice);
    }

    private static Guid ValidateProductId(Guid productId)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty", nameof(productId));

        return productId;
    }

    private static int ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero");

        return quantity;
    }

    private static decimal ValidateUnitPrice(decimal unitPrice)
    {
        if (unitPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative");

        return decimal.Round(unitPrice, 2, MidpointRounding.AwayFromZero);
    }
}
