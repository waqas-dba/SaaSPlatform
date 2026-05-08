// SaaSPlatform.Infrastructure/Services/Tenant/TenantStoreService.cs
using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.Tenant.Entities;
using SaaSPlatform.Core.Tenant.Interfaces;
using SaaSPlatform.Core.Tenant.Models;
using SaaSPlatform.Infrastructure.Persistence;

namespace SaaSPlatform.Infrastructure.Services.TenantServices.Service;

public class TenantStoreService : ITenantStoreService
{
    private readonly SaaSPlatformDbContext _db;

    public TenantStoreService(SaaSPlatformDbContext db)
    {
        _db = db;
    }

    public async Task UpdateStoreAsync(Guid tenantId, UpdateStoreRequest request)
    {
        var store = await _db.Stores
            .Include(s => s.StoreCuisines)
            .Include(s => s.DeliveryZones)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId);

        if (store is null)
            throw new InvalidOperationException("Store not found for this tenant.");

        // Scalars
        if (request.Name != null) store.Name = request.Name;
        if (request.Address != null) store.Address = request.Address;
        if (request.MinPreparingTime.HasValue) store.MinPreparingTime = request.MinPreparingTime.Value;
        if (request.MaxPreparingTime.HasValue) store.MaxPreparingTime = request.MaxPreparingTime.Value;
        if (request.IsOnline.HasValue) store.IsOnline = request.IsOnline.Value;
        if (request.SupportsDelivery.HasValue) store.SupportsDelivery = request.SupportsDelivery.Value;
        if (request.SupportsPickup.HasValue) store.SupportsPickup = request.SupportsPickup.Value;
        if (request.SupportsDineIn.HasValue) store.SupportsDineIn = request.SupportsDineIn.Value;
        if (request.CoverPhotoUrl != null) store.CoverPhotoUrl = request.CoverPhotoUrl;
        if (request.LogoUrl != null) store.LogoUrl = request.LogoUrl;

        // Cuisines – full replacement
        if (request.CuisineIds != null)
        {
            _db.StoreCuisines.RemoveRange(store.StoreCuisines);
            foreach (var cuisineId in request.CuisineIds)
                _db.StoreCuisines.Add(new StoreCuisine { StoreId = store.Id, CuisineId = cuisineId });
        }

        // Delivery zones – full replacement
        if (request.ZoneIds != null)
        {
            _db.StoreDeliveryZones.RemoveRange(store.DeliveryZones);
            foreach (var zoneId in request.ZoneIds)
                _db.StoreDeliveryZones.Add(new StoreDeliveryZone { StoreId = store.Id, ZoneId = zoneId });
        }

        await _db.SaveChangesAsync();
    }
}