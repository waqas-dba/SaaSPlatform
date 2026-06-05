// CoreKit.Tenant/Services/StoreService.cs
using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;
using CoreKit.SharedKernel.Common;
using CoreKit.SharedKernel.Exceptions;
using CoreKit.SharedKernel.Helpers;
using CoreKit.SharedKernel.Interfaces;
using CoreKit.SharedKernel.Tenancy;
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
    private readonly IStoreRepository _storeRepo;          // FIX: was missing
    private readonly ITenantContext _tenantContext;
    private readonly TenantKitOptions _options;
    private readonly ICurrentUserService _currentUser;
    private readonly IPlanLimitProvider? _planLimit;

    public StoreService(
        TenantDbContext db,
        IStoreRepository storeRepo,                        // FIX: inject repo
        ITenantContext tenantContext,
        IOptions<TenantKitOptions> options,
        ICurrentUserService currentUser,
        IPlanLimitProvider? planLimit = null)
    {
        _db = db;
        _storeRepo = storeRepo;
        _tenantContext = tenantContext;
        _options = options.Value;
        _currentUser = currentUser;
        _planLimit = planLimit;
    }

    public async Task<StoreDto?> GetByIdAsync(
        Guid storeId, CancellationToken cancellationToken = default)
    {
        var storeScope = await ResolveStoreScopeAsync();
        var store = await _storeRepo.GetByIdAsync(storeId, cancellationToken);

        if (store is null) return null;

        if (!storeScope.IsAllStores)
        {
            if (_tenantContext.TenantId.HasValue &&
                store.TenantId != _tenantContext.TenantId.Value)
                return null;

            if (!storeScope.StoreIds.Contains(store.Id))
                return null;
        }

        return Map(store);
    }

    public async Task<List<StoreDto>> GetAllByTenantAsync(
        Guid tenantId, CancellationToken cancellationToken = default)
    {
        var storeScope = await ResolveStoreScopeAsync();

        var stores = await _storeRepo.GetAllByTenantAsync(tenantId, cancellationToken);

        if (!storeScope.IsAllStores)
            stores = stores
                .Where(s => storeScope.StoreIds.Contains(s.Id))
                .ToList();

        return stores.OrderBy(s => s.Name).Select(Map).ToList();
    }

    public async Task<List<StoreDto>> GetAllStoresAsync(
        CancellationToken cancellationToken = default)
    {
        if (!_currentUser.HasPermission(Permissions.Platform.ViewAllStores) &&
            !_currentUser.HasPermission(Permissions.Platform.ManageAnyStore))
            throw new ForbiddenException(
                "Access to all stores across tenants requires a platform-level permission.");

        var stores = await _db.Stores
            .Include(x => x.StoreType)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return stores.Select(Map).ToList();
    }

    internal async Task<List<StoreDto>> GetAllStoresInternalAsync(
        CancellationToken cancellationToken = default)
    {
        var stores = await _db.Stores
            .Include(x => x.StoreType)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return stores.Select(Map).ToList();
    }

    public async Task<StoreDto> CreateAsync(
        CreateStoreRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentNullException(nameof(request.Name), "Store name is required.");

        var tenantId = _tenantContext.TenantId
            ?? throw new ForbiddenException("Tenant context missing.");

        await using var transaction =
            await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var lockKey = LockKeyHelper.GuidToLockKey(tenantId);
            await _db.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock({0})", lockKey);

            await EnforceStoreLimitAsync(tenantId, cancellationToken);

            var storeType = await _db.StoreTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == request.StoreTypeId && x.IsActive, cancellationToken)
                ?? throw new InvalidOperationException("Invalid store type.");

            var slug = await GenerateUniqueSlugAsync(
                request.Name, tenantId, cancellationToken);

            var store = new Store
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = request.Name.Trim(),
                Slug = slug,
                StoreTypeId = request.StoreTypeId,
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

            _storeRepo.Add(store);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            store.StoreType = storeType;
            return Map(store);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task UpdateAsync(
        Guid storeId, UpdateStoreRequest request,
        CancellationToken cancellationToken = default)
    {
        var store = await ResolveStoreWithAccessCheckAsync(storeId, cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            store.Name = request.Name.Trim();
            store.Slug = await GenerateUniqueSlugAsync(
                request.Name, store.TenantId, cancellationToken, store.Id);
        }

        if (request.StoreTypeId.HasValue)
        {
            var storeType = await _db.StoreTypes
                .FirstOrDefaultAsync(
                    x => x.Id == request.StoreTypeId.Value && x.IsActive,
                    cancellationToken)
                ?? throw new InvalidOperationException("Invalid store type.");

            store.StoreTypeId = request.StoreTypeId.Value;
            store.StoreType = storeType;
        }

        ApplyAddressUpdates(store, request);
        if (request.IsActive.HasValue) store.IsActive = request.IsActive.Value;

        _storeRepo.Update(store);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Guid storeId, CancellationToken cancellationToken = default)
    {
        var store = await ResolveStoreWithAccessCheckAsync(storeId, cancellationToken);
        _storeRepo.Delete(store);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task SetMarketplaceListingAsync(
        Guid storeId, bool isListed, CancellationToken cancellationToken = default)
    {
        var store = await ResolveStoreWithAccessCheckAsync(storeId, cancellationToken);
        store.IsListedOnMarketplace = isListed;
        _storeRepo.Update(store);
        await _db.SaveChangesAsync(cancellationToken);
    }

    // ── private helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Single place for "find store + enforce tenant/platform access" —
    /// replaces the three identical blocks that existed before.
    /// </summary>
    private async Task<Store> ResolveStoreWithAccessCheckAsync(
        Guid storeId, CancellationToken ct)
    {
        var isPlatformUser =
            _currentUser.HasPermission(Permissions.Platform.ManageAnyStore) ||
            _currentUser.HasPermission(Permissions.Store.ViewAll);

        var store = await _storeRepo.GetByIdAsync(storeId, ct)
            ?? throw new KeyNotFoundException("Store not found.");

        if (!isPlatformUser &&
            _tenantContext.TenantId.HasValue &&
            store.TenantId != _tenantContext.TenantId.Value)
            throw new KeyNotFoundException("Store not found.");

        return store;
    }

    private async Task EnforceStoreLimitAsync(Guid tenantId, CancellationToken ct)
    {
        if (_planLimit is null) return;

        var maxStores = await _planLimit.GetMaxStoresAsync(tenantId, ct);
        if (!maxStores.HasValue) return;

        var count = await _db.Stores.CountAsync(x => x.TenantId == tenantId, ct);
        if (count >= maxStores.Value)
            throw new InvalidOperationException(
                $"Store limit of {maxStores.Value} reached. Upgrade your plan.");
    }

    private async Task<StoreScope> ResolveStoreScopeAsync()
    {
        if (_currentUser.HasPermission(Permissions.Store.ViewAll) ||
            _currentUser.HasPermission(Permissions.Platform.ViewAllStores))
            return StoreScope.All;

        return await _currentUser.GetStoreScopeAsync();
    }

    private async Task<string> GenerateUniqueSlugAsync(
        string name,
        Guid tenantId,
        CancellationToken cancellationToken = default,
        Guid? excludeStoreId = null)
    {
        var baseSlug = SlugHelper.Generate(name);
        var slug = baseSlug;
        var counter = 1;

        while (true)
        {
            IQueryable<Store> query = _db.Stores
                .Where(x => x.TenantId == tenantId && x.Slug == slug);

            if (excludeStoreId.HasValue)
                query = query.Where(x => x.Id != excludeStoreId.Value);

            if (!await query.AnyAsync(cancellationToken))
                return slug;

            slug = $"{baseSlug}-{counter++}";
        }
    }

    private static void ApplyAddressUpdates(Store store, UpdateStoreRequest r)
    {
        if (r.AddressLine1 != null) store.AddressLine1 = r.AddressLine1;
        if (r.AddressLine2 != null) store.AddressLine2 = r.AddressLine2;
        if (r.City != null) store.City = r.City;
        if (r.State != null) store.State = r.State;
        if (r.PostalCode != null) store.PostalCode = r.PostalCode;
        if (r.CountryCode != null) store.CountryCode = r.CountryCode;
        if (r.Latitude.HasValue) store.Latitude = r.Latitude;
        if (r.Longitude.HasValue) store.Longitude = r.Longitude;
        if (r.LogoUrl != null) store.LogoUrl = r.LogoUrl;
        if (r.CoverImageUrl != null) store.CoverImageUrl = r.CoverImageUrl;
        if (r.MetadataJson != null) store.MetadataJson = r.MetadataJson;
    }

    private static StoreDto Map(Store store) => new()
    {
        Id = store.Id,
        TenantId = store.TenantId,
        Name = store.Name,
        Slug = store.Slug,
        StoreTypeId = store.StoreTypeId,
        StoreTypeName = store.StoreType?.Name ?? string.Empty,
        StoreCategory = store.StoreType?.Category ?? string.Empty,
        IsActive = store.IsActive,
        IsListedOnMarketplace = store.IsListedOnMarketplace
    };
}