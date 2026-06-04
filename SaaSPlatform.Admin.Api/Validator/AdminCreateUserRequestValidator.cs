// SaaSPlatform.Admin.api/v{version:apiVersion}/Validators/AdminCreateUserRequestValidator.cs
using FluentValidation;

namespace SaaSPlatform.Admin.Api.Validators;

public class AdminCreateUserRequestValidator : AbstractValidator<CoreKit.IAM.Models.CreateUserRequest>
{
    public AdminCreateUserRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Phone).NotEmpty().Matches(@"^\+?[\d\s\-]{7,15}$");
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("Must contain uppercase.")
            .Matches("[a-z]").WithMessage("Must contain lowercase.")
            .Matches("[0-9]").WithMessage("Must contain a digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Must contain a special character.");
    }
}