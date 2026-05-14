using AuthCoreKit.IAM.Authorization;
using Microsoft.AspNetCore.Mvc;
using TenantKit.Interfaces;
using TenantKit.Models;

namespace SaaSPlatform.Api.Host.Controllers.Stores;

[ApiController]
[Route("api/stores")]
public class StoresController : BaseApiController
{
    private readonly IStoreService _storeService;

    public StoresController(IStoreService storeService) => _storeService = storeService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (TenantId is null) return Fail("Missing tenant context");
        var stores = await _storeService.GetAllByTenantAsync(TenantId.Value);
        return Success(stores);
    }

    [HttpPost]
    public async Task<IActionResult> Create(UpdateStoreRequest request)
    {
        if (TenantId is null) return Fail("Missing tenant context");
        var store = await _storeService.CreateAsync(TenantId.Value, request.Name!, request.Type, request.MetadataJson);
        return Success(store);
    }

    [HttpPut("{storeId}")]
    public async Task<IActionResult> Update(Guid storeId, UpdateStoreRequest request)
    {
        await _storeService.UpdateAsync(storeId, request);
        return NoContent();
    }

    [HttpDelete("{storeId}")]
    public async Task<IActionResult> Delete(Guid storeId)
    {
        await _storeService.DeleteAsync(storeId);
        return NoContent();
    }

    // Admin endpoint
    [HttpPut("{storeId}/marketplace")]
    [RequiresPermission("store.marketplace")]
    public async Task<IActionResult> SetMarketplaceListing(Guid storeId, bool isListed)
    {
        await _storeService.SetMarketplaceListingAsync(storeId, isListed);
        return NoContent();
    }
}