using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AuthCoreKit.IAM.Authorization;               // RequiresPermission
using SaaSPlatform.Api.Host.Controllers;             // BaseApiController
using SaaSPlatform.Core.Tenant.Interfaces;
using SaaSPlatform.Core.Tenant.Models;
using SaaSPlatform.Infrastructure.Persistence;

namespace SaaSPlatform.Api.Host.Controllers.Tenant;

[Authorize]
[ApiController]
[Route("api/tenant")]
public class StoresController : BaseApiController
{
    private readonly ITenantStoreService _storeService;
    private readonly SaaSPlatformDbContext _db;

    public StoresController(ITenantStoreService storeService, SaaSPlatformDbContext db)
    {
        _storeService = storeService;
        _db = db;
    }

    /// <summary>
    /// Returns the store belonging to the current tenant.
    /// </summary>
    [RequiresPermission("store.update")]
    [HttpGet("store")]
    public async Task<IActionResult> GetStore()
    {
        var tenantId = (Guid)HttpContext.Items["TenantId"]!;

        var store = await _db.Stores
            .IgnoreQueryFilters()   // bypass global filter (we filter explicitly)
            .Include(s => s.StoreCuisines)
                .ThenInclude(sc => sc.Cuisine)
            .Include(s => s.DeliveryZones)
                .ThenInclude(sz => sz.Zone)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId);

        if (store is null)
            return Fail("Store not found", status: 404);

        return Success(new
        {
            store.Id,
            store.Name,
            store.Address,
            store.IsOnline,
            store.MinPreparingTime,
            store.MaxPreparingTime,
            store.CoverPhotoUrl,
            store.LogoUrl,
            store.SupportsDelivery,
            store.SupportsPickup,
            store.SupportsDineIn,
            Cuisines = store.StoreCuisines.Select(sc => sc.Cuisine.Name),
            Zones = store.DeliveryZones.Select(sz => new { sz.Zone.City, sz.Zone.Name })
        });
    }

    /// <summary>
    /// Updates the store of the current tenant.
    /// </summary>
    [RequiresPermission("store.update")]
    [HttpPut("store")]
    public async Task<IActionResult> UpdateStore(UpdateStoreRequest request)
    {
        var tenantId = (Guid)HttpContext.Items["TenantId"]!;
        await _storeService.UpdateStoreAsync(tenantId, request);
        return NoContent();
    }
}