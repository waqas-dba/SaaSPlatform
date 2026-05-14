using CoreKit.Tenant.Abstractions;

namespace CoreKit.Tenant.Services;

public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; internal set; }
}