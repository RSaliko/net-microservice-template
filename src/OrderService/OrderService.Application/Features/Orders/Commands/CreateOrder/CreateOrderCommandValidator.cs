using FluentValidation;

namespace OrderService.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(v => v.OrderCode)
            .NotEmpty().WithMessage("Order code is required.")
            .MaximumLength(32).WithMessage("Order code must not exceed 32 characters.")
            .Matches("^[A-Z0-9-]{4,32}$").WithMessage("Order code must contain only A-Z, 0-9 or '-' and be 4-32 chars.");

        RuleFor(v => v.ReceiverName)
            .NotEmpty().WithMessage("Receiver name is required.")
            .MaximumLength(200).WithMessage("Receiver name must not exceed 200 characters.");

        RuleFor(v => v.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters.");

        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(100).WithMessage("Email must not exceed 100 characters.");

        RuleFor(v => v.Note)
            .MaximumLength(1000).WithMessage("Note must not exceed 1000 characters.");

        RuleFor(v => v.Items)
            .NotEmpty().WithMessage("Order must contain at least one item.");

        RuleForEach(v => v.Items)
            .ChildRules(item =>
            {
                item.RuleFor(x => x.ProductId)
                    .NotEmpty().WithMessage("ProductId is required.");

                item.RuleFor(x => x.Quantity)
                    .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

                item.RuleFor(x => x.UnitPrice)
                    .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative.")
                    .PrecisionScale(18, 2, true).WithMessage("Unit price must have up to 18 digits and 2 decimal places.");
            });
    }
}
