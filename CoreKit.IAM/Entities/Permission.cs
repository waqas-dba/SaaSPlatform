using CoreKit.SharedKernel.Common;

namespace CoreKit.IAM.Entities;

public class Permission : AuditableEntity
{
    public string Name { get; set; } = default!;
    public Guid PermissionModuleId { get; set; }
    public PermissionModule Module { get; set; } = default!;
    public ICollection<RolePermission>? RolePermissions { get; set; }
}