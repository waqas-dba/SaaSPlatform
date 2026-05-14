using CoreKit.SharedKernel.Common;

namespace CoreKit.IAM.Entities;

public class PermissionModule : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public ICollection<Permission>? Permissions { get; set; }
}