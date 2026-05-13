namespace AuthCoreKit.IAM.Entities;

/// <summary>
/// Stores sensitive identity documents (images) for a user.
/// </summary>
public class UserDocument
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DocumentType { get; set; } = default!;  // e.g. "cnic_front", "cnic_back", "passport"
    public string ImageUrl { get; set; } = default!;      // URL or base64 data
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = default!;
}