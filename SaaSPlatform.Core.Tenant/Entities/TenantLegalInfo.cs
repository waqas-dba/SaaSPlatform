// SaaSPlatform.Core/Tenant/Entities/TenantLegalInfo.cs
using SaaSPlatform.SharedKernel.Common;

namespace SaaSPlatform.Core.Tenant.Entities;

public class TenantLegalInfo : AuditableEntity
{
    public Guid TenantId { get; set; }
    public string CnicNumber { get; set; } = default!;
    public string? NtnNumber { get; set; }
    public bool HasFoodLicense { get; set; }
    public string? CnicFrontImageUrl { get; set; }
    public string? CnicBackImageUrl { get; set; }
}