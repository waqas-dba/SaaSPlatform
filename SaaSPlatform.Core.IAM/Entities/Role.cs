using SaaSPlatform.SharedKernel.Common;

namespace SaaSPlatform.Core.IAM.Entities;

public class Role : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; set; }

    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public bool IsSystem { get; set; } = false;

    public ICollection<UserRole>? UserRoles { get; set; }
    public ICollection<RolePermission>? RolePermissions { get; set; }
}