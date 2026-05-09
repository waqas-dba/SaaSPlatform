using SaaSPlatform.SharedKernel.Common;

namespace SaaSPlatform.Core.IAM.Entities;

public class User : BaseEntity
{
    // 🔑 Multi-tenant isolation (CRITICAL)
    public Guid TenantId { get; set; }

    public string Name { get; set; } = default!;

    // Optional (kept nullable properly)
    public string? Email {  get; set; }

    // Phone login identity (PRIMARY LOGIN KEY)
    public string Phone { get; set; } = default!;

    public string PasswordHash { get; set; } = default!;

    public bool IsActive { get; set; } = true;

    public DateTime? LastLoginAt { get; set; }

    // Security hardening (IMPORTANT)
    public int FailedLoginAttempts { get; set; } = 0;

    public DateTime? LockoutEnd { get; set; }

    // Navigation
    public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
}