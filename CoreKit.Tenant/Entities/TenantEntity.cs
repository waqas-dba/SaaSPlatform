using CoreKit.SharedKernel.Common;
using CoreKit.Tenant.Enums;

namespace CoreKit.Tenant.Entities;

/// <summary>
/// Represents a tenant (customer) in the SaaS platform.
/// </summary>
public class TenantEntity : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public TenantStatus Status { get; set; } = TenantStatus.Pending;

    /// <summary>
    /// The user who owns this tenant. Nullable to allow public registrations.
    /// </summary>
    public Guid? OwnerUserId { get; set; }

    public string? MetadataJson { get; set; }
    public ICollection<Store>? Stores { get; set; }
}