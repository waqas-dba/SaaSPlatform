using CoreKit.Tenant.Models;
using FluentValidation;

namespace CoreKit.Tenant.Validators;

public class TenantRegistrationRequestValidator
    : AbstractValidator<TenantRegistrationRequest>
{
    public TenantRegistrationRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("Tenant name is required.")
            .MinimumLength(3)
                .WithMessage("Tenant name must be at least 3 characters.")
            .MaximumLength(200)
                .WithMessage("Tenant name must not exceed 200 characters.")
            .Matches(@"^[a-zA-Z0-9\s\-_]+$")
                .WithMessage(
                    "Tenant name can only contain letters, numbers, " +
                    "spaces, hyphens, and underscores.");
    }
}