namespace CoreKit.IAM.Interfaces;

/// <summary>
/// Host‑provided encryption service for sensitive data.
/// </summary>
public interface IEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);

    byte[] Encrypt(byte[] plainBytes);
    byte[] Decrypt(byte[] encryptedBytes);
}