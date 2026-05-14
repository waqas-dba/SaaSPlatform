namespace CoreKit.IAM.Entities;

/// <summary>
/// Stores sensitive identity data (encrypted CNIC number) for a user.
/// Only one record per user.
/// </summary>
public class UserIdentity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string EncryptedCnic { get; set; } = default!;    // AES encrypted CNIC number
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = default!;
}