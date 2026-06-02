// CoreKit.Catalog/Services/TenantDbStoreInfoProvider.cs
using CoreKit.Catalog.Abstractions;
using CoreKit.Catalog.Persistence;
using CoreKit.Tenant.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Catalog.Services;

/// <summary>
/// Default IStoreInfoProvider that resolves store info by querying
/// the Tenant Store table via CatalogDbContext's ability to access
/// shared tables in the same PostgreSQL database.
///
/// This keeps the cross-module dependency explicit and in one place
/// rather than scattered across ProductService and ProductVariantService.
/// Replace with an HTTP client or message-based lookup when splitting databases.
/// </summary>
public sealed class TenantDbStoreInfoProvider : IStoreInfoProvider
{
    private readonly CatalogDbContext _db;

    public TenantDbStoreInfoProvider(CatalogDbContext db) => _db = db;

    public async Task<StoreInfo?> GetStoreInfoAsync(
        Guid storeId,
        CancellationToken ct = default)
    {
        return await _db.Set<Store>()
            .Where(s => s.Id == storeId)
            .Select(s => new StoreInfo
            {
                Id = s.Id,
                TenantId = s.TenantId,
                StoreTypeCode = s.StoreType.Code
            })
            .FirstOrDefaultAsync(ct);
    }
}