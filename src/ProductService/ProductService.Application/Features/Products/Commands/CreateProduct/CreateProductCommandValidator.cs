using FluentValidation;

namespace ProductService.Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(v => v.Sku)
            .NotEmpty().WithMessage("SKU is required.")
            .MaximumLength(32).WithMessage("SKU must not exceed 32 characters.")
            .Matches("^[A-Z0-9-]{4,32}$").WithMessage("SKU must contain only A-Z, 0-9 or '-' and be 4-32 chars.");

        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(v => v.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative.");

        RuleFor(v => v.QuantityStock)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity stock cannot be negative.");
    }
}
