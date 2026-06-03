using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;
using CoreKit.SharedKernel.Common;
using CoreKit.SharedKernel.Helpers;
using CoreKit.SharedKernel.Interfaces;
using CoreKit.SharedKernel.Tenancy;
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
    private readonly IPlanLimitProvider? _planLimit;

    public StoreService(
        TenantDbContext db,
        ITenantContext tenantContext,
        IOptions<TenantKitOptions> options,
        ICurrentUserService currentUser,
        IPlanLimitProvider? planLimit = null)
    {
        // BUG FIX: all injected dependencies were previously ignored
        _db = db;
        _tenantContext = tenantContext;
        _options = options.Value;
        _currentUser = currentUser;
        _planLimit = planLimit;
    }

    public async Task<StoreDto?> GetByIdAsync(
     Guid storeId,
     CancellationToken cancellationToken = default)
    {
        // Single scope-resolution path for all callers.
        // ResolveStoreScopeAsync already returns StoreScope.All for
        // users with store.view_all or platform.stores.view.
        var storeScope = await ResolveStoreScopeAsync();

        IQueryable<Store> query = _db.Stores.Include(x => x.StoreType);

        // Tenant filter: platform admins with All scope skip this
        if (!storeScope.IsAllStores)
        {
            var tenantId = _tenantContext.TenantId;
            if (tenantId.HasValue)
                query = query.Where(x => x.TenantId == tenantId.Value);

            query = query.Where(x => storeScope.StoreIds.Contains(x.Id));
        }

        var store = await query
            .FirstOrDefaultAsync(x => x.Id == storeId, cancellationToken);

        return store == null ? null : Map(store);
    }

    public async Task<List<StoreDto>> GetAllByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var storeScope = await ResolveStoreScopeAsync();

        IQueryable<Store> query = _db.Stores
            .Include(x => x.StoreType)
            .Where(x => x.TenantId == tenantId);

        if (!storeScope.IsAllStores)
        {
            query = query.Where(x => storeScope.StoreIds.Contains(x.Id));
        }

        var stores = await query
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return stores.Select(Map).ToList();
    }

    public async Task<List<StoreDto>> GetAllStoresAsync(
        CancellationToken cancellationToken = default)
    {
        var stores = await _db.Stores
            .Include(x => x.StoreType)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return stores.Select(Map).ToList();
    }

    public async Task<StoreDto> CreateAsync(
     CreateStoreRequest request,
     CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentNullException(nameof(request.Name), "Store name is required.");

        var tenantId = _tenantContext.TenantId
            ?? throw new UnauthorizedAccessException("Tenant context missing.");

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // -------------------------------------------------------
            // FIX: endian-safe advisory lock key
            var lockKey = LockKeyHelper.GuidToLockKey(tenantId);   // was BitConverter.ToInt64(...)
            await _db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock({0})", lockKey);
            // -------------------------------------------------------

            await EnforceStoreLimitAsync(tenantId, cancellationToken);

            var storeType = await _db.StoreTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.StoreTypeId && x.IsActive, cancellationToken)
                ?? throw new InvalidOperationException("Invalid store type.");

            var slug = await GenerateUniqueSlugAsync(request.Name, tenantId, cancellationToken);

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

            _db.Stores.Add(store);
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
        Guid storeId,
        UpdateStoreRequest request,
        CancellationToken cancellationToken = default)
    {
        Store store;

        if (_currentUser.HasPermission(Permissions.Platform.ManageAnyStore) ||
            _currentUser.HasPermission(Permissions.Store.ViewAll))
        {
            store = await _db.Stores
                .Include(x => x.StoreType)
                .FirstOrDefaultAsync(x => x.Id == storeId, cancellationToken)
                ?? throw new KeyNotFoundException("Store not found.");
        }
        else
        {
            var tenantId = _tenantContext.TenantId;
            store = await _db.Stores
                .Include(x => x.StoreType)
                .FirstOrDefaultAsync(x => x.Id == storeId && x.TenantId == tenantId, cancellationToken)
                ?? throw new KeyNotFoundException("Store not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            store.Name = request.Name.Trim();
            store.Slug = await GenerateUniqueSlugAsync(
                request.Name, store.TenantId, cancellationToken, store.Id);
        }

        if (request.StoreTypeId.HasValue)
        {
            var storeType = await _db.StoreTypes
                .FirstOrDefaultAsync(x => x.Id == request.StoreTypeId.Value && x.IsActive, cancellationToken)
                ?? throw new InvalidOperationException("Invalid store type.");

            store.StoreTypeId = request.StoreTypeId.Value;
            store.StoreType = storeType;
        }

        if (request.AddressLine1 != null) store.AddressLine1 = request.AddressLine1;
        if (request.AddressLine2 != null) store.AddressLine2 = request.AddressLine2;
        if (request.City != null) store.City = request.City;
        if (request.State != null) store.State = request.State;
        if (request.PostalCode != null) store.PostalCode = request.PostalCode;
        if (request.CountryCode != null) store.CountryCode = request.CountryCode;
        if (request.Latitude.HasValue) store.Latitude = request.Latitude;
        if (request.Longitude.HasValue) store.Longitude = request.Longitude;
        if (request.LogoUrl != null) store.LogoUrl = request.LogoUrl;
        if (request.CoverImageUrl != null) store.CoverImageUrl = request.CoverImageUrl;
        if (request.MetadataJson != null) store.MetadataJson = request.MetadataJson;
        if (request.IsActive.HasValue) store.IsActive = request.IsActive.Value;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        Guid storeId,
        CancellationToken cancellationToken = default)
    {
        Store store;

        if (_currentUser.HasPermission(Permissions.Platform.ManageAnyStore) ||
            _currentUser.HasPermission(Permissions.Store.ViewAll))
        {
            store = await _db.Stores
                .FirstOrDefaultAsync(x => x.Id == storeId, cancellationToken)
                ?? throw new KeyNotFoundException("Store not found.");
        }
        else
        {
            var tenantId = _tenantContext.TenantId;
            store = await _db.Stores
                .FirstOrDefaultAsync(x => x.Id == storeId && x.TenantId == tenantId, cancellationToken)
                ?? throw new KeyNotFoundException("Store not found.");
        }

        _db.Stores.Remove(store);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task SetMarketplaceListingAsync(
        Guid storeId,
        bool isListed,
        CancellationToken cancellationToken = default)
    {
        Store store;

        if (_currentUser.HasPermission(Permissions.Platform.ManageAnyStore) ||
            _currentUser.HasPermission(Permissions.Store.ViewAll))
        {
            store = await _db.Stores
                .FirstOrDefaultAsync(x => x.Id == storeId, cancellationToken)
                ?? throw new KeyNotFoundException("Store not found.");
        }
        else
        {
            var tenantId = _tenantContext.TenantId;
            store = await _db.Stores
                .FirstOrDefaultAsync(x => x.Id == storeId && x.TenantId == tenantId, cancellationToken)
                ?? throw new KeyNotFoundException("Store not found.");
        }

        store.IsListedOnMarketplace = isListed;
        await _db.SaveChangesAsync(cancellationToken);
    }

    // ---------------------------------------------------------------------------
    // Private helpers
    // ---------------------------------------------------------------------------

    private async Task EnforceStoreLimitAsync(Guid tenantId, CancellationToken ct)
    {
        if (_planLimit == null) return;

        var maxStores = await _planLimit.GetMaxStoresAsync(tenantId, ct);
        if (maxStores.HasValue)
        {
            var count = await _db.Stores.CountAsync(x => x.TenantId == tenantId, ct);
            if (count >= maxStores.Value)
                throw new InvalidOperationException(
                    $"Store limit of {maxStores.Value} reached. Upgrade your plan.");
        }
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

            var exists = await query.AnyAsync(cancellationToken);
            if (!exists) return slug;

            slug = $"{baseSlug}-{counter++}";
        }
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