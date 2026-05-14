using CoreKit.SharedKernel.Common;
using CoreKit.Tenant.Enums;

namespace CoreKit.Tenant.Entities;

public class TenantEntity : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public TenantStatus Status { get; set; } = TenantStatus.Pending;
    public string? MetadataJson { get; set; }
    public ICollection<Store>? Stores { get; set; }
}