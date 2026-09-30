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

        RuleFor(x => x.PreparationTimeMinutes)
            .InclusiveBetween(1, 600)
            .When(x => x.PreparationTimeMinutes.HasValue);

        RuleForEach(x => x.Variants).ChildRules(v =>
        {
            v.RuleFor(i => i.Name).NotEmpty().MaximumLength(100);
            v.RuleFor(i => i.Price).GreaterThanOrEqualTo(0);
        });
    }
}

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public class CreateAddonGroupRequestValidator : AbstractValidator<CreateAddonGroupRequest>
{
    public CreateAddonGroupRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MinSelect).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxSelect).GreaterThanOrEqualTo(1);
        RuleFor(x => x).Must(x => x.MinSelect <= x.MaxSelect)
            .WithMessage("MinSelect cannot be greater than MaxSelect.");

        RuleForEach(x => x.Addons).ChildRules(a =>
        {
            a.RuleFor(i => i.Name).NotEmpty().MaximumLength(200);
            a.RuleFor(i => i.AdditionalPrice).GreaterThanOrEqualTo(0);
        });
    }
}