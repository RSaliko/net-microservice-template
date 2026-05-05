using ProductService.Domain.Entities;
using ProductService.Domain.Enums;
using Xunit;

namespace ProductService.UnitTests.Domain;

public class ProductTests
{
    [Fact]
    public void Product_Should_BeCreatedWithActiveStatus_When_Initialized()
    {
        // Arrange & Act
        var product = new Product("SKU-TEST-001", "Test Product", "Test Description", 100m, 5);

        // Assert
        Assert.Equal(ProductStatus.Active, product.Status);
        Assert.Equal("SKU-TEST-001", product.Sku);
        Assert.Equal("Test Product", product.Name);
    }

    [Fact]
    public void Product_Should_ChangeStatusToInactive_When_Deactivated()
    {
        // Arrange
        var product = new Product("SKU-TEST-002", "Test Product", "Test Description", 100m, 5);

        // Act
        product.Deactivate();

        // Assert
        Assert.Equal(ProductStatus.Inactive, product.Status);
    }

    [Fact]
    public void Product_Should_ChangeStatusToDiscontinued_When_Discontinued()
    {
        // Arrange
        var product = new Product("SKU-TEST-003", "Test Product", "Test Description", 100m, 5);

        // Act
        product.Discontinue();

        // Assert
        Assert.Equal(ProductStatus.Discontinued, product.Status);
    }
}
