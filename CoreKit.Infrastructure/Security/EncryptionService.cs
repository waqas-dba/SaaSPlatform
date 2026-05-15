using System.Security.Cryptography;
using System.Text;
using CoreKit.IAM.Interfaces;

namespace CoreKit.Infrastructure.Security;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(string base64Key)
    {
        _key = Convert.FromBase64String(base64Key);
    }

    public string Encrypt(string plainText)
    {
        var bytes = Encrypt(Encoding.UTF8.GetBytes(plainText));
        return Convert.ToBase64String(bytes);
    }

    public string Decrypt(string cipherText)
    {
        var bytes = Convert.FromBase64String(cipherText);
        return Encoding.UTF8.GetString(Decrypt(bytes));
    }

    public byte[] Encrypt(byte[] plainBytes)
    {
        var nonce = RandomNumberGenerator.GetBytes(12);
        var cipher = new byte[plainBytes.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(_key);
        aes.Encrypt(nonce, plainBytes, cipher, tag);

        return nonce.Concat(cipher).Concat(tag).ToArray();
    }

    public byte[] Decrypt(byte[] encryptedBytes)
    {
        var nonce = encryptedBytes[..12];
        var tag = encryptedBytes[^16..];
        var cipher = encryptedBytes[12..^16];

        var plain = new byte[cipher.Length];

        using var aes = new AesGcm(_key);
        aes.Decrypt(nonce, cipher, tag, plain);

        return plain;
    }
}