using SaaSPlatform.SharedKernel.Common;

namespace SaaSPlatform.Core.IAM.Entities;

public class RefreshToken : AuditableEntity
{
    public Guid UserId { get; set; }

    public User? User { get; set; }

    // Actual refresh token
    public string Token { get; set; } = default!;

    // JWT Id linked to access token
    public string JwtId { get; set; } = default!;

    // Expiry
    public DateTime ExpiresAtUtc { get; set; }

    // Revocation
    public bool IsRevoked { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    // Rotation
    public string? ReplacedByToken { get; set; }

    // Security tracking
    public string? CreatedByIp { get; set; }

    public string? RevokedByIp { get; set; }

    public string? Device { get; set; }

    public string? UserAgent { get; set; }

    // Optional session tracking
    public Guid? SessionId { get; set; }

    // Convenience properties
    public bool IsExpired =>
        DateTime.UtcNow >= ExpiresAtUtc;

    public bool IsActive =>
        !IsRevoked && !IsExpired;
}