// CoreKit.Catalog/Validators/CreateProductRequestValidator.cs
using CoreKit.Catalog.Models;
using FluentValidation;

namespace CoreKit.Catalog.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BasePrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.StoreId).NotEmpty();
    }
}