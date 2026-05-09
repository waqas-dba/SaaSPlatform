using FluentValidation;
using SaaSPlatform.Core.IAM.Models;
using System.Text.RegularExpressions;

namespace SaaSPlatform.Application.Validators.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Phone number is required")
            .Must(BeValidPhone)
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain number");
    }

    private bool BeValidPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        var normalized = NormalizePhone(phone);

        return Regex.IsMatch(normalized, @"^\+?[0-9]{10,15}$");
    }

    private string NormalizePhone(string phone)
    {
        return phone.Trim()
            .Replace(" ", "")
            .Replace("-", "");
    }
}