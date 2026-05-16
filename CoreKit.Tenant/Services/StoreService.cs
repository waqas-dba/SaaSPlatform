// CoreKit.Tenant | CoreKit.Tenant/Services/StoreService.cs
// FIX 4: wrap the single-store limit check in a serializable transaction
// so two concurrent requests cannot both pass the AnyAsync check and
// each create a store before the other's INSERT is visible.
using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;
using CoreKit.SharedKernel.Common;
using CoreKit.Tenant.Entities;
using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Models;
using CoreKit.Tenant.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;

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
        var tenantId = RequireTenant();
        var storeScope = await ResolveStoreScopeAsync();

        var query = _db.Stores.Where(x => x.Id == storeId && x.TenantId == tenantId);
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

    public async Task<StoreDto> CreateAsync(CreateStoreRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentNullException(nameof(request.Name),
                "Store name is required.");

        var tenantId = RequireTenant();

        // FIX 4: open a serializable transaction so the check-then-insert is
        // atomic. Two concurrent calls will serialize; the second will find
        // the first's row and throw before inserting a duplicate.
        await using var tx = await _db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable);
        try
        {
            await EnforceStoreLimitAsync(tenantId);

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
        var tenantId = RequireTenant();
        var store = await _db.Stores
            .FirstOrDefaultAsync(x => x.Id == storeId && x.TenantId == tenantId)
            ?? throw new InvalidOperationException("Store not found.");

        if (request.Name != null) { store.Name = request.Name; store.Slug = GenerateSlug(request.Name); }
        if (request.Type != null) store.Type = request.Type;
        if (request.IsActive.HasValue) store.IsActive = request.IsActive.Value;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid storeId)
    {
        var tenantId = RequireTenant();
        var store = await _db.Stores
            .FirstOrDefaultAsync(x => x.Id == storeId && x.TenantId == tenantId)
            ?? throw new InvalidOperationException("Store not found.");
        _db.Stores.Remove(store);
        await _db.SaveChangesAsync();
    }

    public async Task SetMarketplaceListingAsync(Guid storeId, bool isListed)
    {
        var tenantId = RequireTenant();
        var store = await _db.Stores
            .FirstOrDefaultAsync(x => x.Id == storeId && x.TenantId == tenantId)
            ?? throw new InvalidOperationException("Store not found.");
        store.IsListedOnMarketplace = isListed;
        await _db.SaveChangesAsync();
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private Guid RequireTenant()
        => _tenantContext.TenantId
           ?? throw new UnauthorizedAccessException("Missing tenant context.");

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
                throw new InvalidOperationException(
                    "This tenant is limited to one store.");
        }
        else if (_storeLimitService != null)
        {
            var maxStores = await _storeLimitService.GetMaxStoresAsync(tenantId);
            if (maxStores.HasValue)
            {
                var count = await _db.Stores.CountAsync(s => s.TenantId == tenantId);
                if (count >= maxStores.Value)
                    throw new InvalidOperationException(
                        $"Store limit of {maxStores.Value} reached.");
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
    {
        var slug = name.Trim().ToLowerInvariant();
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        return slug.Trim('-');
    }
}