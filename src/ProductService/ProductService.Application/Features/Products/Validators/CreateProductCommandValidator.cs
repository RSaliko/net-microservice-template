using FluentValidation;
using ProductService.Application.Features.Products.Commands.CreateProduct;

namespace ProductService.Application.Features.Products.Validators;

/// <summary>
/// Validator for CreateProductCommand ensuring data integrity before processing.
/// </summary>
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(v => v.Sku)
            .NotEmpty().WithMessage("SKU is required.")
            .MaximumLength(50).WithMessage("SKU must not exceed 50 characters.");

        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(v => v.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than zero.");

        RuleFor(v => v.QuantityStock)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity in stock cannot be negative.");
    }
}
