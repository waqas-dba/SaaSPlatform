using SaaSPlatform.Core.IAM.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace SaaSPlatform.Infrastructure.Services.IAM;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            saltSize: 16,
            iterations: 100_000,
            HashAlgorithmName.SHA256);

        var salt = pbkdf2.Salt;
        var hash = pbkdf2.GetBytes(32);

        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string passwordHash)
    {
        var parts = passwordHash.Split(':');
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var expectedHash = Convert.FromBase64String(parts[1]);

        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            100_000,
            HashAlgorithmName.SHA256);

        var actualHash = pbkdf2.GetBytes(32);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}