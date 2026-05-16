// CoreKit.IAM | CoreKit.IAM/Entities/UserRole.cs
namespace CoreKit.IAM.Entities;

public class UserRole
{
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = default!;

    // FIX: nullable so system/global roles don't need a fake tenant GUID.
    // null  = global assignment (SuperAdmin, system-level roles)
    // value = tenant-scoped assignment
    public Guid? TenantId { get; set; }
}