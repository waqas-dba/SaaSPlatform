using FluentValidation;
using AuthCoreKit.IAM.Models;
using System.Text.RegularExpressions;

namespace SaaSPlatform.Application.Validators.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        // The Login field can be phone or email depending on configuration.
        // For simplicity, we just require it to be non‑empty.
        // If you want phone‑format validation only when login mode is "phone",
        // you can inject IamOptions and check – left as an exercise.
        RuleFor(x => x.Login)
            .NotEmpty()
            .WithMessage("Login (phone or email) is required");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain number");
    }

    // These helper methods are kept for reference but are no longer used.
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