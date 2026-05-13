namespace AuthCoreKit.IAM.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    // 🔐 hashed token (NOT raw token)
    public string TokenHash { get; set; } = default!;

    // 🔗 rotation family (JWT theft detection core)
    public string FamilyId { get; set; } = default!;

    // JWT ID binding
    public string JwtId { get; set; } = default!;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? RevokedAtUtc { get; set; }

    public bool IsRevoked { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public string? CreatedByIp { get; set; }

    public string? RevokedByIp { get; set; }

    public string? UserAgent { get; set; }

    // helper
    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAtUtc;
}