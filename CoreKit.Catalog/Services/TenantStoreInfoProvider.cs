using CoreKit.Catalog.Abstractions;
using CoreKit.Tenant.Interfaces;

namespace CoreKit.Catalog.Services;

/// <summary>
/// Resolves store info (TenantId, StoreTypeCode) from the Tenant module's
/// IStoreRepository. This is the single canonical implementation — all API
/// projects register this type via AddCatalogModule's configureStoreInfo
/// callback instead of maintaining per-project copies.
/// </summary>
public sealed class TenantStoreInfoProvider : IStoreInfoProvider
{
    private readonly IStoreRepository _storeRepo;

    public TenantStoreInfoProvider(IStoreRepository storeRepo)
        => _storeRepo = storeRepo;

    public async Task<StoreInfo?> GetStoreInfoAsync(
        Guid storeId,
        CancellationToken ct = default)
    {
        // StoreRepository.GetByIdAsync now eagerly loads StoreType (fix #1),
        // so StoreType.Code is always populated here.
        var store = await _storeRepo.GetByIdAsync(storeId, ct);
        if (store == null) return null;

        return new StoreInfo
        {
            Id = store.Id,
            TenantId = store.TenantId,
            StoreTypeCode = store.StoreType?.Code ?? string.Empty
        };
    }
}