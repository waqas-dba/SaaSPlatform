using SaaSPlatform.SharedKernel.Common;

namespace SaaSPlatform.Core.IAM.Entities;

public class Permission : BaseEntity
{
    public string Name { get; set; } = default!;
    // orders.create
    // orders.view
    // catalog.update

    public Guid PermissionModuleId { get; set; }
    public PermissionModule Module { get; set; } = default!;

    public ICollection<RolePermission>? RolePermissions { get; set; }
}