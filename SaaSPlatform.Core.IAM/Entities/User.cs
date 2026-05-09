using SaaSPlatform.SharedKernel.Common;

namespace SaaSPlatform.Core.IAM.Entities;

public class User : BaseEntity
{
    // =========================
    // MULTI TENANT
    // =========================

    public Guid TenantId { get; set; }

    // =========================
    // BASIC INFO
    // =========================

    public string Name { get; set; } = default!;

    public string? Email { get; set; }

    public string Phone { get; set; } = default!;

    // =========================
    // SECURITY
    // =========================

    public string PasswordHash { get; set; } = default!;

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }

    // =========================
    // LOGIN SECURITY
    // =========================

    public int FailedLoginAttempts { get; set; }

    public DateTime? LockoutEnd { get; set; }

    // =========================
    // NAVIGATION
    // =========================

    public ICollection<UserRole> Roles { get; set; }
        = new List<UserRole>();

    public ICollection<RefreshToken> RefreshTokens { get; set; }
        = new List<RefreshToken>();
}