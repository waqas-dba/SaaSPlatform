using SaaSPlatform.SharedKernel.Common;
using TenantKit.Enums;

namespace TenantKit.Entities;

public class Tenant : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public TenantStatus Status { get; set; } = TenantStatus.Pending;
    public string? MetadataJson { get; set; }
    public ICollection<Store>? Stores { get; set; }
}