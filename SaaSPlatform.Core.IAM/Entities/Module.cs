using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.IAM.Entities;

public class Module : BaseEntity
{
    public string Name { get; set; } = default!;
    // Examples:
    // Orders
    // Billing
    // Catalog
    // Agents
}