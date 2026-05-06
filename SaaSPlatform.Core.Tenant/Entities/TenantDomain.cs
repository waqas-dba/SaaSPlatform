using SaaSPlatform.Core.SharedKernel.Common;

namespace SaaSPlatform.Core.Tenant.Entities;

public class TenantDomain : AuditableEntity
{
    public Guid TenantId { get; set; }

    public string Domain { get; set; } = default!; // example: restaurant.com

    public bool IsPrimary { get; set; }
    public bool IsVerified { get; set; }

    public DateTime? VerifiedAt { get; set; }
}