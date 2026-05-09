using SaaSPlatform.SharedKernel.Common;

namespace SaaSPlatform.Core.IAM.Entities;

public class Role : BaseEntity
{
    // =========================
    // MULTI TENANT
    // =========================

    public Guid TenantId { get; set; }

    // =========================
    // BASIC INFO
    // =========================

    public string Name { get; set; } = default!;

    public string? Description { get; set; }

    public bool IsSystem { get; set; }

    // =========================
    // NAVIGATION
    // =========================

    public ICollection<UserRole> UserRoles { get; set; }
        = new List<UserRole>();

    public ICollection<RolePermission> RolePermissions { get; set; }
        = new List<RolePermission>();
}