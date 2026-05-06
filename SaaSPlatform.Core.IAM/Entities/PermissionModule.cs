using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.IAM.Entities;

public class PermissionModule : BaseEntity
{
    public string Name { get; set; } = default!; // Orders, Billing
    public string Code { get; set; } = default!; // orders, billing

    public ICollection<Permission>? Permissions { get; set; }
}