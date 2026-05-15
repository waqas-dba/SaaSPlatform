using System.Security.Cryptography;
using System.Text;

namespace CoreKit.IAM.Helpers;

/// <summary>
/// Provides a consistent SHA‑256 hashing method for tokens.
/// </summary>
public static class TokenHasher
{
    public static string Hash(string token)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}