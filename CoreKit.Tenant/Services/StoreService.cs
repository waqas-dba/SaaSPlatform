using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;
using CoreKit.SharedKernel.Common;
using CoreKit.SharedKernel.Helpers;
using CoreKit.Tenant.Entities;
using CoreKit.Tenant.Enums;
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
        var storeScope = await ResolveStoreScopeAsync();

        var query = _currentUser.IsSuperAdmin
            ? _db.Stores.Where(x => x.Id == storeId)
            : _db.Stores.Where(x =>
                x.Id == storeId &&
                x.TenantId == _tenantContext.TenantId);

        if (!storeScope.IsAllStores)
            query = query.Where(x => storeScope.StoreIds.Contains(x.Id));

        var store = await query.FirstOrDefaultAsync();
        return store == null ? null : Map(store);
    }

    public async Task<List<StoreDto>> GetAllByTenantAsync(Guid tenantId)
    {
        var storeScope = await ResolveStoreScopeAsync();
        var query = _db.Stores.Where(s => s.TenantId == tenantId);

        if (!storeScope.IsAllStores)
            query = query.Where(s => storeScope.StoreIds.Contains(s.Id));

        return (await query.ToListAsync()).Select(Map).ToList();
    }

    public async Task<List<StoreDto>> GetAllStoresAsync()
        => (await _db.Stores.ToListAsync()).Select(Map).ToList();

    public async Task<StoreDto> CreateAsync(CreateStoreRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentNullException(nameof(request.Name), "Store name is required.");

        var tenantId = _tenantContext.TenantId
            ?? throw new UnauthorizedAccessException(
                "Provide x-tenant-id header to specify which tenant to create the store under.");

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var lockKey = BitConverter.ToInt64(tenantId.ToByteArray(), 0);
            await _db.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock({0})", lockKey);

            await EnforceStoreLimitAsync(tenantId);

            // HIGH FIX — generate a unique slug; append a numeric suffix on collision
            var slug = await GenerateUniqueSlugAsync(request.Name, tenantId);

            var store = new Store
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = request.Name,
                Slug = slug,
                Type = request.Type ?? StoreType.Other,
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
                IsActive = request.IsActive ?? true,
                IsPrimary = false,
                IsListedOnMarketplace = false
            };

            _db.Stores.Add(store);
            await _db.SaveChangesAsync();
            await tx.CommitAsync();
            return Map(store);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateAsync(Guid storeId, UpdateStoreRequest request)
    {
        var store = _currentUser.IsSuperAdmin
            ? await _db.Stores.FirstOrDefaultAsync(x => x.Id == storeId)
                ?? throw new KeyNotFoundException("Store not found.")
            : await _db.Stores.FirstOrDefaultAsync(x =>
                x.Id == storeId &&
                x.TenantId == _tenantContext.TenantId)
                ?? throw new KeyNotFoundException("Store not found.");

        if (request.Name != null)
        {
            store.Name = request.Name;
            store.Slug = await GenerateUniqueSlugAsync(request.Name, store.TenantId, storeId);
        }

        if (request.Type.HasValue) store.Type = request.Type.Value;
        if (request.AddressLine1 != null) store.AddressLine1 = request.AddressLine1;
        if (request.AddressLine2 != null) store.AddressLine2 = request.AddressLine2;
        if (request.City != null) store.City = request.City;
        if (request.State != null) store.State = request.State;
        if (request.PostalCode != null) store.PostalCode = request.PostalCode;
        if (request.CountryCode != null) store.CountryCode = request.CountryCode;
        if (request.Latitude.HasValue) store.Latitude = request.Latitude.Value;
        if (request.Longitude.HasValue) store.Longitude = request.Longitude.Value;
        if (request.LogoUrl != null) store.LogoUrl = request.LogoUrl;
        if (request.CoverImageUrl != null) store.CoverImageUrl = request.CoverImageUrl;
        if (request.IsActive.HasValue) store.IsActive = request.IsActive.Value;
        if (request.MetadataJson != null) store.MetadataJson = request.MetadataJson;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid storeId)
    {
        var store = _currentUser.IsSuperAdmin
            ? await _db.Stores.FirstOrDefaultAsync(x => x.Id == storeId)
                ?? throw new KeyNotFoundException("Store not found.")
            : await _db.Stores.FirstOrDefaultAsync(x =>
                x.Id == storeId &&
                x.TenantId == _tenantContext.TenantId)
                ?? throw new KeyNotFoundException("Store not found.");

        _db.Stores.Remove(store);
        await _db.SaveChangesAsync();
    }

    public async Task SetMarketplaceListingAsync(Guid storeId, bool isListed)
    {
        var store = _currentUser.IsSuperAdmin
            ? await _db.Stores.FirstOrDefaultAsync(x => x.Id == storeId)
                ?? throw new KeyNotFoundException("Store not found.")
            : await _db.Stores.FirstOrDefaultAsync(x =>
                x.Id == storeId &&
                x.TenantId == _tenantContext.TenantId)
                ?? throw new KeyNotFoundException("Store not found.");

        store.IsListedOnMarketplace = isListed;
        await _db.SaveChangesAsync();
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private async Task<StoreScope> ResolveStoreScopeAsync()
    {
        if (_currentUser.IsSuperAdmin ||
            _currentUser.HasPermission(Permissions.Store.ViewAll))
            return StoreScope.All;

        return await _currentUser.GetStoreScopeAsync();
    }

    private async Task EnforceStoreLimitAsync(Guid tenantId)
    {
        if (!_options.AllowMultipleStores)
        {
            if (await _db.Stores.AnyAsync(s => s.TenantId == tenantId))
                throw new InvalidOperationException("This tenant is limited to one store.");
            return;
        }

        if (_storeLimitService == null) return;

        var maxStores = await _storeLimitService.GetMaxStoresAsync(tenantId);
        if (!maxStores.HasValue) return;

        var count = await _db.Stores.CountAsync(s => s.TenantId == tenantId);
        if (count >= maxStores.Value)
            throw new InvalidOperationException(
                $"Store limit of {maxStores.Value} reached.");
    }

    /// <summary>
    /// HIGH FIX — generates a slug and appends a numeric suffix if it
    /// already exists for this tenant, ensuring DB uniqueness constraint
    /// is never violated by duplicate names.
    /// excludeStoreId is passed on update so the store doesn't conflict with itself.
    /// </summary>
    private async Task<string> GenerateUniqueSlugAsync(
        string name, Guid tenantId, Guid? excludeStoreId = null)
    {
        var baseSlug = SlugHelper.Generate(name);
        var candidate = baseSlug;
        var counter = 1;

        while (true)
        {
            var query = _db.Stores.Where(s =>
                s.TenantId == tenantId &&
                s.Slug == candidate);

            if (excludeStoreId.HasValue)
                query = query.Where(s => s.Id != excludeStoreId.Value);

            if (!await query.AnyAsync())
                return candidate;

            candidate = $"{baseSlug}-{++counter}";
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
}