using System.Security.Cryptography;
using System.Text;
using AuthCoreKit.IAM.Interfaces;

namespace AuthCoreKit.IAM.Services;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentNullException(nameof(key));
        }

        _key = Convert.FromBase64String(key);

        if (_key.Length != 32)
        {
            throw new ArgumentException(
                "Encryption key must be 32 bytes.");
        }
    }

    // STRING ENCRYPTION

    public string Encrypt(string plainText)
    {
        var plainBytes =
            Encoding.UTF8.GetBytes(plainText);

        var encrypted =
            Encrypt(plainBytes);

        return Convert.ToBase64String(encrypted);
    }

    public string Decrypt(string cipherText)
    {
        var encryptedBytes =
            Convert.FromBase64String(cipherText);

        var plainBytes =
            Decrypt(encryptedBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }

    // BYTE[] ENCRYPTION

    public byte[] Encrypt(byte[] plainBytes)
    {
        var nonce =
            RandomNumberGenerator.GetBytes(12);

        var cipherBytes =
            new byte[plainBytes.Length];

        var tag =
            new byte[16];

        using var aes =
            new AesGcm(_key);

        aes.Encrypt(
            nonce,
            plainBytes,
            cipherBytes,
            tag);

        var result =
            new byte[12 + cipherBytes.Length + 16];

        Buffer.BlockCopy(
            nonce,
            0,
            result,
            0,
            12);

        Buffer.BlockCopy(
            cipherBytes,
            0,
            result,
            12,
            cipherBytes.Length);

        Buffer.BlockCopy(
            tag,
            0,
            result,
            12 + cipherBytes.Length,
            16);

        return result;
    }

    public byte[] Decrypt(byte[] encryptedBytes)
    {
        var nonce = new byte[12];
        var tag = new byte[16];

        var cipherBytes =
            new byte[encryptedBytes.Length - 12 - 16];

        Buffer.BlockCopy(
            encryptedBytes,
            0,
            nonce,
            0,
            12);

        Buffer.BlockCopy(
            encryptedBytes,
            12,
            cipherBytes,
            0,
            cipherBytes.Length);

        Buffer.BlockCopy(
            encryptedBytes,
            12 + cipherBytes.Length,
            tag,
            0,
            16);

        var plainBytes =
            new byte[cipherBytes.Length];

        using var aes =
            new AesGcm(_key);

        aes.Decrypt(
            nonce,
            cipherBytes,
            tag,
            plainBytes);

        return plainBytes;
    }
}