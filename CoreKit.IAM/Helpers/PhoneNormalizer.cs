using System.Text.RegularExpressions;

namespace CoreKit.IAM.Helpers;

public static class PhoneNormalizer
{
    private static readonly Regex StripNonDigits =
        new(@"[^\d]", RegexOptions.Compiled);

    /// <summary>
    /// Strips all non-digit characters and validates the length (7–15 digits).
    /// The leading '+' is removed; the caller is responsible for country code handling.
    /// </summary>
    public static string Normalize(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        var trimmed = phone.Trim();
        var digitsOnly = StripNonDigits.Replace(trimmed, "");

        if (digitsOnly.Length < 7 || digitsOnly.Length > 15)
            throw new ArgumentException(
                $"Phone number '{phone}' is not a valid length " +
                $"(got {digitsOnly.Length} digits, expected 7–15).");

        return digitsOnly;
    }

    /// <summary>
    /// Normalises a phone number and ensures it carries the given country code.
    /// </summary>
    /// <param name="phone">Raw phone input, e.g. "0300-1234567" or "+923001234567"</param>
    /// <param name="countryCode">
    ///   Digits-only country code, e.g. "92" for Pakistan.
    ///   A leading '+' is accepted and stripped automatically.
    /// </param>
    public static string Normalize(string phone, string countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
            throw new ArgumentException(
                "Country code must not be empty.", nameof(countryCode));

        // BUG FIX: strip '+' from countryCode if the caller passes "+92" instead of "92"
        var digitsOnlyCode = StripNonDigits.Replace(countryCode.Trim(), "");
        if (digitsOnlyCode.Length == 0)
            throw new ArgumentException(
                $"Country code '{countryCode}' contains no digits.", nameof(countryCode));

        var normalized = Normalize(phone);

        // If the number already starts with the country code, leave it as-is.
        // Otherwise strip a leading '0' (local trunk prefix) and prepend the code.
        // BUG FIX: previously the leading-zero strip happened unconditionally before
        // the StartsWith check, which corrupted numbers that began with the country
        // code digits but also happened to start with '0' after stripping.
        if (normalized.StartsWith(digitsOnlyCode))
            return normalized;

        if (normalized.StartsWith('0'))
            normalized = normalized[1..];

        return digitsOnlyCode + normalized;
    }
}