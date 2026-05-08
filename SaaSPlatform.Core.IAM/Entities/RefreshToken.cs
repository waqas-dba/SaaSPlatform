using SaaSPlatform.SharedKernel.Common;

namespace SaaSPlatform.Core.IAM.Entities;

public class RefreshToken : AuditableEntity
{
    public Guid UserId { get; set; }

    public string Token { get; set; } = default!;

    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; } = false;

    public DateTime? RevokedAt { get; set; }

    public string? ReplacedByToken { get; set; }
}