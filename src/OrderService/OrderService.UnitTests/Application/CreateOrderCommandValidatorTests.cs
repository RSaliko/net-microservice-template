using FluentValidation.TestHelper;
using OrderService.Application.Features.Orders.Commands.CreateOrder;

namespace OrderService.UnitTests.Application;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator;

    public CreateOrderCommandValidatorTests()
    {
        _validator = new CreateOrderCommandValidator();
    }

    [Fact]
    public void Validator_Should_HaveError_When_OrderCodeIsEmpty()
    {
        // Arrange
        var command = new CreateOrderCommand("", "Receiver", "123", "email@test.com", "Note", new List<CreateOrderItemCommand>());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OrderCode);
    }

    [Fact]
    public void Validator_Should_HaveError_When_ItemsAreEmpty()
    {
        // Arrange
        var command = new CreateOrderCommand("ORD-123", "Receiver", "123", "email@test.com", "Note", new List<CreateOrderItemCommand>());

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Fact]
    public void Validator_Should_NotHaveError_When_CommandIsValid()
    {
        // Arrange
        var items = new List<CreateOrderItemCommand> { new(Guid.NewGuid(), 1, 10.0m) };
        var command = new CreateOrderCommand("ORD-123", "Receiver", "123", "email@test.com", "Note", items);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
