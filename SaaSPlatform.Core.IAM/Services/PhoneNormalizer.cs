namespace SaaSPlatform.Core.IAM.Services;

public static class PhoneNormalizer
{
    public static string Normalize(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        return phone.Trim()
            .Replace(" ", "")
            .Replace("-", "");
    }
}