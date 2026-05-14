using Microsoft.EntityFrameworkCore;
using TenantKit.Entities;
using TenantKit.Interfaces;

namespace TenantKit.Repositories;

public class TenantLegalInfoRepository : ITenantLegalInfoRepository
{
    private readonly ITenantKitDbContext _db;
    public TenantLegalInfoRepository(ITenantKitDbContext db) => _db = db;

    public async Task<TenantLegalInfo?> GetByTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        if (_db.TenantLegalInfos == null) return null;
        return await _db.TenantLegalInfos.FirstOrDefaultAsync(i => i.TenantId == tenantId, ct);
    }

    public async Task AddOrUpdateAsync(TenantLegalInfo info, CancellationToken ct = default)
    {
        if (_db.TenantLegalInfos == null) return;
        var existing = await _db.TenantLegalInfos.FirstOrDefaultAsync(i => i.TenantId == info.TenantId, ct);
        if (existing != null)
        {
            existing.BusinessLicenseNumber = info.BusinessLicenseNumber;
            existing.TaxId = info.TaxId;
            existing.AdditionalJson = info.AdditionalJson;
        }
        else
        {
            await _db.TenantLegalInfos.AddAsync(info, ct);
        }
        await _db.SaveChangesAsync(ct);
    }
}