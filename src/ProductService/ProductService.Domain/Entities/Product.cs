using BuildingBlocks.Models;
using ProductService.Domain.Enums;
using ProductService.Domain.ValueObjects;
using System.Text.RegularExpressions;

namespace ProductService.Domain.Entities;

/// <summary>
/// Represents a Product in the system.
/// Follows DDD principles with private setters and domain logic.
/// </summary>
public class Product : BaseEntity
{
    private static readonly Regex SkuRegex = new("^[A-Z0-9-]{4,32}$", RegexOptions.Compiled);
    private const int MaxSkuLength = 32;
    private const int MaxTitleLength = 200;
    private const int MaxDescriptionLength = 1000;

    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ProductStatus Status { get; private set; } = ProductStatus.Active;
    public decimal UnitPrice { get; private set; }
    public int QuantityStock { get; private set; }
    public byte[] RowVersion { get; private set; } = []; // Optimistic Concurrency

    // EF Core requirement
    private Product() { }

    public Product(string sku, string name, string? description, decimal unitPrice, int quantityStock)
    {
        Sku = ValidateSku(sku);
        Name = ValidateName(name);
        Description = ValidateDescription(description);
        Status = ProductStatus.Active;
        UnitPrice = ValidateUnitPrice(unitPrice);
        QuantityStock = ValidateQuantityStock(quantityStock);
    }

    public void ChangeSku(string sku)
    {
        if (Status == ProductStatus.Discontinued)
            throw new InvalidOperationException("Cannot change SKU of a discontinued product");

        Sku = ValidateSku(sku);
    }

    public void Rename(string name)
    {
        if (Status == ProductStatus.Discontinued)
            throw new InvalidOperationException("Cannot rename a discontinued product");

        Name = ValidateName(name);
    }

    public void UpdateDescription(string? description)
    {
        Description = ValidateDescription(description);
    }

    public void SetUnitPrice(decimal unitPrice)
    {
        UnitPrice = ValidateUnitPrice(unitPrice);
    }

    public void SetQuantityStock(int quantityStock)
    {
        QuantityStock = ValidateQuantityStock(quantityStock);
        
        if (QuantityStock == 0 && Status == ProductStatus.Active)
        {
            Status = ProductStatus.OutOfStock;
        }
    }

    public void DeductStock(int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity to deduct must be greater than 0");
        if (QuantityStock < quantity) throw new InvalidOperationException($"Insufficient stock for product {Name}. Requested: {quantity}, Available: {QuantityStock}");
        
        QuantityStock -= quantity;
        
        if (QuantityStock == 0)
        {
            Status = ProductStatus.OutOfStock;
        }
    }

    public void Activate()
    {
        if (Status == ProductStatus.Discontinued)
            throw new InvalidOperationException("Cannot activate a discontinued product");

        Status = QuantityStock > 0 ? ProductStatus.Active : ProductStatus.OutOfStock;
    }

    public void Deactivate()
    {
        Status = ProductStatus.Inactive;
    }


    public void Discontinue()
    {
        Status = ProductStatus.Discontinued;
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        var normalizedName = name.Trim();
        if (normalizedName.Length > MaxTitleLength)
            throw new ArgumentException($"Name cannot exceed {MaxTitleLength} characters", nameof(name));

        return normalizedName;
    }

    private static string ValidateSku(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU cannot be empty", nameof(sku));

        var normalizedSku = sku.Trim().ToUpperInvariant();
        if (normalizedSku.Length > MaxSkuLength)
            throw new ArgumentException($"SKU cannot exceed {MaxSkuLength} characters", nameof(sku));

        if (!SkuRegex.IsMatch(normalizedSku))
            throw new ArgumentException("SKU must contain only A-Z, 0-9 or '-' and be between 4 and 32 characters", nameof(sku));

        return normalizedSku;
    }

    private static string? ValidateDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return null;

        var normalizedDescription = description.Trim();
        if (normalizedDescription.Length > MaxDescriptionLength)
            throw new ArgumentException($"Description cannot exceed {MaxDescriptionLength} characters", nameof(description));

        return normalizedDescription;
    }

    private static decimal ValidateUnitPrice(decimal unitPrice)
    {
        if (unitPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Price cannot be negative");

        return decimal.Round(unitPrice, 2, MidpointRounding.AwayFromZero);
    }

    private static int ValidateQuantityStock(int quantityStock)
    {
        if (quantityStock < 0)
            throw new ArgumentOutOfRangeException(nameof(quantityStock), "Quantity stock cannot be negative");

        return quantityStock;
    }
}
