using CoreKit.Catalog.Abstractions;
using CoreKit.Tenant.Interfaces;

namespace SaaSPlatform.Admin.Api.Services;

public class TenantStoreInfoProvider : IStoreInfoProvider
{
    private readonly IStoreRepository _storeRepo;

    public TenantStoreInfoProvider(IStoreRepository storeRepo) => _storeRepo = storeRepo;

    public async Task<StoreInfo?> GetStoreInfoAsync(
        Guid storeId,
        CancellationToken ct = default)
    {
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