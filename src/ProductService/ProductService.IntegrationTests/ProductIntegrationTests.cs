using BuildingBlocks.TestUtils;
using ProductService.Persistence.Contexts;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;

namespace ProductService.IntegrationTests;

/// <summary>
/// BP #12: Integration tests for ProductService.
/// Uses real PostgreSQL and RabbitMQ via Testcontainers.
/// </summary>
public class ProductIntegrationTests : BaseIntegrationTest<Program, ProductServiceDbContext>
{
    [Fact]
    public async Task CreateProduct_Should_PersistToDatabase()
    {
        // Arrange
        var product = new Product("SKU-INT-001", "Integration Product", "Desc", 99.99m, 10);

        // Act
        DbContext.Products.Add(product);
        await DbContext.SaveChangesAsync();

        // Assert
        var persisted = await DbContext.Products
            .FirstOrDefaultAsync(p => p.Sku == "SKU-INT-001");

        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("Integration Product");
        persisted.UnitPrice.Should().Be(99.99m);
    }
}
