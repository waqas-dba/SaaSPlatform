using Microsoft.EntityFrameworkCore;

using SaaSPlatform.Core.Tenant.Interfaces;
using SaaSPlatform.Infrastructure.Persistence;

namespace SaaSPlatform.Infrastructure.Services.TenantServices.Service;

public class TenantAccessService : ITenantAccessService
{
    private readonly SaaSPlatformDbContext _db;

    public TenantAccessService(
        SaaSPlatformDbContext db)
    {
        _db = db;
    }

    public async Task<bool> TenantExistsAsync(Guid tenantId)
    {
        return await _db.Tenants
            .AsNoTracking()
            .AnyAsync(x =>
                x.Id == tenantId &&
                x.IsActive);
    }
}