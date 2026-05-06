using BuildingBlocks.TestUtils;
using OrderService.Persistence.Contexts;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.IntegrationTests;

/// <summary>
/// BP #12: Integration tests for OrderService.
/// Uses real PostgreSQL and RabbitMQ via Testcontainers.
/// </summary>
public class OrderIntegrationTests : BaseIntegrationTest<Program, OrderServiceDbContext>
{
    [Fact]
    public async Task CreateOrder_Should_PersistToDatabase_With_Items()
    {
        // Arrange
        var order = new Order("ORD-INT-001", "Integration Receiver", "090", "int@test.com", "Note");
        order.AddItem(Guid.NewGuid(), 2, 50m);

        // Act
        DbContext.Orders.Add(order);
        await DbContext.SaveChangesAsync();

        // Assert
        var persisted = await DbContext.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderCode == "ORD-INT-001");

        persisted.Should().NotBeNull();
        persisted!.ReceiverName.Should().Be("Integration Receiver");
        persisted.Items.Should().HaveCount(1);
        persisted.TotalAmount.Should().Be(100m);
    }
}
