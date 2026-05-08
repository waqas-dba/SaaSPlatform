// SaaSPlatform.Api.Host/Controllers/Tenant/StoresController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Api.Host.Authorization;
using SaaSPlatform.Core.Tenant.Interfaces;
using SaaSPlatform.Core.Tenant.Models;
using SaaSPlatform.Infrastructure.Persistence;

namespace SaaSPlatform.Api.Host.Controllers.Tenant;

[Authorize]
[ApiController]
[Route("api/tenant")]
public class StoresController : ControllerBase
{
    private readonly ITenantStoreService _storeService;
    private readonly SaaSPlatformDbContext _db;

    public StoresController(ITenantStoreService storeService, SaaSPlatformDbContext db)
    {
        _storeService = storeService;
        _db = db;
    }

    [TenantPermission("store.update")]
    [HttpGet("store")]
    public async Task<IActionResult> GetStore()
    {
        var tenantId = (Guid)HttpContext.Items["TenantId"]!;
        var store = await _db.Stores
            .Include(s => s.StoreCuisines).ThenInclude(sc => sc.Cuisine)
            .Include(s => s.DeliveryZones).ThenInclude(sz => sz.Zone)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId);

        if (store is null) return NotFound();
        return Ok(new
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

    [TenantPermission("store.update")]
    [HttpPut("store")]
    public async Task<IActionResult> UpdateStore(UpdateStoreRequest request)
    {
        var tenantId = (Guid)HttpContext.Items["TenantId"]!;
        await _storeService.UpdateStoreAsync(tenantId, request);
        return NoContent();
    }
}