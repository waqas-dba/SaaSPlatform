// CoreKit.Infrastructure/Controllers/TenantApiControllerBase.cs
using CoreKit.SharedKernel.Exceptions;
using CoreKit.SharedKernel.Tenancy;

namespace CoreKit.Infrastructure.Controllers;

public abstract class TenantApiControllerBase : ApiControllerBase
{
    protected ITenantContext TenantContext { get; }

    protected TenantApiControllerBase(ITenantContext tenantContext)
    {
        TenantContext = tenantContext;
    }

    // FIX: was throwing UnauthorizedAccessException (401) for an authenticated
    // user missing tenant context — correct code is 403 Forbidden
    protected Guid RequireTenantId() =>
        TenantContext.TenantId
        ?? throw new ForbiddenException(
            "A tenant context is required to perform this action. " +
            "Ensure the X-Tenant-Id header is present.");
}