using CoreKit.Tenant.Abstractions;
using CoreKit.Tenant.Entities;
using CoreKit.Tenant.Interfaces;
using CoreKit.Tenant.Models;
using CoreKit.Tenant.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Tenant.Services;

/// <summary>
/// Manages store entities for the current tenant.
/// </summary>
public class StoreService : IStoreService
{
    private readonly TenantDbContext _db;
    private readonly ITenantContext _tenantContext;

    public StoreService(TenantDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<StoreDto?> GetByIdAsync(Guid storeId)
    {
        var tenantId = _tenantContext.TenantId
                       ?? throw new UnauthorizedAccessException("Missing tenant");
        var store = await _db.Stores
            .FirstOrDefaultAsync(x => x.Id == storeId && x.TenantId == tenantId);
        return store == null ? null : Map(store);
    }

    public async Task<List<StoreDto>> GetAllByTenantAsync(Guid tenantId)
    {
        var stores = await _db.Stores
            .Where(x => x.TenantId == tenantId)
            .ToListAsync();
        return stores.Select(Map).ToList();
    }

    public async Task<StoreDto> CreateAsync(CreateStoreRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentNullException(nameof(request.Name), "Store name is required.");

        var tenantId = _tenantContext.TenantId
                       ?? throw new UnauthorizedAccessException("Missing tenant");

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
        // other fields omitted for brevity; add as needed

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