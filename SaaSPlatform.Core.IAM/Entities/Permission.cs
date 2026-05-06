using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.IAM.Entities;

public class Permission : BaseEntity
{
    public string Name { get; set; } = default!;

    // Examples:
    // orders.create
    // orders.view
    // billing.manage
    // catalog.delete

    public Guid ModuleId { get; set; }
}