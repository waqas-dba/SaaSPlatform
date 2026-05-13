using AuthCoreKit.IAM.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.Tenant.Interfaces;
using SaaSPlatform.Core.Tenant.Models;
using SaaSPlatform.Infrastructure.Persistence;

namespace SaaSPlatform.Api.Host.Controllers.Tenant;

[ApiController]
[Route("api/tenant")]
public class StoresController : BaseApiController
{
    private readonly SaaSPlatformDbContext _db;
    private readonly ITenantStoreService _service;

    public StoresController(
        SaaSPlatformDbContext db,
        ITenantStoreService service)
    {
        _db = db;
        _service = service;
    }

    [RequiresPermission("store.view")]
    [HttpGet("store")]
    public async Task<IActionResult> GetStore()
    {
        var tenantId = TenantId ?? throw new Exception("Tenant missing");

        var store = await _db.Stores
            .Include(x => x.StoreCuisines)
            .Include(x => x.DeliveryZones)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId);

        if (store is null)
            return Fail("Store not found", status: 404);

        return Success(new
        {
            store.Id,
            store.Name,
            store.Address,
            store.IsOnline
        });
    }

    [RequiresPermission("store.update")]
    [HttpPut("store")]
    public async Task<IActionResult> Update(UpdateStoreRequest request)
    {
        var tenantId = TenantId ?? throw new Exception("Tenant missing");

        await _service.UpdateStoreAsync(tenantId, request);
        return NoContent();
    }
}