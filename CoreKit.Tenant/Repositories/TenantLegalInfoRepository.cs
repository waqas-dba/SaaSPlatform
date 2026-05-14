using CoreKit.Tenant.Entities;
using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Tenant.Repositories;

public class TenantLegalInfoRepository : ITenantLegalInfoRepository
{
    private readonly TenantDbContext _db;

    public TenantLegalInfoRepository(TenantDbContext db) => _db = db;

    public async Task<TenantLegalInfo?> GetByTenantAsync(Guid tenantId, CancellationToken ct = default)
        => await (_db.TenantLegalInfos ?? throw new InvalidOperationException("TenantLegalInfos not configured"))
            .FirstOrDefaultAsync(i => i.TenantId == tenantId, ct);

    public async Task AddOrUpdateAsync(TenantLegalInfo info, CancellationToken ct = default)
    {
        if (_db.TenantLegalInfos == null)
            throw new InvalidOperationException("TenantLegalInfos not configured");

        var existing = await _db.TenantLegalInfos
            .FirstOrDefaultAsync(i => i.TenantId == info.TenantId, ct);

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