using CoreKit.SharedKernel.Tenancy;
using CoreKit.SharedKernel.Interfaces;

namespace CoreKit.Tenant.Services;

public class TenantContext : IMutableTenantContext
{
    public Guid? TenantId { get; private set; }

    public void SetTenant(Guid? tenantId)
    {
        TenantId = tenantId == Guid.Empty ? null : tenantId;
    }
}