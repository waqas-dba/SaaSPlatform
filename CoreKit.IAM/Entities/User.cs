using CoreKit.SharedKernel.Common;

namespace CoreKit.IAM.Entities;

public class User : AuditableEntity
{
    public Guid? TenantId { get; set; }
    public string Name { get; set; } = default!;
    public string? Email { get; set; }
    public string Phone { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }

    public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}