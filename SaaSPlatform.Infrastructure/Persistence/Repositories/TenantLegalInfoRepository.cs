using SaaSPlatform.Core.Tenant.Entities;
using SaaSPlatform.Core.Tenant.Interfaces;

namespace SaaSPlatform.Infrastructure.Persistence.Repositories;

public class TenantLegalInfoRepository : ITenantLegalInfoRepository
{
    private readonly SaaSPlatformDbContext _db;
    public TenantLegalInfoRepository(SaaSPlatformDbContext db) => _db = db;
    public void Add(TenantLegalInfo info) => _db.TenantLegalInfos.Add(info);
}