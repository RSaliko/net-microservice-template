using FluentValidation;
using OrderService.Application.Features.Orders.Commands.CreateOrder;

namespace OrderService.Application.Features.Orders.Validators;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.OrderCode)
            .NotEmpty().WithMessage("Order code is required.")
            .MaximumLength(50).WithMessage("Order code must not exceed 50 characters.");

        RuleFor(x => x.ReceiverName)
            .NotEmpty().WithMessage("Receiver name is required.")
            .MaximumLength(100).WithMessage("Receiver name must not exceed 100 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?[0-9]{10,15}$").WithMessage("Invalid phone number format.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item.");

        RuleForEach(x => x.Items).SetValidator(new CreateOrderItemCommandValidator());
    }
}

public class CreateOrderItemCommandValidator : AbstractValidator<CreateOrderItemCommand>
{
    public CreateOrderItemCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative.");
    }
}
