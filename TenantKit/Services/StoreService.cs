using TenantKit.Abstractions;
using TenantKit.Entities;
using TenantKit.Interfaces;
using TenantKit.Models;

namespace TenantKit.Services;

public class StoreService : IStoreService
{
    private readonly IStoreRepository _storeRepo;
    private readonly IStoreLimitService? _storeLimitService;
    private readonly TenantKitOptions _options;

    public StoreService(IStoreRepository storeRepo, TenantKitOptions options, IStoreLimitService? storeLimitService = null)
    {
        _storeRepo = storeRepo;
        _options = options;
        _storeLimitService = storeLimitService;
    }

    public async Task<Store?> GetByIdAsync(Guid storeId) => await _storeRepo.GetByIdAsync(storeId);
    public async Task<List<Store>> GetAllByTenantAsync(Guid tenantId) => await _storeRepo.GetAllByTenantAsync(tenantId);

    public async Task<Store> CreateAsync(Guid tenantId, string name, string? type = null, string? metadataJson = null)
    {
        if (!_options.AllowMultipleStores)
        {
            var existing = await _storeRepo.GetAllByTenantAsync(tenantId);
            if (existing.Count > 0)
                throw new InvalidOperationException("Multiple stores are not allowed.");
        }

        if (_storeLimitService != null)
        {
            int? max = await _storeLimitService.GetMaxStoresAsync(tenantId);
            if (max.HasValue)
            {
                int currentCount = (await _storeRepo.GetAllByTenantAsync(tenantId)).Count;
                if (currentCount >= max.Value)
                    throw new InvalidOperationException($"Store limit reached. Your plan allows {max.Value} store(s).");
            }
        }

        var store = new Store
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = name,
            Slug = GenerateSlug(name),
            Type = type,
            MetadataJson = metadataJson
        };
        _storeRepo.Add(store);
        return store;
    }

    public async Task UpdateAsync(Guid storeId, UpdateStoreRequest request)
    {
        var store = await _storeRepo.GetByIdAsync(storeId) ?? throw new KeyNotFoundException("Store not found.");
        if (request.Name != null) store.Name = request.Name;
        if (request.Type != null) store.Type = request.Type;
        if (request.AddressLine1 != null) store.AddressLine1 = request.AddressLine1;
        if (request.AddressLine2 != null) store.AddressLine2 = request.AddressLine2;
        if (request.City != null) store.City = request.City;
        if (request.State != null) store.State = request.State;
        if (request.PostalCode != null) store.PostalCode = request.PostalCode;
        if (request.CountryCode != null) store.CountryCode = request.CountryCode;
        if (request.Latitude.HasValue) store.Latitude = request.Latitude.Value;
        if (request.Longitude.HasValue) store.Longitude = request.Longitude.Value;
        if (request.IsActive.HasValue) store.IsActive = request.IsActive.Value;
        if (request.LogoUrl != null) store.LogoUrl = request.LogoUrl;
        if (request.CoverImageUrl != null) store.CoverImageUrl = request.CoverImageUrl;
        if (request.MetadataJson != null) store.MetadataJson = request.MetadataJson;
        _storeRepo.Update(store);
    }

    public async Task DeleteAsync(Guid storeId)
    {
        var store = await _storeRepo.GetByIdAsync(storeId) ?? throw new KeyNotFoundException("Store not found.");
        _storeRepo.Delete(store);
    }

    public async Task SetMarketplaceListingAsync(Guid storeId, bool isListed)
    {
        var store = await _storeRepo.GetByIdAsync(storeId) ?? throw new KeyNotFoundException("Store not found.");
        store.IsListedOnMarketplace = isListed;
        _storeRepo.Update(store);
    }

    private static string GenerateSlug(string name)
        => name.ToLowerInvariant().Replace(" ", "-") + "-" + Guid.NewGuid().ToString("N")[..6];
}