using CoreKit.IAM.Models;
using FluentValidation;

namespace SaaSPlatform.Admin.Api.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
                .WithMessage("Name is required.")
            .MaximumLength(200)
                .WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty()
                .WithMessage("Phone is required.")
            .Matches(@"^\+?[\d\s\-]{7,15}$")
                .WithMessage("Phone number is not valid.");

        RuleFor(x => x.Email)
            .EmailAddress()
                .WithMessage("Email is not valid.")
            .MaximumLength(200)
                .WithMessage("Email must not exceed 200 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Password)
            .NotEmpty()
                .WithMessage("Password is required.")
            .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters.")
            .MaximumLength(100)
                .WithMessage("Password must not exceed 100 characters.")
            .Matches("[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]")
                .WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]")
                .WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]")
                .WithMessage("Password must contain at least one special character.");
    }
}