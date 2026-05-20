using System.Text.RegularExpressions;

namespace CoreKit.IAM.Helpers;

public static class PhoneNormalizer
{
    // Normalizes to E.164-style digits only, e.g.:
    // "+92-300-1234567" => "923001234567"
    // "0300-123-4567"   => "03001234567"  (local, no country code)
    // "  +1 (800) 555-1234 " => "18005551234"
    private static readonly Regex StripNonDigits =
        new(@"[^\d]", RegexOptions.Compiled);

    public static string Normalize(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        var trimmed = phone.Trim();

        // Preserve leading + as a country-code marker before stripping
        var hasPlus = trimmed.StartsWith('+');
        var digitsOnly = StripNonDigits.Replace(trimmed, "");

        // If it started with + we already have the full international number
        // If it starts with 0 and has 10-11 digits it's a local Pakistani number
        // — we do NOT auto-expand to +92 here because the country code is
        // caller-supplied; we just store the normalized digit string so that
        // "+92-300-1234567" and "0923001234567" are NOT silently merged.
        // If you want auto-expansion, pass a defaultCountryCode parameter.

        if (digitsOnly.Length < 7 || digitsOnly.Length > 15)
            throw new ArgumentException(
                $"Phone number '{phone}' is not a valid length " +
                $"(got {digitsOnly.Length} digits, expected 7–15).");

        return digitsOnly;
    }

    /// <summary>
    /// Normalizes and expands a local number to E.164 using the supplied
    /// country code, e.g. Normalize("0300-1234567", "92") => "923001234567"
    /// </summary>
    public static string Normalize(string phone, string countryCode)
    {
        var normalized = Normalize(phone);

        // Strip leading 0 (trunk prefix) before prepending country code
        if (normalized.StartsWith('0'))
            normalized = normalized[1..];

        // Avoid double-prepending if already starts with country code
        if (!normalized.StartsWith(countryCode))
            normalized = countryCode + normalized;

        return normalized;
    }
}