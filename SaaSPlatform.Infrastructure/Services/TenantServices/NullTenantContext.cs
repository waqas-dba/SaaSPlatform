using SaaSPlatform.Core.Tenant.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SaaSPlatform.Infrastructure.Services.TenantServices;

public class NullTenantContext : ITenantContext
{
    public Guid? TenantId => Guid.Empty;
    public void SetTenantId(Guid tenantId) { }
}
