using FluentAssertions;
using ProductService.Domain.Entities;
using ProductService.Domain.Enums;
using Xunit;

namespace ProductService.UnitTests.Domain;

/// <summary>
/// BP #12: Unit tests for Product entity logic.
/// Uses FluentAssertions for better readability.
/// </summary>
public class ProductTests
{
    [Fact]
    public void Product_Should_BeCreatedWithActiveStatus_When_Initialized()
    {
        // Arrange & Act
        var product = new Product("SKU-TEST-001", "Test Product", "Test Description", 100m, 5);

        // Assert (AAA Pattern)
        product.Status.Should().Be(ProductStatus.Active);
        product.Sku.Should().Be("SKU-TEST-001");
        product.Name.Should().Be("Test Product");
        product.UnitPrice.Should().Be(100m);
        product.QuantityStock.Should().Be(5);
        product.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Product_Should_ChangeStatusToInactive_When_Deactivated()
    {
        // Arrange
        var product = new Product("SKU-TEST-002", "Test Product", "Test Description", 100m, 5);

        // Act
        product.Deactivate();

        // Assert
        product.Status.Should().Be(ProductStatus.Inactive);
    }

    [Fact]
    public void Product_Should_ChangeStatusToDiscontinued_When_Discontinued()
    {
        // Arrange
        var product = new Product("SKU-TEST-003", "Test Product", "Test Description", 100m, 5);

        // Act
        product.Discontinue();

        // Assert
        product.Status.Should().Be(ProductStatus.Discontinued);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("SKU")] // Too short
    [InlineData("SKU-WITH-SPECIAL-CHARS-@#$")] // Invalid chars
    public void Product_Should_ThrowArgumentException_When_SkuIsInvalid(string invalidSku)
    {
        // Act & Assert
        Action act = () => new Product(invalidSku, "Name", "Desc", 10m, 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Product_Should_DeductStock_When_ValidQuantityProvided()
    {
        // Arrange
        var product = new Product("SKU-001", "Product", "Desc", 10m, 10);

        // Act
        product.DeductStock(3);

        // Assert
        product.QuantityStock.Should().Be(7);
    }

    [Fact]
    public void Product_Should_ThrowException_When_DeductingMoreThanStock()
    {
        // Arrange
        var product = new Product("SKU-001", "Product", "Desc", 10m, 5);

        // Act & Assert
        Action act = () => product.DeductStock(10);
        act.Should().Throw<Exception>().WithMessage("*Insufficient stock*");
    }
}
