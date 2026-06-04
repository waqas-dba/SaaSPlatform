using CoreKit.Infrastructure.Controllers;
using CoreKit.SharedKernel.Tenancy;

namespace CoreKit.Infrastructure.Controllers;

public abstract class TenantApiControllerBase : ApiControllerBase
{
    protected ITenantContext TenantContext { get; }

    protected TenantApiControllerBase(ITenantContext tenantContext)
    {
        TenantContext = tenantContext;
    }

    protected Guid RequireTenantId() =>
        TenantContext.TenantId ?? throw new UnauthorizedAccessException("Tenant context is missing.");
}