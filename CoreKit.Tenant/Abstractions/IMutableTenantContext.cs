
namespace CoreKit.Tenant.Services;

// FIX: Add a separate mutable interface so middleware can set the tenant
// without casting to the concrete TenantContext implementation.
// The middleware depends on IMutableTenantContext; controllers depend on ITenantContext.
public interface IMutableTenantContext : ITenantContext
{
    void SetTenant(Guid tenantId);
}
