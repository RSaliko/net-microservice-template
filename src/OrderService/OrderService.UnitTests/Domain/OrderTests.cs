using FluentAssertions;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.ValueObjects;
using Xunit;

namespace OrderService.UnitTests.Domain;

/// <summary>
/// BP #12: Unit tests for Order entity logic.
/// </summary>
public class OrderTests
{
    [Fact]
    public void Order_Create_Should_InitializeCorrectly_When_ValidParametersProvided()
    {
        // Arrange
        var orderCode = "ORD-123";
        var receiverName = "John Doe";
        var phone = "0901234567";
        var email = "john@example.com";
        var note = "Test Note";
        var address = new Address("Street", "City", "State", "Country", "Zip");

        // Act
        var order = new Order(orderCode, receiverName, phone, email, note);
        order.SetAddress(address);

        // Assert
        order.OrderCode.Should().Be(orderCode.ToUpperInvariant());
        order.Status.Should().Be(OrderStatus.Draft);
        order.ReceiverName.Should().Be(receiverName);
        order.Address.Should().Be(address);
    }

    [Fact]
    public void Order_AddItem_Should_AddSuccessfully_When_ValidItemProvided()
    {
        // Arrange
        var order = new Order("ORD-001", "Receiver", "090123", "email@test.com", "");
        var productId = Guid.NewGuid();

        // Act
        order.AddItem(productId, 2, 50m);

        // Assert
        order.Items.Should().HaveCount(1);
        order.Items.First().ProductId.Should().Be(productId);
        order.Items.First().Quantity.Should().Be(2);
        order.TotalAmount.Should().Be(100m);
    }

    [Fact]
    public void Order_ReserveStock_Should_ChangeStatusToStockReserved_When_Called()
    {
        // Arrange
        var order = new Order("ORD-001", "Receiver", "090123", "email@test.com", "");
        order.Submit();

        // Act
        order.ReserveStock();

        // Assert
        order.Status.Should().Be(OrderStatus.StockReserved);
    }

    [Fact]
    public void Order_Cancel_Should_ChangeStatusToCancelled_When_Called()
    {
        // Arrange
        var order = new Order("ORD-001", "Receiver", "090123", "email@test.com", "");

        // Act
        order.Cancel();

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("S")] // Too short
    public void Order_Should_ThrowArgumentException_When_OrderCodeIsInvalid(string invalidCode)
    {
        // Act & Assert
        Action act = () => new Order(invalidCode, "Name", "090", "test@test.com", "");
        act.Should().Throw<ArgumentException>();
    }
}
