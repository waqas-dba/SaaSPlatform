using SaaSPlatform.Core.Tenant.Interfaces;

namespace SaaSPlatform.Infrastructure.Services;

public class TenantContext : ITenantContext
{
    public Guid TenantId { get; set; }
}