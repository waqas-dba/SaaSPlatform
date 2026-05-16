using CoreKit.IAM.Interfaces;
using CoreKit.SharedKernel.Common;
using CoreKit.Tenant.Abstractions;
using CoreKit.Tenant.Entities;
using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Models;
using CoreKit.Tenant.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CoreKit.Tenant.Services;

public class StoreService : IStoreService
{
    private readonly TenantDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly TenantKitOptions _options;
    private readonly ICurrentUserService _currentUser;
    private readonly IStoreLimitService? _storeLimitService;

    public StoreService(
        TenantDbContext db,
        ITenantContext tenantContext,
        IOptions<TenantKitOptions> options,
        ICurrentUserService currentUser,
        IStoreLimitService? storeLimitService = null)
    {
        _db = db;
        _tenantContext = tenantContext;
        _options = options.Value;
        _currentUser = currentUser;
        _storeLimitService = storeLimitService;
    }

    public async Task<StoreDto?> GetByIdAsync(Guid storeId)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new UnauthorizedAccessException("Missing tenant");
        var storeScope = await _currentUser.GetStoreScopeAsync();

        var query = _db.Stores
            .Where(x => x.Id == storeId && x.TenantId == tenantId);

        // Apply store-level filter if not all stores
        if (!storeScope.IsAllStores)
            query = query.Where(x => storeScope.StoreIds.Contains(x.Id));

        var store = await query.FirstOrDefaultAsync();
        return store == null ? null : Map(store);
    }

    public async Task<List<StoreDto>> GetAllByTenantAsync(Guid tenantId)
    {
        var storeScope = await _currentUser.GetStoreScopeAsync();

        var query = _db.Stores.Where(s => s.TenantId == tenantId);

        // Apply store-level filter if not all stores
        if (!storeScope.IsAllStores)
            query = query.Where(s => storeScope.StoreIds.Contains(s.Id));

        var stores = await query.ToListAsync();
        return stores.Select(Map).ToList();
    }

    public async Task<StoreDto> CreateAsync(CreateStoreRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentNullException(nameof(request.Name), "Store name is required.");

        var tenantId = _tenantContext.TenantId
            ?? throw new UnauthorizedAccessException("Missing tenant");

        // ✅ Enforce store limit
        await EnforceStoreLimit(tenantId);

        var store = new Store
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Slug = GenerateSlug(request.Name),
            Type = request.Type,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            CountryCode = request.CountryCode,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            LogoUrl = request.LogoUrl,
            CoverImageUrl = request.CoverImageUrl,
            MetadataJson = request.MetadataJson,
            IsActive = true,
            IsListedOnMarketplace = false
        };

        _db.Stores.Add(store);
        await _db.SaveChangesAsync();
        return Map(store);
    }

    public async Task UpdateAsync(Guid storeId, UpdateStoreRequest request)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new UnauthorizedAccessException("Missing tenant");
        var store = await _db.Stores
            .FirstOrDefaultAsync(x => x.Id == storeId && x.TenantId == tenantId)
            ?? throw new InvalidOperationException("Store not found");

        if (request.Name != null)
        {
            store.Name = request.Name;
            store.Slug = GenerateSlug(request.Name);
        }
        if (request.Type != null) store.Type = request.Type;
        if (request.IsActive.HasValue) store.IsActive = request.IsActive.Value;
        // ... update other fields as needed ...

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid storeId)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new UnauthorizedAccessException("Missing tenant");
        var store = await _db.Stores
            .FirstOrDefaultAsync(x => x.Id == storeId && x.TenantId == tenantId)
            ?? throw new InvalidOperationException("Store not found");

        _db.Stores.Remove(store);
        await _db.SaveChangesAsync();
    }

    public async Task SetMarketplaceListingAsync(Guid storeId, bool isListed)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new UnauthorizedAccessException("Missing tenant");
        var store = await _db.Stores
            .FirstOrDefaultAsync(x => x.Id == storeId && x.TenantId == tenantId)
            ?? throw new InvalidOperationException("Store not found");

        store.IsListedOnMarketplace = isListed;
        await _db.SaveChangesAsync();
    }

    // ✅ New helper to enforce store limits
    private async Task EnforceStoreLimit(Guid tenantId)
    {
        if (!_options.AllowMultipleStores)
        {
            // Hard limit of 1 store if multiple stores are disabled
            var existing = await _db.Stores.AnyAsync(s => s.TenantId == tenantId);
            if (existing)
                throw new InvalidOperationException("This tenant is limited to one store.");
        }
        else if (_storeLimitService != null)
        {
            // Dynamic limit from subscription/plan
            var maxStores = await _storeLimitService.GetMaxStoresAsync(tenantId);
            if (maxStores.HasValue)
            {
                var count = await _db.Stores.CountAsync(s => s.TenantId == tenantId);
                if (count >= maxStores.Value)
                    throw new InvalidOperationException($"Store limit of {maxStores.Value} reached.");
            }
        }
    }

    private static StoreDto Map(Store store) => new()
    {
        Id = store.Id,
        TenantId = store.TenantId,
        Name = store.Name,
        Slug = store.Slug,
        Type = store.Type,
        IsActive = store.IsActive,
        IsListedOnMarketplace = store.IsListedOnMarketplace
    };

    private static string GenerateSlug(string name)
        => name.Trim().ToLower().Replace(" ", "-");
}