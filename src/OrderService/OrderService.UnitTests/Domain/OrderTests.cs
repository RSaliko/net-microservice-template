using FluentAssertions;
using OrderService.Domain.Entities;
using OrderService.Domain.ValueObjects;

namespace OrderService.UnitTests.Domain;

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

        // Assert (AAA Pattern)
        order.OrderCode.Should().Be(orderCode.ToUpperInvariant());
        order.ReceiverName.Should().Be(receiverName);
        order.Phone.Should().Be(phone);
        order.Email.Should().Be(email);
        order.Note.Should().Be(note);
        order.Address.Should().Be(address);
    }
}
